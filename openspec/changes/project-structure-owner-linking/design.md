# Design: Project Structure & Email-Based Owner→Unit Linking

> Path note: OpenSpec store lives at repo-root `openspec/`. Source under `microservices/src/`. This design is the HOW for the proposal in the same folder (`proposal.md`). Approach 3 from exploration; decisions are locked in the proposal's "Decisions / Assumptions" section and are referenced here as D1–D6.

## 0. Architecture Approach

We extend the EXISTING transactional-outbox → RabbitMQ topic-exchange pipeline (built in `analytics-event-driven`, ADR-1..ADR-8) rather than introducing any new integration style. No service gains a synchronous runtime dependency on another. Every cross-service interaction is an event over the durable topic exchange `iobuild.domain.events`.

Reused infrastructure (verbatim, no changes to the contract):
- `IoBuild.Shared/Infrastructure/Messaging/RabbitMqDomainEventPublisher.cs` — singleton publisher, topic exchange, `event-type` header, persistent delivery, publisher confirms.
- `IoBuild.Shared/Infrastructure/Messaging/DomainEventPublishingExtensions.cs` — `AddDomainEventPublishing(IConfiguration)`, the keyed Polly resilience pipeline (`OutboxResiliencePipelineKey`), `BuildResiliencePipeline()`.
- `IoBuild.Shared/Domain/Model/Events/DomainEvent.cs` — base record: `EventId`, `OccurredOn`, abstract `RoutingKey`.
- Per-service `OutboxMessage` entity + `IOutboxMessageRepository` + `OutboxWorker` (Projects/Devices/Subscriptions all have an identical copy — IAM will get the same).
- Consumer pattern: `IoBuild.Analytics/Infrastructure/Messaging/AnalyticsEventConsumer.cs` (`BackgroundService`, per-message DI scope, manual ack, LWW idempotency, transient-vs-poison nack).

The change introduces, net-new: an IAM publisher (outbox+worker), a Projects consumer, a Devices consumer, and a queryable IAM-mirror table inside Projects. It is intentionally large; delivery is via chained/stacked PRs (Section 10).

### System data flow (prose diagram)

```
Builder ──POST /projects/{id}/structure──> Projects.AppDbContext (units + outbox, one CompleteAsync)
                                              │
   Projects OutboxWorker ──► iobuild.domain.events (topic)
        emits: UnitCreatedEvent       (rk project.unit.created)
               FloorStructureDefinedEvent (rk project.floor.defined)  [1 per floor]
                                              │
            ┌─────────────────────────────────┼───────────────────────────────┐
            ▼                                 ▼                                 ▼
  Analytics (analytics.read-model)   Devices (devices.provisioning)   (no consumer for unit.created besides Analytics)
   project.# / device.# / iam.#       project.floor.defined
   → UnitProjection, DeviceProjection  → seed FloorDeviceDefaults (idempotent)
                                        → emits DeviceCreatedEvent (rk device.created)

Owner ──POST /iam/auth/sign-up──> IAM.ApplicationDbContext (user + outbox, one CompleteAsync)
   IAM OutboxWorker ──► emits UserRegisteredEvent (rk iam.user.registered)
                                              │
            ┌─────────────────────────────────┴─────────────┐
            ▼                                                ▼
  Projects (projects.owner-linking)                 Analytics (binds iam.#) [optional, see §5]
   binds iam.user.#
   → upsert registered_owner mirror row
   → match Unit.OwnerEmail (case-insensitive), set OwnerId
   → emits UnitOwnerMatchedEvent (rk project.unit.owner-matched)
                                              │
                                              ▼
                                   Analytics → UnitProjection.OwnerUserId
```

Two orderings are both handled by the Projects owner-linking consumer + the `registered_owner` mirror (D6). See Section 3.

---

## 1. Domain Model Changes — Projects

### 1.1 `Unit` aggregate (`IoBuild.Projects/Domain/Model/Aggregates/Unit.cs`)

Current shape: `Id`, `ProjectId`, `UnitNumber`, `OwnerId:int` (non-null).

Target shape:

```csharp
public class Unit
{
    public int Id { get; private set; }
    public int ProjectId { get; private set; }
    public int Floor { get; private set; }            // NEW
    public string RoomNumber { get; private set; }    // NEW
    public string UnitNumber { get; private set; }     // KEPT — derived, see §1.2
    public string? OwnerEmail { get; private set; }    // NEW — IAM linking attribute (D2)
    public int? OwnerId { get; private set; }          // CHANGED int → int? (D3)

    protected Unit() { }

    // Structure-definition ctor (owner unknown at creation)
    public Unit(int projectId, int floor, string roomNumber, string? ownerEmail)
    {
        ProjectId = projectId;
        Floor = floor;
        RoomNumber = roomNumber;
        UnitNumber = ComposeUnitNumber(floor, roomNumber);
        OwnerEmail = ownerEmail;
        OwnerId = null;
    }

    public void LinkOwner(int ownerId) => OwnerId = ownerId;          // owner-matching consumer
    public void AssignOwnerEmail(string? email) => OwnerEmail = email; // unit-first edit path

    public static string ComposeUnitNumber(int floor, string room) => $"{floor}-{room}";
}
```

Keep the legacy `(projectId, unitNumber, ownerId)` constructor ONLY if `CreateUnitCommand` is still wired; preferred is to retire single-unit creation in favor of the structure command. Decision below.

### 1.2 `UnitNumber` — derived, kept (DECISION)

KEEP `UnitNumber` as a persisted column, DERIVED at construction via `ComposeUnitNumber(Floor, RoomNumber)` → `"{Floor}-{RoomNumber}"` (e.g. floor 5, room "02" → `"5-02"`).
Rationale: (a) existing read paths, REST resources, and Analytics already expose `UnitNumber`; dropping it is a wider breaking change with no benefit; (b) it remains a human-friendly label; (c) storing it (not computing on read) keeps EF/query simplicity and lets seed rows keep their historical labels. It is NOT a uniqueness key — uniqueness is `(ProjectId, Floor, RoomNumber)` (see EF config).

### 1.3 Define-structure command + handler

New command `DefineProjectStructureCommand` under `Domain/Services/Commands/Projects/`:

```csharp
public record RoomSpec(string RoomNumber, string? OwnerEmail);
public record FloorSpec(int Floor, IReadOnlyList<RoomSpec> Rooms);
public record DefineProjectStructureCommand(int ProjectId, IReadOnlyList<FloorSpec> Floors);
```

Supports BOTH UX shapes from D1: a uniform `unitsPerFloor` count is expanded by the REST layer into explicit `RoomSpec` lists (`"01".."NN"`) before reaching the command, so the domain only ever sees explicit rooms. Keeps the handler single-path.

Handler `IProjectStructureCommandService.Handle(DefineProjectStructureCommand)` (new service, or a method on a new `ProjectStructureCommandService`):

1. Load `Project` by `ProjectId` (guard: not found → `KeyNotFoundException`).
2. For each `FloorSpec`, for each `RoomSpec`: `new Unit(projectId, floor, room, ownerEmail)`, `await _unitRepository.AddAsync(unit)`.
3. `await _unitOfWork.CompleteAsync()` — single transaction creates ALL units + the outbox rows. **The outbox events for each unit are built AFTER CompleteAsync** (Section 7 bug fix) so `unit.Id` is real.
4. After commit, in a SECOND pass build and persist outbox messages:
   - one `UnitCreatedEvent` per unit (with real `Id`, `Floor`, `RoomNumber`, `OwnerEmail`),
   - one `FloorStructureDefinedEvent` per distinct floor (`ProjectId`, `Floor`, `UnitCount`, `BuilderId`).
   Then a SECOND `CompleteAsync()` to persist the outbox rows.

   > Trade-off: this is two commits (units, then outbox). The alternative — one commit — reintroduces the `Id==0` bug. We accept two commits: if the process crashes between them, units exist without events; an idempotent backfill (mirror of `OutboxBackfill.RunAsync`, already present in Projects) re-emits on next startup. This matches the existing `OutboxBackfill` safety net and is the cleanest fix for the identity problem. Document this explicitly.

5. Optionally set `Project.TotalUnits = sum(rooms)` via a new `Project.SetStructure(totalUnits)` method and emit `ProjectUpdatedEvent` — keeps Analytics' `TotalUnits` correct. Recommended.

REST: `POST /api/v1/projects/{id}/structure` in the Projects controller, maps resource → `DefineProjectStructureCommand`. Builder-authorized (same JWT middleware as other Projects write endpoints).

`CreateUnitCommand` (single-unit) — KEEP for backward compatibility but route it through the same `Unit(projectId, floor, room, ownerEmail)` ctor; callers that still pass only `unitNumber` get `Floor=0`, `RoomNumber=unitNumber`. Mark as legacy in code comments. This preserves the existing SpecFlow path and the `OutboxWriteInTransactionTests`.

### 1.4 EF config (`AppDbContext.OnModelCreating`)

Extend the `Unit` entity block:

```csharp
modelBuilder.Entity<Unit>(entity =>
{
    entity.HasKey(u => u.Id);
    entity.Property(u => u.UnitNumber).IsRequired().HasMaxLength(50);
    entity.Property(u => u.Floor);
    entity.Property(u => u.RoomNumber).IsRequired().HasMaxLength(20);
    entity.Property(u => u.OwnerEmail).HasMaxLength(255);   // nullable by CLR type
    entity.Property(u => u.OwnerId);                         // now nullable
    entity.HasIndex(u => new { u.ProjectId, u.Floor, u.RoomNumber }).IsUnique();
    entity.HasIndex(u => u.OwnerEmail);                      // owner-matching lookup
    entity.HasOne<Project>().WithMany().HasForeignKey(u => u.ProjectId);
});
```

Note: MySQL/Pomelo default collation is case-insensitive (`utf8mb4_general_ci` / `_0900_ai_ci`), so the `OwnerEmail` equality match is case-insensitive at the DB level. We ALSO normalize emails to lower-case on write (both in `AssignOwnerEmail` and in the consumer query) so the EF-InMemory test provider (which is case-sensitive and ordinal) behaves identically. See Section 3.4.

New `registered_owner` mirror entity (Section 3.3) is configured in the same `OnModelCreating`.

### 1.5 New Projects migration

`dotnet ef migrations add AddUnitStructureAndOwnerLinking -p IoBuild.Projects`. Generated migration must:
- `AddColumn floor int NOT NULL DEFAULT 0` on `units`.
- `AddColumn room_number varchar(20) NOT NULL DEFAULT ''` on `units`.
- `AddColumn owner_email varchar(255) NULL` on `units`.
- `AlterColumn owner_id int NULL` (was NOT NULL) on `units`.
- create `registered_owner` table (Section 3.3).
- create unique index `(project_id, floor, room_number)` and index `(owner_email)`.

Additive except the `owner_id` nullability relax, which is a safe widening. Down-migration reverses (requires no NULL `owner_id` rows to exist before narrowing — acceptable for rollback).

### 1.6 Seed-data reconciliation (`ProjectsSeedData.cs`)

Existing seed units (Ids 1–5) currently have `UnitNumber` like `"A-501"` and `OwnerId` 2/3/4 with NO floor/room/email. Reconcile so `HasData` stays valid (every shadow/required column needs a value):
- Add `Floor` (derive from the existing label, e.g. `A-501` → floor 5; or assign sensible floors 1..N) and `RoomNumber` (e.g. `"01"`).
- Keep historical `UnitNumber` labels as-is (D: UnitNumber is a stored label; seed rows are grandfathered and need not match `ComposeUnitNumber`).
- Set `OwnerEmail = null` (these were created pre-linking; no IAM identity).
- `OwnerId`: keep the existing int (2/3/4) as the nullable value — these point at seeded IAM users and remain valid. Do NOT null them; they represent already-linked owners.
- `registered_owner`: optionally seed rows for the seeded IAM users so the unit-first match path works in demos.

> Important: `HasData` requires ALL non-nullable new columns to have explicit values in every seed row, or `ef migrations add` will fail. The migration author MUST update all 5 unit seed rows.

---

## 2. IAM Eventing (net-new publisher)

IAM currently publishes NOTHING. We mirror the Projects/Devices outbox stack verbatim. This is the single biggest scope item (D5) and gets its own PR slice.

### 2.1 Files to add in `IoBuild.IAM`

| New file | Mirror of |
|---|---|
| `Domain/Model/Entities/OutboxMessage.cs` | `IoBuild.Projects/Domain/Model/Entities/OutboxMessage.cs` (copy exactly) |
| `Domain/Repositories/IOutboxMessageRepository.cs` | Projects' interface (`AddAsync`, `GetPendingAsync`, `UpdateAsync`) |
| `Infrastructure/Persistence/EFC/Repositories/OutboxMessageRepository.cs` | Projects' repo |
| `Workers/OutboxWorker.cs` | `IoBuild.Projects/Workers/OutboxWorker.cs` — only the `EventTypeMap` changes (just `UserRegisteredEvent`) |
| `Infrastructure/Persistence/.../OutboxBackfill.cs` | optional, mirrors Projects/Devices backfill (re-emit `UserRegisteredEvent` for seeded users so the demo links) |

### 2.2 DbContext + migration

In `ApplicationDbContext`:
- add `public DbSet<OutboxMessage> OutboxMessages { get; set; }`.
- configure the entity identically to Projects' outbox block (key, `EventType` maxlen 100, `Payload` longtext, `Status` default "Pending", `EventId`, `CreatedAt`, index `(Status, CreatedAt)`).

Migration: `dotnet ef migrations add AddOutbox -p IoBuild.IAM` → creates the `outbox_message` table only.

### 2.3 `UserCommandService.Handle(SignUpCommand)`

Inject `IOutboxMessageRepository`. After `userRepository.AddAsync(user)` and BEFORE/around `CompleteAsync`, write the event so it commits in the SAME unit of work:

```csharp
public async Task Handle(SignUpCommand command)
{
    if (userRepository.ExistsByEmail(command.Email))
        throw new InvalidOperationException("A user with this email already exists.");

    var user = new User(command.Email, hashingService.HashPassword(command.Password), command.Role);
    await userRepository.AddAsync(user);
    await unitOfWork.CompleteAsync();        // (1) user committed → user.Id is real

    var evt = new UserRegisteredEvent
    {
        UserId = user.Id,                    // real id (D-bugfix applied proactively)
        Email = command.Email.ToLowerInvariant(),
        Role = command.Role
    };
    await outboxRepository.AddAsync(new OutboxMessage(nameof(UserRegisteredEvent),
        JsonSerializer.Serialize(evt)) { EventId = evt.EventId });
    await unitOfWork.CompleteAsync();        // (2) outbox committed
}
```

> Note: `User.Id` is `public int Id { get; }` (no setter) — EF sets it via the backing field on insert; it is 0 until `CompleteAsync`. Hence the two-phase commit (mirrors the Projects fix in Section 7). If a single-transaction guarantee is required, an alternative is to capture the id with `DbContext.SaveChanges` interception, but two-phase + backfill is consistent with the rest of the codebase — use it.

### 2.4 IAM `Program.cs` wiring

Add (mirroring `Devices/Program.cs` lines 88–96): register `IOutboxMessageRepository`, `AddDomainEventPublishing(builder.Configuration)`, `AddHostedService<OutboxWorker>()`, and run `OutboxBackfill` after `db.Database.Migrate()`. IAM must adopt the same `RabbitMq:ConnectionString` config key and the pinned RabbitMQ.Client NuGet version.

---

## 3. Projects as a Consumer (net-new) — owner-linking

New `BackgroundService`: `IoBuild.Projects/Infrastructure/Messaging/OwnerLinkingConsumer.cs`, structurally identical to `AnalyticsEventConsumer` (per-message DI scope resolving `AppDbContext`, manual ack, transient-vs-poison nack, internal test constructor taking a direct `AppDbContext`).

### 3.1 Topology

```
Exchange : iobuild.domain.events (topic, durable)
Queue    : projects.owner-linking (durable)
Binding  : iam.user.#            (only IAM user events)
```

DI: new `OwnerLinkingConsumerExtensions.AddOwnerLinkingConsumer(...)` → `AddHostedService<OwnerLinkingConsumer>()`, registered in `Projects/Program.cs`.

### 3.2 Handling `UserRegisteredEvent` (registration-first ordering)

On `UserRegisteredEvent` with `Role == "owner"` (case-insensitive compare):
1. Upsert a `registered_owner` mirror row (Section 3.3) — records that this email now has a known `UserId`.
2. Query units: `Units.Where(u => u.OwnerEmail == evt.Email.ToLower() && u.OwnerId == null)`.
3. For each match: `unit.LinkOwner(evt.UserId)`.
4. For each linked unit, write a `UnitOwnerMatchedEvent` to the Projects outbox (same `AppDbContext`, same `CompleteAsync`).
5. `CompleteAsync` — units + mirror + outbox in one transaction.

Non-owner roles (builder): still upsert the mirror row? No — only owners link to units. We only persist `registered_owner` rows for `role == owner` to keep the table focused. (Builders are matched to projects by `BuilderId`, out of scope here.)

### 3.3 The `registered_owner` mirror (handles unit-first ordering) — DECISION

We pick the **queryable mirror** option from the task (D6, both directions covered), NOT the "v1 only covers register-after-assign" fallback. Reason: the unit-first path (builder types an owner email for a unit AFTER that owner already registered) is a realistic and graded scenario; the mirror is cheap and removes the ordering gap.

New entity `RegisteredOwner` in Projects (a local read-model of IAM identity, owned by Projects, populated only from events — never a runtime call to IAM):

```csharp
public class RegisteredOwner
{
    public string Email { get; private set; }   // PK, lower-cased
    public int UserId { get; private set; }
    public DateTime LastEventAt { get; private set; }  // LWW guard
}
```

EF: `HasKey(Email)`, `Email` maxlen 255. Table `registered_owner`.

Unit-first match path: when a `Unit.OwnerEmail` is assigned/changed (via the structure command OR a future edit endpoint), the command handler — AFTER persisting the unit — looks up `RegisteredOwner` by the email; if found and `unit.OwnerId == null`, it sets `OwnerId` immediately and emits `UnitOwnerMatchedEvent`. This closes the loop without waiting for another IAM event.

So matching fires from BOTH triggers:
- IAM event arrives → scan units by email (registration-first).
- Unit email assigned → scan `registered_owner` by email (unit-first).

### 3.4 Case-insensitivity contract

All emails are stored and compared **lower-cased**:
- IAM emits `Email` already lower-cased (Section 2.3).
- `Unit.AssignOwnerEmail` and the structure command lower-case before persisting.
- `RegisteredOwner.Email` is lower-cased.
This makes MySQL (CI collation) and EF-InMemory (ordinal) agree, so tests are deterministic. Document as an invariant.

### 3.5 `UnitOwnerMatchedEvent` flow

Emitted by Projects (outbox → `OutboxWorker`, add `UnitOwnerMatchedEvent` to the `EventTypeMap`). Routing key `project.unit.owner-matched`. Consumed by Analytics to backfill `UnitProjection.OwnerUserId`.

---

## 4. Devices Provisioning (net-new consumer)

New `BackgroundService`: `IoBuild.Devices/Infrastructure/Messaging/FloorProvisioningConsumer.cs`, mirroring `AnalyticsEventConsumer`.

### 4.1 Topology

```
Queue   : devices.provisioning (durable)
Binding : project.floor.defined
```

### 4.2 Default device set (D4)

`FloorDeviceDefaults` static constant in Devices:

```csharp
public static readonly IReadOnlyList<(string Type, string NamePrefix)> Defaults = new[]
{
    ("SmartMeter",   "Smart Meter"),
    ("WaterSensor",  "Water Sensor"),
    ("SmokeDetector","Smoke Detector"),
};
```

On `FloorStructureDefinedEvent(ProjectId, Floor, UnitCount, BuilderId)`: for each default, create a `Device` with `Location = $"Floor {Floor}"`, `FloorNumber = Floor`, `Status = "Active"`, generated MAC. Persist via the existing `IDeviceRepository` + outbox (emit one `DeviceCreatedEvent` per device, with the `Id==0` fix from Section 7 applied — build event after SaveChanges).

### 4.3 Idempotency guard — DECISION

Redelivery (RabbitMQ at-least-once) must NOT create duplicate floor devices. Use a **processed-event ledger** keyed by the natural provisioning key, NOT only a column check, because the three devices are written together:

Option chosen: a unique constraint `(ProjectId, Floor, Type)` on `devices` PLUS a guard query at the top of the handler:

```
if (await repo.ExistsByProjectFloorType(evt.ProjectId, evt.Floor, anyDefaultType)) return; // already provisioned → ack, no-op
```

The unique index is the hard backstop (a racing duplicate insert throws `DbUpdateException` → caught as transient OR treated as already-done and acked). The pre-check avoids the common path throwing. This mirrors the LWW/idempotency philosophy already in `AnalyticsEventConsumer`. Simpler than a separate `processed_events` table and sufficient because `(ProjectId, Floor, Type)` is a stable natural key for "this floor's default set".

> If the team prefers an explicit ledger (to also dedupe by `EventId`), add a `processed_floor_event(project_id, floor)` table written in the same transaction as the devices. Either is acceptable; the unique-index approach is the lighter recommendation.

### 4.4 `Device` aggregate + migration

Add `FloorNumber:int?` (and `UnitId:int?` — recommended, supports future per-unit device placement and Analytics `DeviceProjection.UnitId`):

```csharp
public int? FloorNumber { get; private set; }   // NEW
public int? UnitId { get; private set; }         // NEW (optional but recommended)
```

Extend the `Device` ctor/`Update` to accept these (nullable, default null). EF config: map both columns nullable; add unique index `(ProjectId, FloorNumber, Type)` for the idempotency guard. Migration `dotnet ef migrations add AddDeviceFloorPlacement -p IoBuild.Devices` (additive columns + unique index). `DeviceCreatedEvent` gains `FloorNumber?` (Section 6).

---

## 5. Analytics

### 5.1 Read-model migration

`dotnet ef migrations add AddUnitFloorAndOwnerEmailProjections -p IoBuild.Analytics`:
- `unit_projections`: add `floor int NULL`, `room_number varchar(20) NULL`, `owner_email varchar(255) NULL`.
- `device_projections`: add `floor_number int NULL`.

Fields on the projection classes:

```csharp
// UnitProjection
public int? Floor { get; set; }
public string? RoomNumber { get; set; }
public string? OwnerEmail { get; set; }
// DeviceProjection
public int? FloorNumber { get; set; }
```

### 5.2 Consumer changes (`AnalyticsEventConsumer`)

- `UpsertUnitAsync(UnitCreatedEvent)` reads new fields: `row.Floor`, `row.RoomNumber`, `row.OwnerEmail` (and existing `OwnerUserId` stays from `evt.OwnerUserId`).
- `UpsertDeviceAsync(DeviceCreatedEvent/UpdatedEvent)` reads `row.FloorNumber = evt.FloorNumber`.
- Add `case nameof(UnitOwnerMatchedEvent)` to BOTH `ApplyEventByTypeAsync` (production) and `ApplyEventWithDb` (test switch). Handler `UpsertUnitOwnerAsync(UnitOwnerMatchedEvent)`: `FindAsync(evt.UnitId)`, set `OwnerUserId = evt.OwnerUserId` with the LWW `OccurredOn` guard; create the projection row if absent (out-of-order safety — matched event could arrive before created event).

### 5.3 Bindings

Add a binding so Analytics receives owner-match and IAM events:
- `QueueBind(analytics.read-model, exchange, "project.#")` — already present, covers `project.unit.owner-matched`. So `UnitOwnerMatchedEvent` is covered by the EXISTING `project.#` binding. No new binding needed for it.
- Optional: bind `iam.user.#` ONLY if Analytics wants its own owner registry; not required for the owner-matched flow. **Recommendation: do NOT bind `iam.#` in Analytics** — owner linkage reaches Analytics via `UnitOwnerMatchedEvent` (already `project.#`). Keeps Analytics' responsibilities unchanged.

So Analytics needs ZERO new bindings — only new event cases and projection columns. Good for slice size.

---

## 6. Event Contracts (`IoBuild.Shared/Domain/Model/Events`)

### 6.1 New records

```csharp
public record UserRegisteredEvent : DomainEvent
{
    public int UserId { get; init; }
    public string Email { get; init; } = string.Empty;   // lower-cased
    public string Role { get; init; } = string.Empty;
    public override string RoutingKey => "iam.user.registered";
}

public record UnitOwnerMatchedEvent : DomainEvent
{
    public int UnitId { get; init; }
    public int ProjectId { get; init; }
    public int OwnerUserId { get; init; }
    public string OwnerEmail { get; init; } = string.Empty;
    public override string RoutingKey => "project.unit.owner-matched";
}

public record FloorStructureDefinedEvent : DomainEvent
{
    public int ProjectId { get; init; }
    public int Floor { get; init; }
    public int UnitCount { get; init; }
    public int BuilderId { get; init; }
    public override string RoutingKey => "project.floor.defined";
}
```

### 6.2 Changed records

```csharp
// UnitCreatedEvent — add:
public int Floor { get; init; }
public string RoomNumber { get; init; } = string.Empty;
public string? OwnerEmail { get; init; }
// (existing UnitId, ProjectId, BuilderUserId, OwnerUserId?, Status, RoutingKey "project.unit.created" unchanged)

// DeviceCreatedEvent — add:
public int? FloorNumber { get; init; }
```

Adding `init` properties to records is backward-compatible for JSON deserialization (old payloads simply default the new fields). No consumer breaks.

### 6.3 Routing-key & binding topology (authoritative table)

| Event | Routing key | Published by | Bound by (queue) |
|---|---|---|---|
| `UnitCreatedEvent` | `project.unit.created` | Projects | `analytics.read-model` (`project.#`) |
| `UnitOwnerMatchedEvent` | `project.unit.owner-matched` | Projects | `analytics.read-model` (`project.#`) |
| `FloorStructureDefinedEvent` | `project.floor.defined` | Projects | `devices.provisioning` (`project.floor.defined`) |
| `UserRegisteredEvent` | `iam.user.registered` | IAM | `projects.owner-linking` (`iam.user.#`) |
| `DeviceCreatedEvent` | `device.created` | Devices | `analytics.read-model` (`device.#`) |

Each consumer must register its routing type in its `OutboxWorker.EventTypeMap` (publisher side) — Projects worker adds `FloorStructureDefinedEvent` and `UnitOwnerMatchedEvent`; IAM worker adds `UserRegisteredEvent`.

---

## 7. The `unit.Id == 0` Outbox Bug Fix

### 7.1 Bug (`UnitCommandService.Handle(CreateUnitCommand)`)

Lines 33–65: the `UnitCreatedEvent` is built with `UnitId = unit.Id` BEFORE `_unitOfWork.CompleteAsync()`. Because `Id` is a DB identity column, it is `0` at that point — the outbox payload carries `UnitId=0`, corrupting the Analytics `UnitProjection` PK (D-bugfix in proposal risk table).

### 7.2 Fix

Reorder to build/serialize the event AFTER the aggregate is persisted:

```csharp
await _repository.AddAsync(unit);
await _unitOfWork.CompleteAsync();          // (1) unit.Id now real

var parentProject = await _projectRepository.FindByIdAsync(command.ProjectId);
var evt = new UnitCreatedEvent
{
    UnitId = unit.Id,                       // real id
    ProjectId = unit.ProjectId,
    BuilderUserId = parentProject?.BuilderId ?? 0,
    OwnerUserId = unit.OwnerId,             // now nullable, fine
    Floor = unit.Floor,
    RoomNumber = unit.RoomNumber,
    OwnerEmail = unit.OwnerEmail,
    Status = "Active"
};
await _outboxRepository.AddAsync(new OutboxMessage(nameof(UnitCreatedEvent),
    JsonSerializer.Serialize(evt)) { EventId = evt.EventId });
await _unitOfWork.CompleteAsync();          // (2) outbox row
```

Same two-phase pattern as the structure handler (Section 1.3) and IAM (Section 2.3). The existing `OutboxWriteInTransactionTests.Handle_CreateUnit_*` asserts "AddAsync called once, CompleteAsync called once" — that test must be UPDATED to expect `CompleteAsync` twice (or refactored to assert the outbox row carries a non-zero `UnitId` against a real EF-InMemory context — preferred, see Section 9).

### 7.3 Same latent bug in `DeviceCommandService`

`DeviceCommandService.Handle(CreateDeviceCommand)` lines 28–50 has the IDENTICAL pattern (`DeviceId = device.Id` before `SaveChangesAsync`) — the code comment even admits it ("Device.Id is 0 until SaveChanges"). It is currently masked because Analytics upserts by `DeviceId` and 0 just collides. With floor provisioning creating multiple devices, a `DeviceId=0` collision becomes real. Apply the SAME two-phase fix in `DeviceCommandService` as part of the Devices slice. Note it explicitly in tasks.

---

## 8. Idempotency & Ordering

Reuse existing semantics; no new patterns.

| Flow | Idempotency mechanism | Ordering safety |
|---|---|---|
| `UnitCreated` → Analytics | Upsert by `UnitId` PK + `OccurredOn` LWW (existing) | Out-of-order tolerated by LWW |
| `UserRegistered` → Projects link | Match only `OwnerId == null` units; re-delivery re-links to same id (idempotent set) + `registered_owner` LWW | Mirror covers unit-first; unit scan covers registration-first |
| `UnitOwnerMatched` → Analytics | Upsert `OwnerUserId` by `UnitId` + LWW; creates row if absent | If it arrives before `UnitCreated`, projection row is created then enriched by the later created event (LWW keeps newest) |
| `FloorStructureDefined` → Devices | Unique `(ProjectId, Floor, Type)` + pre-check guard | Re-delivery is a no-op (already provisioned) |
| `DeviceCreated` → Analytics | Upsert by `DeviceId` PK + LWW (existing) | Out-of-order tolerated |

Failure/redelivery (all consumers): success → `BasicAck`; transient (`DbUpdateException`/`TimeoutException`) → `BasicNack(requeue:true)`; poison/unknown → `BasicNack(requeue:false)` + log. The `DbUpdateException` from a unique-constraint hit on floor provisioning is treated as "already provisioned" → `BasicAck` (catch it specifically before the transient branch), so a duplicate delivery does not loop. Publisher side keeps the Polly circuit-breaker + RetryCount outbox behavior unchanged.

---

## 9. Test Strategy (Strict TDD)

### 9.1 Existing infra reality

The Projects test project is NOT purely shallow SpecFlow stubs — it ALREADY has a working EF-InMemory pattern:
- `OutboxMessageRepositoryPersistenceTests` uses the **three-context pattern** (`DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(name)`, separate write/worker/read contexts) for REAL EF persistence assertions.
- `OutboxWriteInTransactionTests` uses Moq for command-service unit tests.

So the prerequisite is NOT "build EF-InMemory from scratch" — it is to **promote the existing in-memory pattern into a shared fixture** and extend command/consumer tests onto real EF.

### 9.2 Shared fixture (prerequisite task)

Add `IoBuild.Projects.Tests/Infrastructure/ProjectsDbFixture.cs`:
- `static AppDbContext NewContext(string dbName)` building an InMemory `AppDbContext` (mirrors the existing `BuildContext` helper, hoisted for reuse).
- Helpers to seed Projects/Units/RegisteredOwner rows.
- Caveat to document: EF-InMemory does NOT enforce unique indexes and is ordinal/case-sensitive. Therefore (a) uniqueness `(ProjectId,Floor,RoomNumber)` and `(ProjectId,Floor,Type)` must ALSO be asserted via explicit pre-check logic in tests (not relying on the provider), and (b) the lower-casing email invariant (Section 3.4) is what makes case-insensitive matching testable on InMemory. For true unique-constraint behavior, a **SQLite-in-memory** context is the higher-fidelity option — recommend SQLite-in-memory for the consumer idempotency/uniqueness tests, InMemory for the simpler upsert tests. (Devices/Analytics already have `ConsumerIdempotencyTests` to mirror.)

### 9.3 Test seams per component

| Component | Seam | Tests |
|---|---|---|
| `Unit` aggregate | pure ctor/methods | `ComposeUnitNumber`, nullable `OwnerId`, `LinkOwner`, `AssignOwnerEmail` lower-cases |
| `DefineProjectStructureCommand` handler | EF-InMemory `AppDbContext` + real repos | creates N units (floors×rooms), one outbox `UnitCreatedEvent` per unit with NON-ZERO id, one `FloorStructureDefinedEvent` per floor |
| `unit.Id==0` fix | EF-InMemory | outbox payload `UnitId != 0` after handle (replaces the Moq `CompleteAsync` count assert) |
| `OwnerLinkingConsumer` | internal ctor with direct `AppDbContext` (mirror `AnalyticsEventConsumer`) | registration-first match; unit-first via `registered_owner`; case-insensitive; only `OwnerId==null` units; emits `UnitOwnerMatchedEvent`; redelivery idempotent |
| IAM `SignUp` outbox | EF-InMemory `ApplicationDbContext` | one `UserRegisteredEvent` row with real `UserId`, lower-cased email |
| IAM `OutboxWorker` | mirror `OutboxWorkerPublishTests` (Devices) | publishes pending, marks Processed |
| `FloorProvisioningConsumer` | SQLite-in-memory (for unique constraint) | seeds 3 devices/floor; redelivery → no duplicates; `FloorNumber` set |
| `DeviceCommandService` fix | EF-InMemory | `DeviceCreatedEvent` payload `DeviceId != 0` |
| Analytics new cases | existing `AnalyticsEventConsumer` internal ctor | `UnitOwnerMatchedEvent` sets `OwnerUserId`; `UnitCreatedEvent` sets floor/room/email; out-of-order matched-before-created |
| Shared events | serialization round-trip | new/changed records serialize with new fields; routing keys correct |

Each component is RED-first: write the failing test against the new shape, then implement. Test runner: `dotnet test` (solution-wide), per Strict TDD.

---

## 10. Migration & Rollout Ordering — CHAINED/STACKED PRs

This change touches 5 services + Shared + a net-new IAM outbox under Strict TDD. It will FAR exceed a 400-line single PR. **Deliver as chained/stacked PRs.** Recommended slice boundaries (each independently buildable, testable, revertible; ordered by dependency):

1. **Projects test-infra fixture + `unit.Id==0` fix** (`ProjectsDbFixture`, promote InMemory pattern, fix `UnitCommandService`, update its test). No schema change. Foundation for all later Projects tests.
2. **Shared events** (new 3 records + extend `UnitCreatedEvent`/`DeviceCreatedEvent`). Pure additive; nothing consumes the new fields yet. Tiny, safe, unblocks everyone.
3. **Projects `Unit` schema + define-structure command** (aggregate, EF config, migration `AddUnitStructureAndOwnerLinking`, seed reconciliation, REST endpoint, `RegisteredOwner` entity). Depends on slices 1+2.
4. **IAM outbox + `UserRegisteredEvent`** (mirror outbox stack, migration `AddOutbox`, `SignUp` emits, `Program.cs` wiring, backfill). Depends on slice 2. Self-contained — biggest single slice; keep it isolated.
5. **Projects owner-linking consumer** (`OwnerLinkingConsumer`, `registered_owner` population, bidirectional match, emit `UnitOwnerMatchedEvent`). Depends on 3+4.
6. **Devices floor provisioning** (`FloorProvisioningConsumer`, `FloorDeviceDefaults`, `Device` floor/unit columns, migration `AddDeviceFloorPlacement`, `DeviceCommandService` `Id==0` fix, idempotency guard). Depends on 2+3.
7. **Analytics projection updates** (projection columns, migration, new `UnitOwnerMatchedEvent` case, read floor/room/email). Depends on 2; no new bindings.

Chain strategy recommendation: **feature-branch-chain** (a tracker branch accumulates integration; PR #1 → tracker, each later PR → previous PR branch) — gives rollback control across 7 slices and keeps review diffs focused. Stacked-to-main is acceptable if the team prioritizes speed, since slices 2/7 are low-risk and additive.

Cross-service migration deploy order at runtime: deploy Shared (no DB) → Projects migration → IAM migration → Devices migration → Analytics migration. Consumers tolerate events for columns they already have (additive), so deploy order among consumers is not strict, but apply each service's migration before that service starts emitting/consuming the new fields. RabbitMQ queues/bindings are declared idempotently by each consumer at startup, so no manual broker setup.

### Review Workload Forecast
- Chained PRs recommended: **Yes**
- 400-line budget risk: **High** (IAM outbox slice alone approaches it; total far exceeds)
- Decision needed before apply: **Yes** (confirm chain strategy: feature-branch-chain recommended)

---

## ADR-Style Decisions

**ADR-A — Two-phase commit (persist aggregate, then outbox) to fix `Id==0`.**
Decision: build the domain event AFTER `CompleteAsync`/`SaveChanges` so the identity is real; persist the outbox row in a second `CompleteAsync`. Rationale: identity columns are 0 pre-commit; the existing single-commit code ships `Id=0` payloads (live bug). Rejected: (a) `SaveChanges` interception to capture ids — more machinery, opaque; (b) client-generated GUID PKs for aggregates — a much larger schema change. Cost: a crash between the two commits leaves an aggregate without its event; mitigated by the existing idempotent `OutboxBackfill` re-emit on startup. Applied uniformly to Projects structure handler, `UnitCommandService`, `DeviceCommandService`, and IAM `SignUp`.

**ADR-B — `registered_owner` mirror in Projects to cover both linking orderings.**
Decision: Projects keeps a local, event-populated mirror of `(email → userId)` for owners, queried on the unit-first path. Rationale: covers the realistic "email assigned after the owner already registered" case without any synchronous IAM call. Rejected: (a) v1 covers only register-after-assign — leaves a real gap; (b) synchronous IAM lookup — reintroduces the runtime coupling the prior change removed. Cost: a second small read-model table in Projects; acceptable.

**ADR-C — Floor-provisioning idempotency via unique `(ProjectId, Floor, Type)` + pre-check.**
Decision: a DB unique index is the hard backstop; a pre-check query is the fast path; a unique-violation on redelivery is caught and acked. Rejected: a separate `processed_events`/`EventId` ledger — heavier; the natural key already fully expresses "this floor's default set". Noted alternative kept for the team if `EventId`-level dedupe is later required.

**ADR-D — Lower-cased email as the cross-service linking invariant.**
Decision: every email is stored/compared lower-cased across IAM, `Unit.OwnerEmail`, and `registered_owner`. Rationale: makes MySQL CI-collation and EF-InMemory ordinal matching agree, so the linking logic is deterministic and unit-testable. Rejected: relying on DB collation alone — breaks on the InMemory test provider, defeats Strict TDD.

**ADR-E — Reuse the existing outbox→topic-exchange pipeline everywhere; IAM becomes a publisher.**
Decision: no new integration mechanism; IAM mirrors the Projects/Devices outbox stack verbatim. Rationale: consistency, at-least-once, zero runtime cross-service coupling. Rejected: lightweight direct publish (loses outbox consistency) and synchronous HTTP (reintroduces coupling) — both already rejected in the proposal (D5), restated here as the architectural backbone.

**ADR-F — `UnitNumber` kept as a derived stored label, not dropped.**
Decision: `UnitNumber = "{Floor}-{RoomNumber}"`, computed at construction, persisted, not a uniqueness key. Uniqueness moves to `(ProjectId, Floor, RoomNumber)`. Rationale: avoids a wide breaking change to existing read/REST/Analytics paths; keeps a human label. Rejected: dropping `UnitNumber` (breaks consumers for no gain); computing on read (more query complexity, breaks seed labels).
