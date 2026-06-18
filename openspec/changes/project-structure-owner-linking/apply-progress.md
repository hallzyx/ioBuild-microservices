# Apply Progress: project-structure-owner-linking

> Last slice completed: **PR 7 — Analytics Projection Updates** (ALL SLICES DONE)
> Branch (PR7): `feat/psol/pr7-analytics`
> Last updated: 2026-06-18

## PR 1 Tasks — Status

- [x] 1.1 [RED] `ProjectsDbFixtureTests.NewContext_CanRoundTrip_Unit` — failing test written first (compilation error — fixture didn't exist). File: `tests/IoBuild.Projects.Tests/Infrastructure/ProjectsDbFixtureTests.cs`
- [x] 1.2 [GREEN] `ProjectsDbFixture` created with `NewContext(string dbName)` static builder + `SeedProjectAsync` / `SeedUnitAsync` helpers. `SeedRegisteredOwnerAsync` intentionally omitted — `RegisteredOwner` entity is PR 3 scope. File: `tests/IoBuild.Projects.Tests/Infrastructure/ProjectsDbFixture.cs`
- [x] 1.3 [RED] New test `Handle_CreateUnit_CallsCompleteAsyncTwice_TwoPhaseCommit` added to `OutboxWriteInTransactionTests` asserting `CompleteAsync` called `Times.Exactly(2)` — confirmed FAILING on pre-fix code (called once). Also added `Handle_CreateUnit_WritesOutboxWithRealId` using real EF context (GREEN on both old and new code via InMemory key-gen behavior — see note). File: `tests/IoBuild.Projects.Tests/Application/OutboxWriteInTransactionTests.cs`
- [x] 1.4 [GREEN] `UnitCommandService.cs` reordered to two-phase commit: Phase 1 `CompleteAsync` (unit persisted, real Id assigned), then event built, then outbox row, then Phase 2 `CompleteAsync`. File: `src/IoBuild.Projects/Application/Services/UnitCommandService.cs`
- [x] 1.5 All `IoBuild.Projects.Tests` pass: **11/11 green** (8 original + 3 new).
- [x] 1.6 Two-phase commit reasoning documented in `UnitCommandService.cs` as an ADR-A code comment referencing the MySQL Id==0 bug, crash-recovery via `OutboxBackfill`, and consistency with Devices/IAM pattern.

## Notes / Discoveries

### EF InMemory ID assignment behavior (important for future test authors)
EF InMemory assigns `Id = 1` (positive sequential) immediately after `AddAsync`, BEFORE `SaveChanges`. This differs from MySQL/Pomelo where `Id` remains `0` until the DB INSERT returns the generated key. As a result:
- `Handle_CreateUnit_WritesOutboxWithRealId` passes on both old and new code under InMemory.
- The canonical RED test for the two-phase commit is `Handle_CreateUnit_CallsCompleteAsyncTwice_TwoPhaseCommit` (Moq, verifies `Times.Exactly(2)`).
- The bug is real on production MySQL. The fix is correct. InMemory simply can't replicate the `Id==0` pre-save scenario.

### `SeedRegisteredOwnerAsync` stub
Per PR 1 scope: `RegisteredOwner` entity does not exist yet (introduced in PR 3, Task 3.3). The fixture includes a code comment noting where to add this helper in PR 3.

### Packages added
None — `Microsoft.EntityFrameworkCore.InMemory 9.0.5` was already in the test project `.csproj`.

## Files Changed (PR 1)

| File | Change |
|---|---|
| `tests/IoBuild.Projects.Tests/Infrastructure/ProjectsDbFixture.cs` | CREATED — shared EF InMemory fixture |
| `tests/IoBuild.Projects.Tests/Infrastructure/ProjectsDbFixtureTests.cs` | CREATED — fixture smoke test |
| `tests/IoBuild.Projects.Tests/Application/OutboxWriteInTransactionTests.cs` | UPDATED — added two RED→GREEN tests for ADR-A |
| `src/IoBuild.Projects/Application/Services/UnitCommandService.cs` | UPDATED — two-phase commit fix + ADR-A comment |

## Test Run Summary (PR 1 final)

```
Correctas! - Con error: 0, Superado: 11, Omitido: 0, Total: 11, Duración: ~900ms
IoBuild.Projects.Tests.dll (net9.0)
```

dotnet build IoBuild.Projects: 0 Errors, warnings only (pre-existing).

---

# PR 2 — Shared Event Contracts

> PR slice: **PR 2 — Shared Event Contracts**
> Branch: `feat/psol/pr2-shared-events`
> Last updated: 2026-06-18

## PR 2 Tasks — Status

- [x] 2.1 [RED] Serialization tests written first (compilation errors confirmed) for `UserRegisteredEvent`, `UnitOwnerMatchedEvent`, `FloorStructureDefinedEvent` — `EventId != Guid.Empty`, correct `RoutingKey`, JSON round-trip of all payload fields. File: `tests/IoBuild.Shared.Tests/Domain/Model/Events/SharedEventContractTests.cs`
- [x] 2.2 [RED] Tests for extended `UnitCreatedEvent` — `Floor`, `RoomNumber`, `OwnerEmail` fields serialize/deserialize; existing fields unchanged; default values verified.
- [x] 2.3 [RED] Test for `DeviceCreatedEvent.FloorNumber?` — serializes as null when not set; round-trips when set.
- [x] 2.4 [GREEN] Created `src/IoBuild.Shared/Domain/Model/Events/UserRegisteredEvent.cs` — `UserId:int`, `Email:string`, `Role:string`; `RoutingKey = "iam.user.registered"`.
- [x] 2.5 [GREEN] Created `src/IoBuild.Shared/Domain/Model/Events/UnitOwnerMatchedEvent.cs` — `UnitId:int`, `ProjectId:int`, `OwnerUserId:int`, `OwnerEmail:string`; `RoutingKey = "project.unit.owner-matched"`.
- [x] 2.6 [GREEN] Created `src/IoBuild.Shared/Domain/Model/Events/FloorStructureDefinedEvent.cs` — `ProjectId:int`, `Floor:int`, `UnitCount:int`, `BuilderId:int`; `RoutingKey = "project.floor.defined"`.
- [x] 2.7 [GREEN] Extended `UnitCreatedEvent.cs` — added `Floor:int = 0`, `RoomNumber:string = ""`, `OwnerEmail:string?` init properties. Existing producers compile unchanged (defaults).
- [x] 2.8 [GREEN] Extended `DeviceCreatedEvent.cs` — added `FloorNumber:int?` init property. Existing producers compile unchanged (default null).
- [x] 2.9 All serialization tests green (36/36). `dotnet build IoBuild.sln`: 0 errors, 4 warnings (pre-existing MQTTnet NuGet version warnings only).

## Notes / Discoveries (PR 2)

### Additive extension strategy
Both `UnitCreatedEvent` and `DeviceCreatedEvent` were extended using `init` properties with explicit defaults (`= 0`, `= string.Empty`, `= null`). This means every existing object initializer in Projects, Devices, and Analytics compiles unchanged — no producer needed updating. The new fields appear in JSON output immediately (System.Text.Json serializes all properties by default), which is wire-backward-compatible since consumers that don't read the new fields simply ignore them.

### Serializer: System.Text.Json defaults (PascalCase)
The codebase uses `JsonSerializer.Serialize(domainEvent, domainEvent.GetType())` in `RabbitMqDomainEventPublisher` — no custom options, default PascalCase property naming. Tests use the same approach to stay wire-compatible.

## Files Changed (PR 2)

| File | Change |
|---|---|
| `tests/IoBuild.Shared.Tests/Domain/Model/Events/SharedEventContractTests.cs` | CREATED — 17 serialization round-trip tests for all 5 changed/new events |
| `src/IoBuild.Shared/Domain/Model/Events/UserRegisteredEvent.cs` | CREATED — new event record |
| `src/IoBuild.Shared/Domain/Model/Events/UnitOwnerMatchedEvent.cs` | CREATED — new event record |
| `src/IoBuild.Shared/Domain/Model/Events/FloorStructureDefinedEvent.cs` | CREATED — new event record |
| `src/IoBuild.Shared/Domain/Model/Events/UnitCreatedEvent.cs` | UPDATED — added Floor, RoomNumber, OwnerEmail fields with defaults |
| `src/IoBuild.Shared/Domain/Model/Events/DeviceCreatedEvent.cs` | UPDATED — added FloorNumber? field |

## Test Run Summary (PR 2 final)

```
Correctas! - Con error: 0, Superado: 36, Omitido: 0, Total: 36, Duración: ~490ms
IoBuild.Shared.Tests.dll (net9.0)
```

dotnet build IoBuild.sln: 0 Errores, 4 Advertencias (pre-existing MQTTnet NU1603 only).

---

# PR 3 — Projects `Unit` Schema + Define-Structure Command

> PR slice: **PR 3 — Projects `Unit` Schema + Define-Structure Command**
> Branch: `feat/psol/pr3-unit-schema`
> Last updated: 2026-06-18

## PR 3 Tasks — Status

- [x] 3.1 [RED] `UnitAggregateTests` written with 7 test cases — ComposeUnitNumber, nullable OwnerId, LinkOwner, AssignOwnerEmail lower-casing, Floor/RoomNumber fields, OwnerEmail lower-casing at ctor, legacy ctor shape. Build failed on missing `Floor`, `ComposeUnitNumber`, etc. File: `tests/IoBuild.Projects.Tests/Domain/UnitAggregateTests.cs`
- [x] 3.2 [GREEN] `Unit.cs` updated: added `Floor:int`, `RoomNumber:string`, `OwnerEmail:string?`; changed `OwnerId` to `int?`; added `LinkOwner`, `AssignOwnerEmail` (lower-cases via ToLowerInvariant), `ComposeUnitNumber` static; new structure-definition ctor; legacy ctor kept (`Floor=0, RoomNumber=unitNumber`). File: `src/IoBuild.Projects/Domain/Model/Aggregates/Unit.cs`
- [x] 3.3 [GREEN] `RegisteredOwner.cs` created: `Email` (PK, lower-cased in ctor), `UserId`, `LastEventAt`, `UpdateIfNewer` LWW guard. File: `src/IoBuild.Projects/Domain/Model/Entities/RegisteredOwner.cs`
- [x] 3.4 [GREEN] `AppDbContext.cs` updated: extended Unit EF config (new columns, nullable OwnerId, unique index `(ProjectId,Floor,RoomNumber)`, index on `OwnerEmail`); added `RegisteredOwner` entity block (HasKey Email, maxlen 255, table `registered_owner`); added `DbSet<RegisteredOwner> RegisteredOwners`. File: `src/IoBuild.Projects/Infrastructure/Persistence/AppDbContext.cs`
- [x] 3.5 [GREEN] Seed data reconciled: all 5 existing units got explicit `Floor`/`RoomNumber`/`OwnerEmail=null` values; `OwnerId` kept as `int?` with existing values preserved. File: `src/IoBuild.Projects/Infrastructure/Persistence/EFC/Configuration/Seed/ProjectsSeedData.cs`
- [x] 3.6 Migration generated: `20260618180117_AddUnitStructureAndOwnerLinking.cs`. Adds `floor`/`room_number`/`owner_email` columns; alters `owner_id` to nullable; creates `registered_owner` table; drops old `IX_units_project_id`; adds unique index `IX_units_project_id_floor_room_number` and index `IX_units_owner_email`; UpdateData rows for all 5 seed units.
- [x] 3.7 [RED] `DefineProjectStructureCommandTests` written with 9 test cases covering all PS scenarios: unit count, floor/room fields, null OwnerId, real UnitId in outbox, FloorStructureDefinedEvent per floor, 409 conflict, 422 on floors=0, 422 on unitsPerFloor=0, unit-first owner match (RegisteredOwner lookup), no match when no email. File: `tests/IoBuild.Projects.Tests/Application/DefineProjectStructureCommandTests.cs`
- [x] 3.8 [GREEN] `DefineProjectStructureCommand.cs` created: records `RoomSpec(RoomNumber, OwnerEmail)`, `FloorSpec(Floor, Rooms)`, `DefineProjectStructureCommand(ProjectId, Floors, BuilderId)`. File: `src/IoBuild.Projects/Domain/Services/Commands/Projects/DefineProjectStructureCommand.cs`
- [x] 3.9 [GREEN] `ProjectStructureCommandService.cs` created: validation (ArgumentException for floors<1/rooms<1), 409 guard (InvalidOperationException if units exist), two-phase commit (Phase1: persist units, Phase2: outbox rows), UnitCreatedEvent per unit with real Id, FloorStructureDefinedEvent per floor, unit-first RegisteredOwner lookup → LinkOwner + UnitOwnerMatchedEvent. File: `src/IoBuild.Projects/Application/Services/ProjectStructureCommandService.cs`
- [x] 3.10 [GREEN] `ProjectsController.cs` updated: added `POST {id}/structure` action; Builder-role guard via `HttpContext.Items["UserRole"]`; expands uniform floors×unitsPerFloor into FloorSpec lists; maps OwnerEmails per-unit assignments; catches ArgumentException → 422, InvalidOperationException → 409. `ProjectStructureCommandService` registered in `Program.cs`. New resource: `DefineStructureResource.cs` and `OwnerEmailAssignment`. Also updated `OutboxWorker.EventTypeMap` to include `FloorStructureDefinedEvent` and `UnitOwnerMatchedEvent`. Files: `Interfaces/REST/ProjectsController.cs`, `Program.cs`, `Workers/OutboxWorker.cs`, `Interfaces/Resources/DefineStructureResource.cs`
- [x] 3.11 All 28 tests green. `dotnet build IoBuild.Projects`: 0 Errors, 0 Warnings.

## Notes / Discoveries (PR 3)

### UnitResource.OwnerId changed to int?
`UnitResource.OwnerId` was `int` — changed to `int?` to match the now-nullable aggregate property. Breaking change to the REST response shape (int → int?). Callers receiving this field as required should handle null.

### Seed data: `HasData` requires anonymous types with ALL columns
EF's `HasData` with anonymous objects requires every column that is NOT NULL to have an explicit value. Since `Floor` and `RoomNumber` are NOT NULL (no `HasDefaultValue` in CLR model), they must be in every seed row. The migration generated `UpdateData` rows for all 5 existing units — correct.

### Unique index on (ProjectId, Floor, RoomNumber)
The EF-InMemory provider does NOT enforce unique indexes. Tests assert the business logic guard (409 via InvalidOperationException) rather than the DB constraint — this is correct; the unique index is the hard DB-level backstop for concurrent writes.

### RegisteredOwner table is empty at PR 3
The `registered_owner` table is only populated by the `OwnerLinkingConsumer` (PR 5). Unit-first tests use `SeedRegisteredOwnerAsync` in the fixture to simulate a pre-existing owner row.

## Files Changed (PR 3)

| File | Change |
|---|---|
| `tests/IoBuild.Projects.Tests/Domain/UnitAggregateTests.cs` | CREATED — 7 aggregate unit tests |
| `tests/IoBuild.Projects.Tests/Application/DefineProjectStructureCommandTests.cs` | CREATED — 9 command service tests |
| `tests/IoBuild.Projects.Tests/Infrastructure/ProjectsDbFixture.cs` | UPDATED — added SeedRegisteredOwnerAsync, SeedStructuredUnitAsync helpers; added RegisteredOwner using |
| `src/IoBuild.Projects/Domain/Model/Aggregates/Unit.cs` | UPDATED — Floor, RoomNumber, OwnerEmail, int? OwnerId, LinkOwner, AssignOwnerEmail, ComposeUnitNumber, structure ctor |
| `src/IoBuild.Projects/Domain/Model/Entities/RegisteredOwner.cs` | CREATED — RegisteredOwner mirror entity |
| `src/IoBuild.Projects/Infrastructure/Persistence/AppDbContext.cs` | UPDATED — extended Unit config, added RegisteredOwner config + DbSet |
| `src/IoBuild.Projects/Infrastructure/Persistence/EFC/Configuration/Seed/ProjectsSeedData.cs` | UPDATED — seed reconciliation (Floor, RoomNumber, OwnerEmail=null for all 5 units) |
| `src/IoBuild.Projects/Migrations/20260618180117_AddUnitStructureAndOwnerLinking.cs` | CREATED — EF migration |
| `src/IoBuild.Projects/Migrations/20260618180117_AddUnitStructureAndOwnerLinking.Designer.cs` | CREATED — migration designer snapshot |
| `src/IoBuild.Projects/Migrations/AppDbContextModelSnapshot.cs` | UPDATED — EF snapshot |
| `src/IoBuild.Projects/Domain/Services/Commands/Projects/DefineProjectStructureCommand.cs` | CREATED — RoomSpec, FloorSpec, DefineProjectStructureCommand records |
| `src/IoBuild.Projects/Application/Services/ProjectStructureCommandService.cs` | CREATED — command handler |
| `src/IoBuild.Projects/Interfaces/REST/ProjectsController.cs` | UPDATED — POST {id}/structure endpoint |
| `src/IoBuild.Projects/Interfaces/Resources/DefineStructureResource.cs` | CREATED — request DTO |
| `src/IoBuild.Projects/Interfaces/Resources/UnitResource.cs` | UPDATED — OwnerId changed to int? |
| `src/IoBuild.Projects/Workers/OutboxWorker.cs` | UPDATED — EventTypeMap + FloorStructureDefinedEvent + UnitOwnerMatchedEvent |
| `src/IoBuild.Projects/Program.cs` | UPDATED — registered ProjectStructureCommandService |

## Test Run Summary (PR 3 final)

```
Correctas! - Con error: 0, Superado: 28, Omitido: 0, Total: 28, Duración: ~1s
IoBuild.Projects.Tests.dll (net9.0)
```

dotnet build IoBuild.Projects: 0 Errores, 0 Advertencias.

---

# PR 4 — IAM Outbox + UserRegisteredEvent

> PR slice: **PR 4 — IAM Outbox + UserRegisteredEvent**
> Branch: `feat/psol/pr4-iam-outbox`
> Last updated: 2026-06-18

## PR 4 Tasks — Status

- [x] 4.1 [RED] `IamSignUpOutboxTests` written with 3 test cases — one outbox row with `EventType=UserRegisteredEvent`, `UserId>0`, email lower-cased. File: `tests/IoBuild.IAM.Tests/Application/IamSignUpOutboxTests.cs`
- [x] 4.2 [RED] `IamOutboxWorkerTests` written with 3 test cases mirroring Devices' `OutboxWorkerPublishTests` — publish success marks Processed, publish failure increments RetryCount without throwing, no pending messages skips publisher. File: `tests/IoBuild.IAM.Tests/Workers/IamOutboxWorkerTests.cs`
- [x] 4.3 [GREEN] `OutboxMessage.cs` created in IAM (exact copy of Projects entity). File: `src/IoBuild.IAM/Domain/Model/Entities/OutboxMessage.cs`
- [x] 4.4 [GREEN] `IIamOutboxMessageRepository.cs` created (interface: `GetPendingAsync`, `AddAsync`, `UpdateAsync`). File: `src/IoBuild.IAM/Domain/Repositories/IIamOutboxMessageRepository.cs`
- [x] 4.5 [GREEN] `OutboxMessageRepository.cs` created (EF Core impl). File: `src/IoBuild.IAM/Infrastructure/Persistence/EFC/Repositories/OutboxMessageRepository.cs`
- [x] 4.6 [GREEN] `OutboxWorker.cs` created: `EventTypeMap = { UserRegisteredEvent }`. File: `src/IoBuild.IAM/Workers/OutboxWorker.cs`
- [x] 4.7 [GREEN] `ApplicationDbContext.cs` updated: added `DbSet<OutboxMessage>`, entity config (key, maxlens, `(Status,CreatedAt)` index). File: `src/IoBuild.IAM/Infrastructure/Persistence/EFC/Repositories/ApplicationDbContext.cs`
- [x] 4.8 Migration generated: `20260618181433_AddOutbox.cs` — creates `outbox_messages` table + `IX_outbox_messages_status_created_at` index. Also added `ApplicationDbContextFactory.cs` (design-time factory).
- [x] 4.9 [GREEN] `UserCommandService.cs` updated: injected `IIamOutboxMessageRepository`; two-phase commit (Phase1: user persisted, Phase2: outbox row with real `UserId` + lower-cased `Email`). File: `src/IoBuild.IAM/Application/Internal/CommandServices/UserCommandService.cs`
- [x] 4.10 [GREEN] `Program.cs` updated: registered `IIamOutboxMessageRepository`, `AddDomainEventPublishing(builder.Configuration)`, `AddHostedService<OutboxWorker>()`, switched from `EnsureCreated` to `MigrateAsync + OutboxBackfill`. File: `src/IoBuild.IAM/Program.cs`
- [x] 4.11 `OutboxBackfill.cs` added: emits `UserRegisteredEvent` for seeded IAM users on first boot; idempotent guard. File: `src/IoBuild.IAM/Infrastructure/Persistence/EFC/DbContext/OutboxBackfill.cs`
- [x] 4.12 **9/9 green**. `dotnet build IoBuild.IAM`: 0 errors, 0 warnings.

## Notes / Discoveries (PR 4)

### Interface naming: IIamOutboxMessageRepository
Used `IIamOutboxMessageRepository` instead of `IOutboxMessageRepository` to avoid namespace collision — IAM test project references both `IoBuild.IAM` and transitively `IoBuild.Shared`, and the type name would be ambiguous. This is a local naming choice that doesn't affect the wire contract.

### Migration creates both outbox_messages AND users tables
IAM was previously using `EnsureCreated` (no migrations). Adding the first EF migration creates ALL configured entities (users + outbox_messages). This is correct — the migration replaces `EnsureCreated` for production. For existing deployments with the DB already created, EF will apply only the diff (adding outbox_messages).

### docker-compose.yml: iam depends_on rabbitmq
Added `rabbitmq: condition: service_healthy` to `iam`'s `depends_on`. IAM already had `mysql: service_healthy`. Other services depend on `iam: service_healthy`, and RabbitMQ has no app dependencies — no cycle created.

### EF InMemory Id assignment (same note as PR1)
EF InMemory assigns sequential positive Ids immediately after `AddAsync`, before `SaveChanges`. The two-phase commit test passes under InMemory because `user.Id` is already set when `UserRegisteredEvent` is built. The production bug (Id=0 before MySQL SaveChanges) is correctly fixed by the reorder in `UserCommandService`.

## Files Changed (PR 4)

| File | Change |
|---|---|
| `tests/IoBuild.IAM.Tests/IoBuild.IAM.Tests.csproj` | UPDATED — added `Microsoft.EntityFrameworkCore.InMemory 9.0.5` |
| `tests/IoBuild.IAM.Tests/Application/IamSignUpOutboxTests.cs` | CREATED — 3 signup outbox tests |
| `tests/IoBuild.IAM.Tests/Workers/IamOutboxWorkerTests.cs` | CREATED — 3 worker publish tests |
| `src/IoBuild.IAM/IoBuild.IAM.csproj` | UPDATED — added `Microsoft.EntityFrameworkCore.Design 9.0.5` |
| `src/IoBuild.IAM/Domain/Model/Entities/OutboxMessage.cs` | CREATED — outbox entity |
| `src/IoBuild.IAM/Domain/Repositories/IIamOutboxMessageRepository.cs` | CREATED — repository interface |
| `src/IoBuild.IAM/Infrastructure/Persistence/EFC/Repositories/OutboxMessageRepository.cs` | CREATED — EF Core impl |
| `src/IoBuild.IAM/Infrastructure/Persistence/EFC/Repositories/ApplicationDbContext.cs` | UPDATED — OutboxMessages DbSet + entity config |
| `src/IoBuild.IAM/Infrastructure/Persistence/EFC/ApplicationDbContextFactory.cs` | CREATED — design-time factory for migrations |
| `src/IoBuild.IAM/Infrastructure/Persistence/EFC/DbContext/OutboxBackfill.cs` | CREATED — seeded-user backfill |
| `src/IoBuild.IAM/Workers/OutboxWorker.cs` | CREATED — OutboxWorker with UserRegisteredEvent EventTypeMap |
| `src/IoBuild.IAM/Application/Internal/CommandServices/UserCommandService.cs` | UPDATED — two-phase commit + outbox row |
| `src/IoBuild.IAM/Program.cs` | UPDATED — outbox services + MigrateAsync + BackfillRunAsync |
| `src/IoBuild.IAM/Migrations/20260618181433_AddOutbox.cs` | CREATED — EF migration |
| `src/IoBuild.IAM/Migrations/20260618181433_AddOutbox.Designer.cs` | CREATED — migration designer |
| `src/IoBuild.IAM/Migrations/ApplicationDbContextModelSnapshot.cs` | CREATED — EF snapshot |
| `microservices/docker-compose.yml` | UPDATED — iam depends_on rabbitmq + RabbitMq__ConnectionString env |

## Test Run Summary (PR 4 final)

```
Correctas! - Con error: 0, Superado: 9, Omitido: 0, Total: 9, Duración: ~6s
IoBuild.IAM.Tests.dll (net9.0)
```

dotnet build IoBuild.IAM: 0 Errores, 0 Advertencias.
Migration filename: 20260618181433_AddOutbox.cs
docker compose config -q: no output (valid YAML).

---

# PR 5 — Projects Owner-Linking Consumer

> PR slice: **PR 5 — Projects Owner-Linking Consumer**
> Branch: `feat/psol/pr5-owner-consumer`
> Last updated: 2026-06-18

## PR 5 Tasks — Status

- [x] 5.1 [RED] `OwnerLinkingConsumerTests` written with 5 test cases covering all OL scenarios. Build failed (CS0234: namespace `Messaging` not found — correct RED). File: `tests/IoBuild.Projects.Tests/Application/OwnerLinkingConsumerTests.cs`
- [x] 5.2 [GREEN] `OwnerLinkingConsumer.cs` created: `BackgroundService`, topology `projects.owner-linking / iam.user.#` on exchange `iobuild.domain.events`; `UserRegisteredEvent` handler upserts `registered_owner` mirror (LWW) then queries `Units` by lower-cased email where `OwnerId==null`; `unit.LinkOwner()` + `UnitOwnerMatchedEvent` outbox per match; single `CompleteAsync`; non-owner role skipped; transient/poison nack; internal test-seam constructor (mirrors `AnalyticsEventConsumer` pattern exactly). File: `src/IoBuild.Projects/Infrastructure/Messaging/OwnerLinkingConsumer.cs`
- [x] 5.3 [GREEN] `OwnerLinkingConsumerExtensions.cs` created: `AddOwnerLinkingConsumer()` extension. File: `src/IoBuild.Projects/Infrastructure/Messaging/OwnerLinkingConsumerExtensions.cs`
- [x] 5.4 [GREEN] `OutboxWorker.EventTypeMap` already contained `FloorStructureDefinedEvent` and `UnitOwnerMatchedEvent` (added in PR3, Task 3.10). No change needed.
- [x] 5.5 [GREEN] `Program.cs` updated: added `using IoBuild.Projects.Infrastructure.Messaging`; called `AddOwnerLinkingConsumer(builder.Configuration)`. `IoBuild.Projects.csproj` updated: added `InternalsVisibleTo("IoBuild.Projects.Tests")` so the `internal` test-seam constructor is accessible from the test project. Files: `src/IoBuild.Projects/Program.cs`, `src/IoBuild.Projects/IoBuild.Projects.csproj`
- [x] 5.6 **33/33 green**. `dotnet build IoBuild.Projects`: 0 Errors, 40 pre-existing Warnings (resource nullable CS8618 — unchanged from prior PRs).

## Notes / Discoveries (PR 5)

### InternalsVisibleTo required for test seam
The `internal` constructor pattern used by `AnalyticsEventConsumer` requires an `InternalsVisibleTo` attribute in the production project's `.csproj` — without it, the test assembly cannot call `internal` constructors. This was missing in `IoBuild.Projects.csproj` (Analytics already had it). Added `InternalsVisibleTo("IoBuild.Projects.Tests")` as a `.csproj` `AssemblyAttribute` item (same format as Analytics uses).

### OL-S02 consumer behavior vs. spec
The spec describes OL-S02 as "registration-first" — i.e., the IAM user registers before the unit is email-assigned. The consumer's role in this scenario is to upsert the `registered_owner` mirror row so that `ProjectStructureCommandService`'s inline lookup (PR3) can find it. The consumer does NOT emit a `UnitOwnerMatchedEvent` in this case (no unit matches yet). The test asserts: mirror row created, outbox empty.

### OutboxWorker.EventTypeMap — already complete from PR3
PR3 (Task 3.10) updated the `EventTypeMap` to include `FloorStructureDefinedEvent` and `UnitOwnerMatchedEvent`. No change needed in PR5. Confirmed by reading the file before implementation.

### EF InMemory ordinal string comparison (ADR-D)
EF InMemory uses ordinal (case-sensitive) string comparison for `Where` filters. The ADR-D lower-casing convention ensures email comparisons work on both InMemory (tests) and MySQL CI collation (production). OL-S03 works correctly because both the stored `unit.OwnerEmail` (lower-cased via `Unit` ctor) and the event email (lower-cased by IAM publisher and further lowercased by `normalizedEmail`) are canonical lower-case strings.

## Files Changed (PR 5)

| File | Change |
|---|---|
| `tests/IoBuild.Projects.Tests/Application/OwnerLinkingConsumerTests.cs` | CREATED — 5 OL scenario tests (OL-S01..OL-S05) |
| `src/IoBuild.Projects/Infrastructure/Messaging/OwnerLinkingConsumer.cs` | CREATED — BackgroundService consumer |
| `src/IoBuild.Projects/Infrastructure/Messaging/OwnerLinkingConsumerExtensions.cs` | CREATED — DI extension method |
| `src/IoBuild.Projects/Program.cs` | UPDATED — using + AddOwnerLinkingConsumer() call |
| `src/IoBuild.Projects/IoBuild.Projects.csproj` | UPDATED — InternalsVisibleTo("IoBuild.Projects.Tests") |

## Test Run Summary (PR 5 final)

```
Correctas! - Con error: 0, Superado: 33, Omitido: 0, Total: 33, Duración: 1s
IoBuild.Projects.Tests.dll (net9.0)
```

dotnet build IoBuild.Projects: 0 Errores, 40 Advertencias (pre-existing CS8618 nullable resource warnings).

---

# PR 6 — Devices Floor Provisioning

> PR slice: **PR 6 — Devices Floor Provisioning**
> Branch: `feat/psol/pr6-devices-provisioning`
> Last updated: 2026-06-18

## PR 6 Tasks — Status

- [x] 6.1 [RED] `DeviceCommandServiceOutboxTests` written: 3 tests — (1) EF-InMemory: outbox payload DeviceId matches persisted device Id; (2) Moq: SaveChangesAsync called exactly twice (two-phase commit); (3) Moq: call order AddDevice → SaveChanges → AddOutbox → SaveChanges. Tests 2 and 3 failed on pre-fix single-commit code. File: `tests/IoBuild.Devices.Tests/Application/DeviceCommandServiceOutboxTests.cs`
- [x] 6.2 [GREEN] `DeviceCommandService.Handle(CreateDeviceCommand)` fixed: two-phase commit applied — Phase 1: `SaveChangesAsync()` persists device (real Id assigned); Phase 2: build `DeviceCreatedEvent` with `device.Id` and `device.FloorNumber`, add outbox row, second `SaveChangesAsync()`. File: `src/IoBuild.Devices/Application/Internal/CommandServices/DeviceCommandService.cs`
- [x] 6.3 [RED] `FloorProvisioningConsumerTests` written: 6 tests using SQLite-in-memory so unique constraint is enforced — FD-S01 (3 devices, FloorNumber=2, Location="Floor 2", 3 outbox rows), FD-S02 (9 devices for 3 floors), FD-S03 (redelivery no-op via pre-check), FD-S04 (each DeviceCreatedEvent payload FloorNumber=3), FloorDeviceDefaults constant test. File: `tests/IoBuild.Devices.Tests/Application/FloorProvisioningConsumerTests.cs`
- [x] 6.4 [GREEN] `Device.cs` updated: added `FloorNumber:int?` and `UnitId:int?` private properties; two constructors — existing 6-arg (floor=null,unit=null defaults) and new 8-arg (accepts floorNumber, unitId). File: `src/IoBuild.Devices/Domain/Model/Aggregates/Device.cs`
- [x] 6.5 [GREEN] `DevicesDbContext.OnModelCreating` updated: mapped `FloorNumber` and `UnitId` as nullable columns; added unique index `(ProjectId, FloorNumber, Type)` for idempotency guard (ADR-C). File: `src/IoBuild.Devices/Infrastructure/Persistence/EFC/DbContext/DevicesDbContext.cs`
- [x] 6.6 Migration generated: `20260618183750_AddDeviceFloorPlacement.cs`. Confirms: `AddColumn<int> name: "floor_number" nullable: true` and `AddColumn<int> name: "unit_id" nullable: true` on `devices` table. Unique index `IX_devices_project_id_floor_number_type` created. Additive migration only. File: `src/IoBuild.Devices/Migrations/20260618183750_AddDeviceFloorPlacement.cs`
- [x] 6.7 [GREEN] `FloorDeviceDefaults.cs` created: `Defaults` static readonly list with `("SmartMeter","Smart Meter")`, `("WaterSensor","Water Sensor")`, `("SmokeDetector","Smoke Detector")`. File: `src/IoBuild.Devices/Domain/Constants/FloorDeviceDefaults.cs`
- [x] 6.8 [GREEN] `FloorProvisioningConsumer.cs` created: `BackgroundService`; topology `devices.provisioning / project.floor.defined` on `iobuild.domain.events` exchange; internal test-seam constructor (direct `DevicesDbContext`); `ProvisionFloorAsync` with idempotency pre-check (`AnyAsync` for first type), two-phase commit (Phase1: 3 Device rows, Phase2: 3 DeviceCreatedEvent outbox rows with `FloorNumber=evt.Floor`); `DbUpdateException` unique-violation → ack as already-provisioned; transient/poison nack. File: `src/IoBuild.Devices/Infrastructure/Messaging/FloorProvisioningConsumer.cs`
- [x] 6.9 [GREEN] `Program.cs` updated: added `using IoBuild.Devices.Infrastructure.Messaging`; `AddHostedService<FloorProvisioningConsumer>()`. `IoBuild.Devices.csproj` updated: `InternalsVisibleTo("IoBuild.Devices.Tests")`. Also extended `IDeviceRepository` with `ExistsByProjectFloorTypeAsync` + implemented in `DeviceRepository`. Updated `OutboxWriteInTransactionTests.HandleCreate_SaveChangesCalledOnce_CoveringBothRows` → renamed to `HandleCreate_SaveChangesCalledTwice_TwoPhaseCommit` (expected behavior now `Times.Exactly(2)`). Files: `src/IoBuild.Devices/Program.cs`, `src/IoBuild.Devices/IoBuild.Devices.csproj`, `src/IoBuild.Devices/Domain/Repositories/IDeviceRepository.cs`, `src/IoBuild.Devices/Infrastructure/Persistence/EFC/Repositories/DeviceRepository.cs`, `tests/IoBuild.Devices.Tests/Application/OutboxWriteInTransactionTests.cs`
- [x] 6.10 **45/45 green**. `dotnet build IoBuild.Devices`: 0 Errors, pre-existing warnings only.

## Notes / Discoveries (PR 6)

### EF InMemory: same Id-before-save behavior (same as PR1 discovery)
EF InMemory assigns sequential positive Ids after `AddAsync`, before `SaveChanges`. This means the EF-InMemory test for `DeviceCommandServiceOutboxTests` (task 6.1) passes even on the OLD single-commit code because `device.Id` is already non-zero after `AddAsync`. The canonical RED tests are the Moq-based seam tests (SaveChangesAsync called exactly 2 times; correct call order). The production bug (`Id=0` before MySQL SaveChanges) is real; the fix is correct.

### OutboxWriteInTransactionTests — updated to reflect two-phase behavior
The pre-existing test `HandleCreate_SaveChangesCalledOnce_CoveringBothRows` now fails (it asserted `Times.Once`). Updated to `HandleCreate_SaveChangesCalledTwice_TwoPhaseCommit` asserting `Times.Exactly(2)`. This is the expected update — the old test was documenting the buggy single-commit behavior. Updated test name and expectation match the ADR-A fix.

### SQLite unique index enforcement
SQLite-in-memory enforces unique constraints correctly. The idempotency test (FD-S03) works via the pre-check guard (`AnyAsync` for first type). The unique index `(ProjectId, FloorNumber, Type)` is a hard backstop for concurrent deliveries. Both mechanisms confirmed working in tests.

### Seed data (FloorNumber=null) and unique index coexistence
All 12 seeded devices have `FloorNumber=null`. MySQL and SQLite treat NULL values as distinct in unique indexes, so multiple seed rows with the same `(ProjectId, Type)` and NULL `FloorNumber` do not conflict. The test DB is shared with seed data — tests use ProjectIds 100+ to avoid collision with seed ProjectIds (1-3).

### MAC address generation for floor-provisioned devices
Used a deterministic hash of `(projectId, floor, type)` to generate MAC addresses in the format `F1:XX:XX:XX:XX:XX`. This avoids the unique MAC constraint conflict across multiple floor provisioning calls. Not production-ready (real hardware uses DHCP), but correct for testing and seed purposes.

### `IDeviceRepository.ExistsByProjectFloorTypeAsync` added
Extended the repository interface and implementation to support the idempotency pre-check without requiring the consumer to inject a raw `DbContext`. Consistent with the repository pattern already used in `IoBuild.Devices`.

## Files Changed (PR 6)

| File | Change |
|---|---|
| `tests/IoBuild.Devices.Tests/IoBuild.Devices.Tests.csproj` | UPDATED — added `Microsoft.EntityFrameworkCore.Sqlite 9.0.5` |
| `tests/IoBuild.Devices.Tests/Application/DeviceCommandServiceOutboxTests.cs` | CREATED — 3 two-phase commit fix tests |
| `tests/IoBuild.Devices.Tests/Application/FloorProvisioningConsumerTests.cs` | CREATED — 6 FD scenario tests (SQLite-in-memory) |
| `tests/IoBuild.Devices.Tests/Application/OutboxWriteInTransactionTests.cs` | UPDATED — renamed/updated `HandleCreate_SaveChangesCalledTwice_TwoPhaseCommit` |
| `src/IoBuild.Devices/IoBuild.Devices.csproj` | UPDATED — `InternalsVisibleTo("IoBuild.Devices.Tests")` |
| `src/IoBuild.Devices/Domain/Model/Aggregates/Device.cs` | UPDATED — `FloorNumber:int?`, `UnitId:int?`, extended ctor |
| `src/IoBuild.Devices/Domain/Constants/FloorDeviceDefaults.cs` | CREATED — SmartMeter, WaterSensor, SmokeDetector defaults |
| `src/IoBuild.Devices/Domain/Repositories/IDeviceRepository.cs` | UPDATED — added `ExistsByProjectFloorTypeAsync` |
| `src/IoBuild.Devices/Infrastructure/Persistence/EFC/Repositories/DeviceRepository.cs` | UPDATED — implemented `ExistsByProjectFloorTypeAsync` |
| `src/IoBuild.Devices/Infrastructure/Persistence/EFC/DbContext/DevicesDbContext.cs` | UPDATED — `FloorNumber`/`UnitId` nullable mapping + unique index |
| `src/IoBuild.Devices/Infrastructure/Messaging/FloorProvisioningConsumer.cs` | CREATED — BackgroundService consumer |
| `src/IoBuild.Devices/Application/Internal/CommandServices/DeviceCommandService.cs` | UPDATED — two-phase commit fix (ADR-A, §7.3) |
| `src/IoBuild.Devices/Program.cs` | UPDATED — `using` + `AddHostedService<FloorProvisioningConsumer>()` |
| `src/IoBuild.Devices/Migrations/20260618183750_AddDeviceFloorPlacement.cs` | CREATED — EF migration |
| `src/IoBuild.Devices/Migrations/20260618183750_AddDeviceFloorPlacement.Designer.cs` | CREATED — migration designer |
| `src/IoBuild.Devices/Migrations/DevicesDbContextModelSnapshot.cs` | UPDATED — EF snapshot |

## Test Run Summary (PR 6 final)

```
Correctas! - Con error: 0, Superado: 45, Omitido: 0, Total: 45, Duración: 1s
IoBuild.Devices.Tests.dll (net9.0)
```

dotnet build IoBuild.Devices: 0 Errores, pre-existing MQTTnet NU1603 + CS1998 warnings only.
Migration filename: 20260618183750_AddDeviceFloorPlacement.cs
Package added: Microsoft.EntityFrameworkCore.Sqlite 9.0.5 (test project only)

---

# PR 7 — Analytics Projection Updates

> PR slice: **PR 7 — Analytics Projection Updates**
> Branch: `feat/psol/pr7-analytics`
> Last updated: 2026-06-18

## PR 7 Tasks — Status

- [x] 7.1 [RED] `AnalyticsProjectionUpdateTests` written with 4 test cases — build failed (CS1061: UnitProjection missing Floor/RoomNumber/OwnerEmail; DeviceProjection missing FloorNumber). File: `tests/IoBuild.Analytics.Tests/Infrastructure/AnalyticsProjectionUpdateTests.cs`
- [x] 7.2 [GREEN] `UnitProjection.cs` updated: added `Floor:int?`, `RoomNumber:string?`, `OwnerEmail:string?`. File: `src/IoBuild.Analytics/Domain/Model/Projections/UnitProjection.cs`
- [x] 7.3 [GREEN] `DeviceProjection.cs` updated: added `FloorNumber:int?`. File: `src/IoBuild.Analytics/Domain/Model/Projections/DeviceProjection.cs`
- [x] 7.4 Migration generated: `20260618184424_AddUnitFloorAndOwnerEmailProjections.cs`. Adds `floor` (int, nullable), `room_number` (longtext, nullable), `owner_email` (longtext, nullable) to `unit_projections`; `floor_number` (int, nullable) to `device_projections`. Additive only — no column drops or type changes.
- [x] 7.5 [GREEN] `AnalyticsEventConsumer.cs` updated:
  - `UpsertUnitAsync(UnitCreatedEvent)`: maps `Floor`, `RoomNumber`, `OwnerEmail`; uses conditional `OwnerUserId` update (only overwrites if event value is non-null — preserves value set by `UnitOwnerMatchedEvent` in out-of-order scenario).
  - `UpsertDeviceAsync(DeviceCreatedEvent)`: maps `FloorNumber`.
  - Added `case nameof(UnitOwnerMatchedEvent)` in `ApplyEventByTypeAsync` (byte[] path).
  - Added `UnitOwnerMatchedEvent e => UpsertUnitOwnerAsync(e, db)` in `ApplyEventWithDb` (typed switch).
  - Implemented `UpsertUnitOwnerAsync`: `FindAsync(evt.UnitId)` — creates placeholder row if absent (Status="", LastEventAt=DateTime.MinValue); sets `OwnerUserId` and `OwnerEmail` (if non-empty); updates `LastEventAt`; full LWW guard.
- [x] 7.6 **15/15 green** (11 pre-existing + 4 new). `dotnet build IoBuild.Analytics`: 0 errors. `dotnet build IoBuild.sln`: 0 errors, 48 warnings (all pre-existing). C.1 marked.

## Notes / Discoveries (PR 7)

### Projection entity path: Projections/, not Aggregates/
Tasks.md referenced `Domain/Model/Aggregates/` for projection classes — actual path is `Domain/Model/Projections/`. Corrected in task descriptions.

### Out-of-order LWW strategy for UnitCreatedEvent
When `UnitOwnerMatchedEvent` arrives before `UnitCreatedEvent`, `UpsertUnitOwnerAsync` creates a placeholder with `LastEventAt = evt.OccurredOn` (the owner-matched timestamp). When `UnitCreatedEvent` arrives later with a newer `OccurredOn`, it passes the LWW guard and enriches the row. To avoid clobbering the `OwnerUserId` already set (since `UnitCreatedEvent.OwnerUserId` may be null if the unit was created before an owner was assigned), `UpsertUnitAsync` only overwrites `OwnerUserId` when `evt.OwnerUserId.HasValue`. This is correct for both normal and out-of-order scenarios.

### No new queue bindings needed
Confirmed: `project.#` binding already covers `project.unit.owner-matched` routing key (declared in `ExecuteAsync` via `QueueBindAsync(QueueName, ExchangeName, "project.#")`). No `AnalyticsConsumerExtensions.cs` change needed.

### C.1 (solution build) verified
`dotnet build IoBuild.sln` after PR7 completes: 0 errors. PR7 is the final slice; C.1 is now checkable.

## Files Changed (PR 7)

| File | Change |
|---|---|
| `tests/IoBuild.Analytics.Tests/Infrastructure/AnalyticsProjectionUpdateTests.cs` | CREATED — 4 new tests (RED→GREEN) |
| `src/IoBuild.Analytics/Domain/Model/Projections/UnitProjection.cs` | UPDATED — Floor?, RoomNumber?, OwnerEmail? |
| `src/IoBuild.Analytics/Domain/Model/Projections/DeviceProjection.cs` | UPDATED — FloorNumber? |
| `src/IoBuild.Analytics/Infrastructure/Messaging/AnalyticsEventConsumer.cs` | UPDATED — FloorNumber mapping, Floor/RoomNumber/OwnerEmail mapping, UnitOwnerMatchedEvent case + UpsertUnitOwnerAsync |
| `src/IoBuild.Analytics/Migrations/20260618184424_AddUnitFloorAndOwnerEmailProjections.cs` | CREATED — EF migration |
| `src/IoBuild.Analytics/Migrations/20260618184424_AddUnitFloorAndOwnerEmailProjections.Designer.cs` | CREATED — migration designer |
| `src/IoBuild.Analytics/Migrations/AnalyticsDbContextModelSnapshot.cs` | UPDATED — EF snapshot |

## Test Run Summary (PR 7 final)

```
Correctas! - Con error: 0, Superado: 15, Omitido: 0, Total: 15, Duración: ~1.6s
IoBuild.Analytics.Tests.dll (net9.0)
```

dotnet build IoBuild.Analytics: 0 Errores, 0 Advertencias.
Migration filename: 20260618184424_AddUnitFloorAndOwnerEmailProjections.cs

Solution-wide build:
```
Compilación correcta.
    48 Advertencia(s)
    0 Errores
IoBuild.sln (net9.0)
```
All 48 warnings are pre-existing (CS8618 nullable resources in IoBuild.Projects, CS1998, CS0414 in test steps). Zero warnings introduced by PR7.
