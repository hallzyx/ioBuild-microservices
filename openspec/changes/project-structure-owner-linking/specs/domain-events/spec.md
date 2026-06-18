# Delta for domain-events

**Capability**: MODIFIED `domain-events`
**Change**: project-structure-owner-linking
**Status**: draft

---

## ADDED Requirements

### Requirement: New events — UserRegisteredEvent, UnitOwnerMatchedEvent, FloorStructureDefinedEvent

Three new typed domain events MUST be added to `IoBuild.Shared`, each implementing `IoBuild.Shared.IEvent` and carrying `Guid EventId` and `DateTime OccurredOn` per REQ-DE-01.

| Event | Publisher | Routing key | Payload fields |
|---|---|---|---|
| `UserRegisteredEvent` | IoBuild.IAM | `iam.user.registered` | `UserId: int`, `Email: string`, `Role: string` |
| `UnitOwnerMatchedEvent` | IoBuild.Projects | `project.unit.owner-matched` | `UnitId: int`, `ProjectId: int`, `OwnerUserId: int`, `OwnerEmail: string` |
| `FloorStructureDefinedEvent` | IoBuild.Projects | `project.floor.defined` | `ProjectId: int`, `Floor: int`, `UnitCount: int`, `BuilderId: int` |

All three events MUST be delivered via the transactional outbox+worker pattern (same guarantee as existing events — REQ-DE-02 through REQ-DE-08 apply).

#### Scenario: UserRegisteredEvent flows from IAM to Projects

- GIVEN a new user registers in IAM with role "Owner"
- WHEN IAM persists the user row and the outbox worker runs
- THEN a `UserRegisteredEvent` with non-empty `EventId`, non-zero `UserId`, and valid `Email` is published to routing key `iam.user.registered`
- AND the outbox row is marked Processed after successful publish

#### Scenario: UnitOwnerMatchedEvent flows from Projects to Analytics

- GIVEN Projects has matched a unit to an owner
- WHEN the Projects outbox worker runs
- THEN a `UnitOwnerMatchedEvent` is published to routing key `project.unit.owner-matched`
- AND the payload carries non-zero `UnitId`, `ProjectId`, and `OwnerUserId`

#### Scenario: FloorStructureDefinedEvent published once per floor

- GIVEN a define-structure command creates N floors
- WHEN the Projects outbox worker runs
- THEN exactly N `FloorStructureDefinedEvent` messages are published to routing key `project.floor.defined`
- AND each message carries the correct `Floor` (1-based) and `UnitCount`

---

### Requirement: IAM gains outbox + OutboxWorker

IAM MUST be extended with an outbox table, EF Core migration, and an `OutboxWorker` `BackgroundService` following the same pattern already established in `IoBuild.Devices` and `IoBuild.Projects` (REQ-DE-02, REQ-DE-03, REQ-DE-05, REQ-DE-06). IAM MUST NOT publish events via any other mechanism.

#### Scenario: IAM outbox worker retries on broker failure

- GIVEN IAM's RabbitMQ broker is unreachable
- AND a `UserRegisteredEvent` outbox row is Pending
- WHEN the IAM OutboxWorker polls and publish fails
- THEN the row remains Pending
- AND a Warning is logged
- AND the user registration HTTP response was already 201 (unaffected by broker state)

---

### Requirement: Projects binds iam.user.# queue

Projects MUST declare a dedicated queue bound to the `iam.user.#` routing key pattern on the shared RabbitMQ exchange. This queue MUST be independent from the Analytics queue — each service maintains its own binding and consumer.

#### Scenario: Projects and Analytics both receive UserRegisteredEvent independently

- GIVEN a `UserRegisteredEvent` is published to routing key `iam.user.registered`
- WHEN both the Projects consumer and the Analytics consumer are running
- THEN Projects receives and processes the event on its own queue
- AND Analytics receives and processes the event on its own queue
- AND neither service's processing affects the other

---

## MODIFIED Requirements

### Requirement: REQ-DE-01 — Event contract (extended event table)

All domain events MUST implement `IoBuild.Shared.IEvent`.

Each event MUST carry:
- `Guid EventId` — stable unique identifier for the event occurrence; used by downstream consumers for de-duplication.
- `DateTime OccurredOn` — UTC timestamp set at event creation.
- A typed payload with the resource state at the time of occurrence.

(Previously: event table listed only 6 events from Devices and Projects; IAM was not a publisher.)

Events defined in this capability (complete updated table):

| Event name | Publisher | Trigger |
|---|---|---|
| `DeviceCreatedEvent` | IoBuild.Devices | Device persisted for the first time |
| `DeviceUpdatedEvent` | IoBuild.Devices | Device fields updated and persisted |
| `DeviceDeletedEvent` | IoBuild.Devices | Device removed and persisted |
| `ProjectCreatedEvent` | IoBuild.Projects | Project persisted for the first time |
| `ProjectUpdatedEvent` | IoBuild.Projects | Project fields updated and persisted |
| `UnitCreatedEvent` | IoBuild.Projects | Unit persisted for the first time |
| `UserRegisteredEvent` | IoBuild.IAM | User row persisted for the first time |
| `UnitOwnerMatchedEvent` | IoBuild.Projects | Unit.OwnerId backfilled after email match |
| `FloorStructureDefinedEvent` | IoBuild.Projects | Floor created by define-structure command (one per floor) |

---

### Requirement: UnitCreatedEvent payload extended

`UnitCreatedEvent` MUST include the following additional fields (nullable fields are optional in legacy paths but MUST be populated by the define-structure command):
- `Floor: int` — floor number of the unit.
- `RoomNumber: string` — room identifier within the floor.
- `OwnerEmail: string?` — email pre-assigned at structure definition time, or null.

`UnitCreatedEvent.UnitId` MUST be the real database-assigned identifier (> 0). The event MUST be constructed after `SaveChanges` completes. A `UnitId == 0` in any published event is a defect.

(Previously: `UnitCreatedEvent` had no structural fields; `UnitId` was built before `SaveChanges` causing `UnitId = 0`.)

#### Scenario: UnitCreatedEvent carries Floor, RoomNumber, and non-zero UnitId

- GIVEN the define-structure command creates unit with Floor = 2, RoomNumber = "201", OwnerEmail = "alice@test.com"
- WHEN `SaveChanges` completes and the outbox worker publishes
- THEN `UnitCreatedEvent.UnitId` is greater than zero
- AND `UnitCreatedEvent.Floor = 2`
- AND `UnitCreatedEvent.RoomNumber = "201"`
- AND `UnitCreatedEvent.OwnerEmail = "alice@test.com"`

#### Scenario: UnitId = 0 never appears in outbox

- GIVEN the define-structure command is processing
- WHEN any `UnitCreatedEvent` outbox row is written
- THEN the `UnitId` field in the outbox payload is greater than zero
- AND no row with `UnitId = 0` exists in the outbox table after the transaction commits

---

### Requirement: DeviceCreatedEvent payload extended

`DeviceCreatedEvent` MUST include:
- `FloorNumber: int?` — floor number when the device was provisioned by `floor-default-devices`; null for devices created outside of floor provisioning.

(Previously: `DeviceCreatedEvent` had no floor information.)

#### Scenario: DeviceCreatedEvent carries FloorNumber when provisioned per floor

- GIVEN a device is provisioned by the Devices consumer handling `FloorStructureDefinedEvent{Floor: 3}`
- WHEN the outbox worker publishes the `DeviceCreatedEvent`
- THEN `DeviceCreatedEvent.FloorNumber = 3`

#### Scenario: DeviceCreatedEvent FloorNumber is null for manually created devices

- GIVEN a device is created by a direct builder command (not floor provisioning)
- WHEN the outbox worker publishes the `DeviceCreatedEvent`
- THEN `DeviceCreatedEvent.FloorNumber` is null

---

## Out of scope for this spec

- Event schema versioning or backward-compatibility envelopes.
- Dead-letter queue configuration.
- Routing key changes for existing events (`device.created`, `project.unit.created`, etc.) are NOT changed.
- Analytics projection updates for the new events (covered in `analytics-read-model` delta, separate change).
