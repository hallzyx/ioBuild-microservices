# Tasks: Project Structure & Email-Based Owner→Unit Linking

> Covers capabilities: `project-structure`, `owner-email-linking`, `floor-default-devices`, `domain-events`
> Artifact store: OpenSpec. Strict TDD is ACTIVE — every behavioral item follows RED → GREEN order.
> Design section references (§N) map to `design.md`.

---

## Review Workload Forecast

| Field | Value |
|---|---|
| Estimated changed lines | 1 800 – 2 400 (7 slices) |
| 400-line budget risk | High |
| Chained PRs recommended | Yes |
| Suggested split | PR 1 → PR 2 → PR 3 → PR 4 → PR 5 → PR 6 → PR 7 |
| Delivery strategy | ask-on-risk |
| Chain strategy | feature-branch-chain |

Decision needed before apply: Yes
Chained PRs recommended: Yes
Chain strategy: feature-branch-chain
400-line budget risk: High

### Suggested Work Units

| Unit | Goal | Likely PR | Notes |
|---|---|---|---|
| 1 | Projects test fixture + `unit.Id==0` bug fix | PR 1 → tracker | Foundation; no schema change; unblocks all Projects tests |
| 2 | Shared event contracts | PR 2 → PR 1 | Pure-additive; unblocks all service slices |
| 3 | Projects `Unit` schema + define-structure command | PR 3 → PR 2 | Migration, REST endpoint, seed reconciliation |
| 4 | IAM outbox + `UserRegisteredEvent` | PR 4 → PR 2 | Biggest slice; self-contained |
| 5 | Projects owner-linking consumer | PR 5 → PR 3+4 | Requires slices 3 and 4 |
| 6 | Devices floor provisioning | PR 6 → PR 2+3 | Consumer, migration, `DeviceCommandService` fix |
| 7 | Analytics projection updates | PR 7 → PR 2 | Migration + new consumer cases; no new bindings |

---

## PR 1 — Projects Test Fixture + `unit.Id==0` Bug Fix

**Goal**: Promote existing EF-InMemory pattern into a shared fixture; fix `UnitCreatedEvent.UnitId=0` in `UnitCommandService`; update the affected test. No schema change. Pure foundation.
**Services/files**: `IoBuild.Projects`, `IoBuild.Projects.Tests`
**Test additions**: `ProjectsDbFixtureTests` (smoke-test the fixture); update `OutboxWriteInTransactionTests` → assert `UnitId != 0` against real EF
**Estimated lines**: ~150
**Rollback**: delete the fixture file; revert `UnitCommandService.cs`; revert test

- [x] 1.1 [RED] In `IoBuild.Projects.Tests/Infrastructure/ProjectsDbFixture.cs`: write a failing test asserting `NewContext(name)` returns a usable `AppDbContext` (InMemory) where a `Unit` can be saved and read back.
- [x] 1.2 [GREEN] Create `IoBuild.Projects.Tests/Infrastructure/ProjectsDbFixture.cs` — static `NewContext(string dbName)` builder wrapping `DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(dbName)`, plus seed helpers for `Project`, `Unit`, `RegisteredOwner` rows (stubs for now).
- [x] 1.3 [RED] In `OutboxWriteInTransactionTests`: added `Handle_CreateUnit_CallsCompleteAsyncTwice_TwoPhaseCommit` asserting `CompleteAsync` called twice (FAILED on pre-fix code). Also added `Handle_CreateUnit_WritesOutboxWithRealId` with real EF context.
- [x] 1.4 [GREEN] In `IoBuild.Projects/Application/Services/UnitCommandService.cs` (§7.2): reordered to call `_unitOfWork.CompleteAsync()` FIRST (saves unit, real `Id` assigned), THEN build `UnitCreatedEvent` with `UnitId = unit.Id`, THEN save outbox row, THEN second `CompleteAsync()`.
- [x] 1.5 Verify `OutboxWriteInTransactionTests` is green; `dotnet test IoBuild.Projects.Tests` passes. **11/11 green.**
- [x] 1.6 Document the two-phase commit reasoning as a code comment in `UnitCommandService.cs` referencing ADR-A.

---

## PR 2 — Shared Event Contracts

**Goal**: Add 3 new event records; extend `UnitCreatedEvent` and `DeviceCreatedEvent`. Pure-additive; nothing consumes the new fields yet.
**Services/files**: `IoBuild.Shared/Domain/Model/Events/`
**Test additions**: serialisation round-trip tests for all 5 changed/new records
**Estimated lines**: ~120
**Rollback**: revert `IoBuild.Shared`; no DB change

- [x] 2.1 [RED] Write serialisation tests in `IoBuild.Shared.Tests` (or equivalent) for `UserRegisteredEvent`, `UnitOwnerMatchedEvent`, `FloorStructureDefinedEvent` asserting `EventId != Guid.Empty`, correct `RoutingKey`, and JSON round-trip of all payload fields.
- [x] 2.2 [RED] Write tests for extended `UnitCreatedEvent` asserting `Floor`, `RoomNumber`, `OwnerEmail` fields serialise/deserialise; existing fields unchanged.
- [x] 2.3 [RED] Write test for `DeviceCreatedEvent` asserting `FloorNumber?` serialises as null when not set.
- [x] 2.4 [GREEN] Add `IoBuild.Shared/Domain/Model/Events/UserRegisteredEvent.cs` (§6.1): `UserId`, `Email` (lower-cased), `Role`; `RoutingKey = "iam.user.registered"`.
- [x] 2.5 [GREEN] Add `IoBuild.Shared/Domain/Model/Events/UnitOwnerMatchedEvent.cs` (§6.1): `UnitId`, `ProjectId`, `OwnerUserId`, `OwnerEmail`; `RoutingKey = "project.unit.owner-matched"`.
- [x] 2.6 [GREEN] Add `IoBuild.Shared/Domain/Model/Events/FloorStructureDefinedEvent.cs` (§6.1): `ProjectId`, `Floor`, `UnitCount`, `BuilderId`; `RoutingKey = "project.floor.defined"`.
- [x] 2.7 [GREEN] Extend `UnitCreatedEvent.cs`: add `Floor:int`, `RoomNumber:string`, `OwnerEmail:string?` init properties (§6.2).
- [x] 2.8 [GREEN] Extend `DeviceCreatedEvent.cs`: add `FloorNumber:int?` init property (§6.2).
- [x] 2.9 Verify all serialisation tests are green; `dotnet build IoBuild.Shared` zero errors.

---

## PR 3 — Projects `Unit` Schema + Define-Structure Command

**Goal**: `Unit` aggregate gains `Floor`, `RoomNumber`, `OwnerEmail`, nullable `OwnerId`; `RegisteredOwner` mirror entity; EF config; migration; seed reconciliation; REST endpoint; unit-first owner matching in command handler (§1, §3.3).
**Services/files**: `IoBuild.Projects` — aggregate, EF config, migration, seed, command, handler, controller
**Test additions**: `DefineProjectStructureCommandTests`, `UnitAggregateTests`, seed-data schema smoke test
**Estimated lines**: ~350
**Rollback**: `dotnet ef migrations remove`; revert aggregate, EF config, seed, handler, controller

- [x] 3.1 [RED] Write `UnitAggregateTests`: `ComposeUnitNumber(5,"02") == "5-02"`; `OwnerId` is null on construction; `LinkOwner(42)` sets `OwnerId=42`; `AssignOwnerEmail` lower-cases the value.
- [x] 3.2 [GREEN] Update `IoBuild.Projects/Domain/Model/Aggregates/Unit.cs` (§1.1): add `Floor`, `RoomNumber`, `OwnerEmail`, change `OwnerId` to `int?`; add `LinkOwner`, `AssignOwnerEmail`, `ComposeUnitNumber`; keep legacy ctor with `Floor=0, RoomNumber=unitNumber`.
- [x] 3.3 [GREEN] Add `IoBuild.Projects/Domain/Model/Entities/RegisteredOwner.cs` (§3.3): `Email` (PK, lower-cased), `UserId`, `LastEventAt`.
- [x] 3.4 [GREEN] Update `IoBuild.Projects/Infrastructure/Persistence/AppDbContext.cs` (§1.4): extend `Unit` EF config (new columns, nullable `OwnerId`, unique index `(ProjectId,Floor,RoomNumber)`, index on `OwnerEmail`); add `RegisteredOwner` block (`HasKey(Email)`, maxlen 255, table `registered_owner`).
- [x] 3.5 [GREEN] Update `IoBuild.Projects/Infrastructure/Persistence/EFC/Configuration/Seed/ProjectsSeedData.cs` (§1.6): add explicit `Floor`, `RoomNumber` values to all 5 existing seed units; set `OwnerEmail=null`; keep existing `OwnerId` values.
- [x] 3.6 Run `dotnet ef migrations add AddUnitStructureAndOwnerLinking --project IoBuild.Projects`; migration generated: `20260618180117_AddUnitStructureAndOwnerLinking.cs`. Matches §1.5: addColumn `floor`/`room_number`/`owner_email`; alterColumn `owner_id` nullable; create `registered_owner` table; add unique index `(project_id, floor, room_number)` and index `(owner_email)`.
- [x] 3.7 [RED] Write `DefineProjectStructureCommandTests` (uses `ProjectsDbFixture`): (a) happy path creates `floors × unitsPerFloor` units with correct `Floor`/`RoomNumber`; (b) `UnitId > 0` in each outbox `UnitCreatedEvent`; (c) one `FloorStructureDefinedEvent` outbox row per floor; (d) HTTP 409 when project already has units (REQ-PS-03); (e) 422 on `floors < 1`; (f) unit-first owner match: if `RegisteredOwner` row exists for the assigned email, `OwnerId` is set immediately and `UnitOwnerMatchedEvent` outbox row is written.
- [x] 3.8 [GREEN] Add `IoBuild.Projects/Domain/Services/Commands/Projects/DefineProjectStructureCommand.cs`: records `RoomSpec`, `FloorSpec`, `DefineProjectStructureCommand` (§1.3).
- [x] 3.9 [GREEN] Add `IoBuild.Projects/Application/Services/ProjectStructureCommandService.cs` (§1.3): validate floors/rooms > 0; guard 409 if project already has units; two-phase commit (units first, then outbox rows); build `UnitCreatedEvent` after first `CompleteAsync`; build `FloorStructureDefinedEvent` per floor; unit-first matching via `RegisteredOwner` lookup (§3.3 inline path).
- [x] 3.10 [GREEN] Added `POST /api/v1/projects/{id}/structure` endpoint in `ProjectsController.cs`; Builder-role JWT guard (reads `HttpContext.Items["UserRole"]`); maps request → `DefineProjectStructureCommand`. Registered `ProjectStructureCommandService` in `Program.cs`.
- [x] 3.11 Verify all scenario tests PS-S01…PS-S07 are green; `dotnet test IoBuild.Projects.Tests`. **28/28 green.**

---

## PR 4 — IAM Outbox + UserRegisteredEvent

**Goal**: Mirror Projects/Devices outbox stack in IAM; `SignUp` emits `UserRegisteredEvent` via two-phase commit; `OutboxWorker` wired in `Program.cs`; migration; optional backfill for seeded users.
**Services/files**: `IoBuild.IAM` — entity, repo, worker, DbContext, migration, `UserCommandService`, `Program.cs`
**Test additions**: IAM `SignUp` outbox test; `OutboxWorker` publish test
**Estimated lines**: ~300
**Rollback**: `dotnet ef migrations remove -p IoBuild.IAM`; revert all 5 new IAM files + `UserCommandService` + `Program.cs`

- [ ] 4.1 [RED] Write `IamSignUpOutboxTests` (EF-InMemory `ApplicationDbContext`): after `Handle(SignUpCommand)`, one `outbox_message` row exists with `EventType = "UserRegisteredEvent"`, payload `UserId > 0`, `Email` lower-cased.
- [ ] 4.2 [RED] Write `IamOutboxWorkerTests` (mirror `OutboxWorkerPublishTests` from Devices): pending row is published to mock publisher, then marked Processed.
- [ ] 4.3 [GREEN] Copy `IoBuild.Projects/Domain/Model/Entities/OutboxMessage.cs` → `IoBuild.IAM/Domain/Model/Entities/OutboxMessage.cs` (exact copy, §2.1).
- [ ] 4.4 [GREEN] Add `IoBuild.IAM/Domain/Repositories/IOutboxMessageRepository.cs` (mirrors Projects interface).
- [ ] 4.5 [GREEN] Add `IoBuild.IAM/Infrastructure/Persistence/EFC/Repositories/OutboxMessageRepository.cs` (mirrors Projects impl).
- [ ] 4.6 [GREEN] Add `IoBuild.IAM/Workers/OutboxWorker.cs` (§2.1): `EventTypeMap` contains only `UserRegisteredEvent`.
- [ ] 4.7 [GREEN] Update `IoBuild.IAM/Infrastructure/Persistence/EFC/ApplicationDbContext.cs` (§2.2): add `DbSet<OutboxMessage>`, configure entity (key, column lengths, index `(Status, CreatedAt)`).
- [ ] 4.8 Run `dotnet ef migrations add AddOutbox -p IoBuild.IAM`; verify creates `outbox_message` table only.
- [ ] 4.9 [GREEN] Update `IoBuild.IAM/Application/Services/UserCommandService.cs` (§2.3): inject `IOutboxMessageRepository`; two-phase commit — `CompleteAsync` (user), build `UserRegisteredEvent` with real `UserId` and lower-cased email, save outbox row, second `CompleteAsync`.
- [ ] 4.10 [GREEN] Update `IoBuild.IAM/Program.cs` (§2.4): register `IOutboxMessageRepository`, `AddDomainEventPublishing(builder.Configuration)`, `AddHostedService<OutboxWorker>()`, run `OutboxBackfill` after `db.Database.Migrate()`.
- [ ] 4.11 Add `OutboxBackfill.cs` to IAM (optional but recommended for seeded-user demo linking).
- [ ] 4.12 Verify IAM outbox and worker tests are green; `dotnet build IoBuild.IAM` zero errors.

---

## PR 5 — Projects Owner-Linking Consumer

**Goal**: `OwnerLinkingConsumer` subscribes to `iam.user.#`; upserts `registered_owner` mirror; matches units by email; emits `UnitOwnerMatchedEvent`; add `UnitOwnerMatchedEvent` to Projects `OutboxWorker.EventTypeMap`.
**Services/files**: `IoBuild.Projects` — `OwnerLinkingConsumer.cs`, `OwnerLinkingConsumerExtensions.cs`, `OutboxWorker.cs`, `Program.cs`
**Test additions**: `OwnerLinkingConsumerTests` covering all OL scenarios
**Estimated lines**: ~250
**Rollback**: remove consumer file and extension; revert `OutboxWorker.EventTypeMap`; revert `Program.cs`

- [ ] 5.1 [RED] Write `OwnerLinkingConsumerTests` (uses internal constructor with direct `AppDbContext`, mirrors `AnalyticsEventConsumer` test seam — §9.3):
  - OL-S01: unit-first — existing unit with `OwnerEmail="alice@test.com"`, `OwnerId=null`; process `UserRegisteredEvent{UserId:42,Email:"alice@test.com",Role:"Owner"}` → `OwnerId=42`, one `UnitOwnerMatchedEvent` outbox row.
  - OL-S02: registration-first — `registered_owner` row exists for email; no unit matches on event, but the row is upserted/confirmed (§3.3 inline path is the primary handler; consumer covers the async path).
  - OL-S03: case-insensitive — `OwnerEmail="Carol@Test.COM"`, event `email="carol@test.com"` → matches.
  - OL-S04: non-owner role Builder → no mutation, message acked.
  - OL-S05: redelivery with `OwnerId` already set → no overwrite, no duplicate event, acked.
- [ ] 5.2 [GREEN] Add `IoBuild.Projects/Infrastructure/Messaging/OwnerLinkingConsumer.cs` (§3.1–3.2): `BackgroundService`, topology `projects.owner-linking / iam.user.#`; handle `UserRegisteredEvent`; upsert `registered_owner` (only for `role=="owner"`); query `Units` by lower-cased email where `OwnerId==null`; call `unit.LinkOwner()`; write `UnitOwnerMatchedEvent` outbox per matched unit; single `CompleteAsync`; transient/poison nack handling.
- [ ] 5.3 [GREEN] Add `IoBuild.Projects/Infrastructure/Messaging/OwnerLinkingConsumerExtensions.cs`: `AddOwnerLinkingConsumer()` extension method.
- [ ] 5.4 [GREEN] Update `IoBuild.Projects/Workers/OutboxWorker.cs` `EventTypeMap`: add `FloorStructureDefinedEvent` and `UnitOwnerMatchedEvent` entries.
- [ ] 5.5 [GREEN] Update `IoBuild.Projects/Program.cs`: call `AddOwnerLinkingConsumer()`.
- [ ] 5.6 Verify all OL scenario tests are green; `dotnet test IoBuild.Projects.Tests`.

---

## PR 6 — Devices Floor Provisioning

**Goal**: `FloorProvisioningConsumer` seeds default devices per floor; `Device` aggregate gains `FloorNumber?` and `UnitId?`; idempotency guard; fix `DeviceCommandService` `Id==0` bug; Devices migration; `DeviceCreatedEvent` carries `FloorNumber`.
**Services/files**: `IoBuild.Devices` — aggregate, EF config, migration, consumer, command service
**Test additions**: `FloorProvisioningConsumerTests` (SQLite-in-memory for unique constraint); `DeviceCommandServiceOutboxTests`
**Estimated lines**: ~300
**Rollback**: `dotnet ef migrations remove -p IoBuild.Devices`; revert consumer, aggregate, command service

- [ ] 6.1 [RED] Write `DeviceCommandServiceOutboxTests` (EF-InMemory): after `Handle(CreateDeviceCommand)`, `DeviceCreatedEvent` payload `DeviceId > 0`.
- [ ] 6.2 [GREEN] Fix `IoBuild.Devices/Application/Services/DeviceCommandService.cs` (§7.3): apply same two-phase commit as `UnitCommandService` — `SaveChangesAsync` first, then build `DeviceCreatedEvent` with real `device.Id`.
- [ ] 6.3 [RED] Write `FloorProvisioningConsumerTests` (SQLite-in-memory for unique constraint — §9.2):
  - FD-S01: `FloorStructureDefinedEvent{ProjectId:P,Floor:2}` → exactly 3 devices created, `FloorNumber=2`, `Location="Floor 2"`, 3 outbox rows.
  - FD-S02: 3 events for floors 1–3 → 9 devices total.
  - FD-S03: redelivery → no duplicates (unique constraint enforced; pre-check guard path).
  - FD-S04: each `DeviceCreatedEvent` payload has `FloorNumber` equal to event `Floor`.
- [ ] 6.4 [GREEN] Update `IoBuild.Devices/Domain/Model/Aggregates/Device.cs` (§4.4): add `FloorNumber:int?` and `UnitId:int?` properties; extend ctor to accept them (nullable, default null).
- [ ] 6.5 [GREEN] Update EF config for `Device` (§4.4): map `FloorNumber` and `UnitId` as nullable; add unique index `(ProjectId, FloorNumber, Type)`.
- [ ] 6.6 Run `dotnet ef migrations add AddDeviceFloorPlacement -p IoBuild.Devices`; verify additive columns + unique index.
- [ ] 6.7 [GREEN] Add `IoBuild.Devices/Domain/Constants/FloorDeviceDefaults.cs` (§4.2): static `Defaults` list with SmartMeter, WaterSensor, SmokeDetector.
- [ ] 6.8 [GREEN] Add `IoBuild.Devices/Infrastructure/Messaging/FloorProvisioningConsumer.cs` (§4.1–4.3): `BackgroundService`, topology `devices.provisioning / project.floor.defined`; idempotency pre-check `ExistsByProjectFloorType`; create 3 `Device` rows with `FloorNumber`; two-phase commit (devices, then outbox rows with `DeviceCreatedEvent.FloorNumber` set); catch unique-constraint `DbUpdateException` → ack as already-provisioned; transient/poison nack.
- [ ] 6.9 [GREEN] Register `FloorProvisioningConsumer` in `IoBuild.Devices/Program.cs`.
- [ ] 6.10 Verify all FD scenario tests and device command service fix tests are green; `dotnet build IoBuild.Devices` zero errors.

---

## PR 7 — Analytics Projection Updates

**Goal**: New projection columns on `unit_projections` and `device_projections`; Analytics migration; new `UnitOwnerMatchedEvent` consumer case; enrich existing `UnitCreatedEvent` and `DeviceCreatedEvent` handlers with new fields. No new bindings needed — `project.#` already covers `project.unit.owner-matched`.
**Services/files**: `IoBuild.Analytics` — migration, projections, `AnalyticsEventConsumer`
**Test additions**: extend `AnalyticsEventConsumer` tests for new cases
**Estimated lines**: ~200
**Rollback**: `dotnet ef migrations remove -p IoBuild.Analytics`; revert projection classes and consumer switch

- [ ] 7.1 [RED] Write analytics consumer tests (use existing internal-constructor seam — §9.3):
  - `UnitCreatedEvent` with `Floor=3`, `RoomNumber="301"`, `OwnerEmail="x@y.com"` → `UnitProjection.Floor=3`, `.RoomNumber="301"`, `.OwnerEmail="x@y.com"`.
  - `UnitOwnerMatchedEvent{UnitId:U,OwnerUserId:42}` where projection row already exists → `OwnerUserId=42` set.
  - Out-of-order: `UnitOwnerMatchedEvent` arrives before `UnitCreatedEvent` → projection row is created with `OwnerUserId`; later `UnitCreatedEvent` enriches it (LWW on `OccurredOn`).
  - `DeviceCreatedEvent` with `FloorNumber=2` → `DeviceProjection.FloorNumber=2`.
- [ ] 7.2 [GREEN] Update `IoBuild.Analytics/Domain/Model/Aggregates/UnitProjection.cs` (§5.1): add `Floor:int?`, `RoomNumber:string?`, `OwnerEmail:string?`.
- [ ] 7.3 [GREEN] Update `IoBuild.Analytics/Domain/Model/Aggregates/DeviceProjection.cs` (§5.1): add `FloorNumber:int?`.
- [ ] 7.4 Run `dotnet ef migrations add AddUnitFloorAndOwnerEmailProjections -p IoBuild.Analytics`; verify adds 3 nullable columns to `unit_projections` and 1 to `device_projections`.
- [ ] 7.5 [GREEN] Update `IoBuild.Analytics/Infrastructure/Messaging/AnalyticsEventConsumer.cs` (§5.2):
  - In `UpsertUnitAsync(UnitCreatedEvent)`: map `row.Floor`, `row.RoomNumber`, `row.OwnerEmail`.
  - In `UpsertDeviceAsync(DeviceCreatedEvent)`: map `row.FloorNumber`.
  - Add `case nameof(UnitOwnerMatchedEvent)` in `ApplyEventByTypeAsync` and `ApplyEventWithDb`; implement `UpsertUnitOwnerAsync` — `FindAsync(evt.UnitId)` (create projection if absent), set `OwnerUserId = evt.OwnerUserId` with LWW guard on `OccurredOn`.
- [ ] 7.6 Verify all analytics consumer tests are green; `dotnet build IoBuild.Analytics` zero errors.

---

## Cross-Cutting Verification (after all PRs merged)

- [ ] C.1 `dotnet build` solution-wide: zero errors.
- [ ] C.2 `dotnet test` solution-wide: all tests pass.
- [ ] C.3 Migration deploy order validated: Projects → IAM → Devices → Analytics.
- [ ] C.4 RabbitMQ queue/binding topology matches §6.3 authoritative table (declared idempotently at consumer startup — no manual broker setup required).
- [ ] C.5 End-to-end smoke: `POST /structure` → devices provisioned → owner registers → `UnitProjection.OwnerId` updated in Analytics.
