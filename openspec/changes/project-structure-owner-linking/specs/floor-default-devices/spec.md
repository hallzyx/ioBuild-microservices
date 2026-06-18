# Floor Default Devices Specification

**Capability**: NEW `floor-default-devices`
**Change**: project-structure-owner-linking
**Status**: draft

---

## Purpose

Provision a hard-coded set of IoT devices for each floor automatically when a builder defines the project structure. The Devices service consumes `FloorStructureDefinedEvent` from RabbitMQ and seeds the default device set per floor. Provisioning is idempotent — RabbitMQ redelivery of the same event MUST NOT create duplicate devices.

---

## Requirements

### REQ-FD-01 — Default device set per floor

For each `FloorStructureDefinedEvent` received, the Devices service MUST provision exactly the following devices with `Location = "Floor {n}"` where `{n}` is the floor number:

| Device type  | Count per floor |
|--------------|-----------------|
| SmartMeter   | 1               |
| WaterSensor  | 1               |
| SmokeDetector | 1              |

The set is defined as a constant (`FloorDeviceDefaults`) inside the Devices service. Changing the set requires a code change; no runtime configuration is required at this stage.

### REQ-FD-02 — FloorNumber on DeviceCreatedEvent

Each device provisioned by this capability MUST include `FloorNumber: int` in the `DeviceCreatedEvent` outbox row. This field MUST equal the `Floor` value from the triggering `FloorStructureDefinedEvent`.

### REQ-FD-03 — Idempotent provisioning

Processing the same `FloorStructureDefinedEvent` more than once (at-least-once redelivery from RabbitMQ) MUST NOT create duplicate devices. The system MUST check for an existing device with the same `(ProjectId, Floor, DeviceType)` composite key before inserting. If a device with that key already exists, the insert MUST be skipped and the message MUST be acknowledged without error.

### REQ-FD-04 — Consumer acknowledgement

The Devices consumer MUST acknowledge the `FloorStructureDefinedEvent` message to the broker only after all device rows (and their corresponding outbox rows) have been committed to the database. A partial failure (e.g., DB transaction rolled back) MUST NOT result in message acknowledgement.

### REQ-FD-05 — No cross-service HTTP

The Devices consumer MUST NOT make HTTP calls to Projects or any other service to process `FloorStructureDefinedEvent`. All information needed for provisioning MUST come from the event payload.

### REQ-FD-06 — Build and test integrity

- `dotnet build` MUST succeed with zero errors.
- `dotnet test` MUST pass; the following behaviors MUST each have a covering xUnit test: REQ-FD-01 (3 devices created per floor), REQ-FD-03 (idempotency — no duplicates on redelivery), REQ-FD-02 (FloorNumber present in DeviceCreatedEvent).

---

## Scenarios

### Scenario FD-S01 — Happy path: default devices provisioned for a single floor

- GIVEN the Devices service has no devices for ProjectId = P, Floor = 2
- WHEN a `FloorStructureDefinedEvent{ProjectId: P, Floor: 2, UnitCount: 4, BuilderId: B}` is received
- THEN exactly 3 devices are created: 1 SmartMeter, 1 WaterSensor, 1 SmokeDetector
- AND each device has `Location = "Floor 2"` and `FloorNumber = 2`
- AND 3 `DeviceCreatedEvent` outbox rows are written, each carrying `FloorNumber = 2`
- AND the message is acknowledged to the broker

### Scenario FD-S02 — Multiple floors: each floor receives its own device set

- GIVEN the Devices service has no devices for ProjectId = P
- WHEN 3 `FloorStructureDefinedEvent` messages are received for Floor 1, 2, and 3 of project P
- THEN 9 devices total are created (3 per floor)
- AND devices for Floor 1 have `Location = "Floor 1"`, Floor 2 have `Location = "Floor 2"`, etc.

### Scenario FD-S03 — Idempotency: redelivered event does not create duplicate devices

- GIVEN devices for ProjectId = P, Floor = 1 already exist (SmartMeter, WaterSensor, SmokeDetector)
- WHEN `FloorStructureDefinedEvent{ProjectId: P, Floor: 1}` is redelivered (at-least-once)
- THEN no new device rows are created
- AND the existing 3 device rows remain unchanged
- AND the message is acknowledged without error

### Scenario FD-S04 — FloorNumber is present on DeviceCreatedEvent

- GIVEN a `FloorStructureDefinedEvent{ProjectId: P, Floor: 3}` is received
- WHEN the Devices consumer provisions the default set
- THEN each `DeviceCreatedEvent` published to RabbitMQ carries `FloorNumber = 3`
- AND no `DeviceCreatedEvent` with `FloorNumber = null` or `FloorNumber = 0` is emitted for this provisioning

### Scenario FD-S05 — Partial DB failure: message not acknowledged

- GIVEN the database throws an exception mid-transaction while inserting floor devices
- WHEN the Devices consumer handles the exception
- THEN the transaction is rolled back
- AND the message is NOT acknowledged (it will be redelivered)
- AND no partial device rows remain committed

---

## Out of scope for this spec

- Runtime-configurable device set (noted as future extension; not built now).
- Per-unit device provisioning.
- Device decommissioning on structure removal.
- Devices created by builders outside the default set are unaffected by this spec.
