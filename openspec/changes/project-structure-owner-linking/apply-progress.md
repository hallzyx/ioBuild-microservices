# Apply Progress: project-structure-owner-linking

> PR slice: **PR 1 — Projects Test Fixture + `unit.Id==0` Bug Fix**
> Branch: `feat/psol/pr1-test-fixture`
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
