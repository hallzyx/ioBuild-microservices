# Proposal: Project Structure & Email-Based Owner→Unit Linking

> Path note: the OpenSpec store lives at repo-root `openspec/`, not `microservices/openspec/`. This proposal is written there to match the existing `analytics-event-driven` change.

## Intent

Today the project model is FLAT: a `Project` holds a `TotalUnits` count and a loose set of `Unit(UnitNumber)` rows with a raw, non-nullable `OwnerId:int` pointing at an IAM user that may not exist. The builder cannot lay out a tower (floors → rooms), owners are not linked by any real identity, and `Device` has no floor/unit/owner placement (`OwnerUserId` is hardcoded `0`). This change lets a builder **define the project structure (floors + rooms/units)**, links **owners to units by email** (the only stable identity across IAM and Projects), and **provisions a default IoT device set per floor** — all event-driven over the existing outbox→RabbitMQ pipeline.

## Scope

### In Scope
- Builder **defines project structure** in one command: floors + rooms-per-floor → creates all units.
- `Unit` gains `Floor:int`, `RoomNumber:string`, `OwnerEmail:string?`; `OwnerId` becomes **nullable**.
- **Email-based owner→unit linking**: IAM emits user registration; Projects matches `OwnerEmail` and backfills `OwnerId`.
- **Default IoT devices per floor** provisioned via event consumed by Devices.
- Event-contract delta (see below) in `IoBuild.Shared`.
- Fix pre-existing `UnitCommandService` bug (event built with `unit.Id == 0` before SaveChanges).
- Idempotency guard for floor device provisioning.

### Out of Scope
- Subscription gate before structure definition (separate change, not built today).
- End-to-end test of the full journey (separate change).
- Owner self-service unit claiming UI; backfill/replay of historical units.
- Migrating `Client` CRM into the IAM identity model.

## Capabilities

### New Capabilities
- `project-structure`: builder command to define floors + units; `Unit` floor/room/owner-email schema; structure-defined event.
- `owner-linking`: email-based async matching of IAM users to units (`UserRegisteredEvent` → `UnitOwnerMatchedEvent`); Projects consumer.
- `floor-device-provisioning`: Devices consumes `FloorStructureDefinedEvent` and seeds the default device set per floor, idempotently.

### Modified Capabilities
- `domain-events`: add `UserRegisteredEvent`, `UnitOwnerMatchedEvent`, `FloorStructureDefinedEvent`; extend `UnitCreatedEvent` (+`Floor`,`RoomNumber`,`OwnerEmail?`) and `DeviceCreatedEvent` (+`FloorNumber?`). IAM becomes a publisher (gains outbox).
- `analytics-read-model`: `UnitProjection.OwnerUserId` populated via `UnitOwnerMatchedEvent`; `DeviceProjection.UnitId`/floor populated.

## Decisions / Assumptions (veto at review)

1. **Single "define structure" command.** A `POST /projects/{id}/structure` command takes `floors` + `unitsPerFloor` (or explicit room list) and creates ALL units in one transaction, then emits one `FloorStructureDefinedEvent` per floor. Rejected: inferring completion from counts (fragile, no clear "done" signal). Matches the UX of laying out the whole tower at once.
2. **`OwnerEmail` lives on `Unit` only; `Client` stays builder CRM.** `Unit.OwnerEmail` is the IAM linking attribute (source of truth for owner identity). `Client` remains the builder's standalone CRM record (project-scoped), NOT reconciled into units. No FK between them. Keeps concepts cleanly separated.
3. **`Unit.OwnerId` nullable.** A unit exists before its owner registers. Migration sets existing rows' `OwnerId` from current int value (or null if 0/orphaned). Seed data updated accordingly.
4. **Default device set = hard-coded constant now.** Per floor: `1× SmartMeter`, `1× WaterSensor`, `1× SmokeDetector` (3 devices/floor), `Location = "Floor {n}"`. A `FloorDeviceDefaults` constant in Devices; extensible to a config/table later — noted, not built.
5. **IAM emits `UserRegisteredEvent` via full outbox+worker (event-driven).** Consistent with the project's whole recent direction (outbox→RabbitMQ everywhere) and the `domain-events` at-least-once guarantee. **Cost called out: this is the single biggest scope driver** — IAM gains an outbox table, migration, `OutboxWorker`, RabbitMQ wiring, and tests where it currently publishes NOTHING. Rejected: lightweight direct publish (loses at-least-once, breaks outbox consistency) and synchronous HTTP IAM→Projects (reintroduces the cross-service runtime coupling the prior change removed).
6. **First-login UX during the async matching window.** Owner may register a beat before the projection updates. Expected behavior: login/auth succeeds immediately (IAM owns auth); unit association is **eventually consistent** — the owner's dashboard shows "linking your unit…" / empty state until `UnitOwnerMatchedEvent` lands, then populates. No blocking, no error. Matching is also re-checked when a unit with a matching email is created after the user already exists (both directions covered).

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `IoBuild.Shared` | Modified | 3 new events; extend `UnitCreatedEvent`, `DeviceCreatedEvent` |
| `IoBuild.IAM` | Modified | NEW outbox + `OutboxWorker` + RabbitMQ; emit `UserRegisteredEvent` |
| `IoBuild.Projects` | Modified | `Unit` schema (+migration); define-structure command; NEW consumer for `UserRegisteredEvent`; emit `FloorStructureDefinedEvent`, `UnitOwnerMatchedEvent`; fix `unit.Id==0` bug |
| `IoBuild.Devices` | Modified | Consume `FloorStructureDefinedEvent`; seed default devices (idempotent); set `FloorNumber` |
| `IoBuild.Analytics` | Modified | Populate `UnitProjection.OwnerUserId`, `DeviceProjection.UnitId`/floor |
| Projects test infra | New | EF InMemory fixture (replaces shallow SpecFlow stubs) — prerequisite for behavior tests |

## Event-Contract Delta

| Event | Change | Flow |
|---|---|---|
| `UnitCreatedEvent` | +`Floor`, +`RoomNumber`, +`OwnerEmail?` | Projects→Analytics |
| `UserRegisteredEvent` | NEW `{UserId,Email,Role}` | IAM→Projects |
| `UnitOwnerMatchedEvent` | NEW `{UnitId,ProjectId,OwnerUserId,OwnerEmail}` | Projects→Analytics |
| `FloorStructureDefinedEvent` | NEW `{ProjectId,Floor,UnitCount,BuilderId}` | Projects→Devices |
| `DeviceCreatedEvent` | +`FloorNumber?` | Devices→Analytics |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| IAM outbox is large net-new scope | High | Isolate as its own PR slice; reuse Devices/Projects outbox pattern verbatim |
| RabbitMQ redelivery → duplicate floor devices | Med | Idempotency guard (unique key per project+floor+type) before seeding |
| `unit.Id==0` corrupts Analytics floor/owner data | High | Fix included in scope; build event AFTER SaveChanges |
| Async window confuses graders ("owner has no unit yet") | Med | Document eventual-consistency UX (Decision 6) |
| Shallow Projects test infra blocks strict TDD | High | Build EF InMemory fixture FIRST (prerequisite task) |
| Migration on non-nullable→nullable `OwnerId` | Med | Migration + seed update; verify existing rows map cleanly |

## Note for tasks phase

This change is **large** (touches 5 services + Shared + new IAM outbox) and **strict TDD is active**. It will almost certainly exceed the 400-line PR budget — the tasks phase MUST plan **chained/stacked PRs**. Suggested slice order: (1) Projects test-infra fixture + `unit.Id==0` fix; (2) `Unit` schema + define-structure command; (3) Shared events; (4) IAM outbox + `UserRegisteredEvent`; (5) Projects owner-matching consumer; (6) Devices floor provisioning; (7) Analytics projection updates.

## Rollback Plan

Each slice is independently revertible. Schema migration is additive (new nullable columns) + one nullability change — revert via down-migration. IAM outbox is self-contained; reverting it leaves IAM as today (publishes nothing). Owner-matching and floor provisioning are consumers — disabling them leaves structure creation intact, just unlinked/undevice'd.

## Dependencies

- Existing outbox→RabbitMQ pipeline (from `analytics-event-driven`) — reused.
- RabbitMQ client NuGet version already pinned across services — IAM must adopt the same.

## Success Criteria

- [ ] Builder defines floors + rooms in one command; all units created with `Floor`/`RoomNumber`.
- [ ] IAM user registration links to a matching `Unit.OwnerEmail` (both registration-first and unit-first orderings).
- [ ] Default device set seeded once per floor (no duplicates on redelivery).
- [ ] `UnitCreatedEvent` carries correct non-zero `UnitId` (bug fixed).
- [ ] `dotnet build` and `dotnet test` pass across the solution.
