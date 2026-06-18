# Apply Progress: project-structure-owner-linking

> Last slice completed: **PR 3 — Projects `Unit` Schema + Define-Structure Command**
> Branch (PR3): `feat/psol/pr3-unit-schema`
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
