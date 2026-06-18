# Project Structure Specification

**Capability**: NEW `project-structure`
**Change**: project-structure-owner-linking
**Status**: draft

---

## Purpose

Allow a builder to define the physical layout of a project (floors and rooms/units) in a single command. The command creates all `Unit` rows with structural coordinates (`Floor`, `RoomNumber`) and optional owner-email pre-assignment, then emits one `FloorStructureDefinedEvent` per floor via the outbox.

---

## Requirements

### REQ-PS-01 — Define-structure command

A builder MUST be able to define the project structure by issuing `POST /api/v1/projects/{id}/structure` with a payload that specifies `floors` (positive integer) and `unitsPerFloor` (positive integer), plus an optional per-unit owner email list.

The system MUST reject requests where `floors < 1` or `unitsPerFloor < 1` with HTTP 422 and a descriptive error body.

### REQ-PS-02 — Unit creation with structural coordinates

When the define-structure command executes, the system MUST create exactly `floors × unitsPerFloor` `Unit` rows. Each row MUST carry:
- `Floor: int` — the floor number (1-based).
- `RoomNumber: string` — unique within the project (e.g. "101", "102", "201").
- `OwnerEmail: string?` — nullable; set from the per-unit assignment in the request, or null if not provided.
- `OwnerId: int?` — nullable; set to null at creation time (linked asynchronously by owner-email-linking).

The `Unit.OwnerId` MUST be nullable; the schema MUST allow a unit to exist without an assigned owner.

### REQ-PS-03 — Idempotency / one-time definition

Structure definition MUST be idempotent in the following sense: if a project already has units (structure previously defined), the system MUST return HTTP 409 Conflict and MUST NOT create additional units or emit additional events.

### REQ-PS-04 — UnitCreatedEvent carries correct persisted UnitId (bug fix)

The `UnitCreatedEvent` MUST be built **after** `SaveChanges` completes so that `UnitId` reflects the real database-assigned value. The event MUST NOT be built before `SaveChanges`; `UnitId == 0` in any published event is a defect.

### REQ-PS-05 — FloorStructureDefinedEvent per floor via outbox

After all units are persisted, the system MUST write one `FloorStructureDefinedEvent{ProjectId, Floor, UnitCount, BuilderId}` outbox row per floor in the same transaction as the unit rows. The `OutboxWorker` delivers these events to RabbitMQ under routing key `project.floor.defined`.

### REQ-PS-06 — Authorization

Only a user with the `Builder` role MAY call `POST /api/v1/projects/{id}/structure`. A caller with any other role MUST receive HTTP 403 Forbidden.

### REQ-PS-07 — Build and test integrity

- `dotnet build` MUST succeed with zero errors.
- `dotnet test` MUST pass; the following behaviors MUST each have a covering xUnit test: REQ-PS-01 (validation), REQ-PS-02 (unit count and fields), REQ-PS-03 (conflict on re-definition), REQ-PS-04 (UnitId non-zero), REQ-PS-05 (outbox rows per floor).

---

## Scenarios

### Scenario PS-S01 — Happy path: builder defines a 3-floor tower with 4 units per floor

- GIVEN a project with id P exists and has zero units
- AND the caller has the Builder role
- WHEN `POST /api/v1/projects/P/structure` with `{ floors: 3, unitsPerFloor: 4 }` is received
- THEN HTTP 200 (or 201) is returned
- AND 12 Unit rows are created with Floor values 1, 2, 3 and RoomNumbers "101"–"104", "201"–"204", "301"–"304"
- AND each Unit has `OwnerId = null` and `OwnerEmail = null`
- AND 3 outbox rows of type `FloorStructureDefinedEvent` are written (one per floor, UnitCount = 4)
- AND the OutboxWorker publishes all 3 events to routing key `project.floor.defined`

### Scenario PS-S02 — Per-unit owner email pre-assignment

- GIVEN a project with id P exists and has zero units
- AND the request payload includes `ownerEmails: [{ floor: 1, roomNumber: "101", email: "alice@test.com" }]`
- WHEN `POST /api/v1/projects/P/structure` is processed
- THEN the Unit for Floor 1 / RoomNumber "101" is created with `OwnerEmail = "alice@test.com"` and `OwnerId = null`
- AND all other units are created with `OwnerEmail = null` and `OwnerId = null`

### Scenario PS-S03 — Validation: floors = 0 is rejected

- GIVEN a project with id P exists
- WHEN `POST /api/v1/projects/P/structure` with `{ floors: 0, unitsPerFloor: 4 }` is received
- THEN HTTP 422 is returned
- AND the response body contains a message indicating `floors` must be greater than zero
- AND no units are created

### Scenario PS-S04 — Validation: unitsPerFloor = 0 is rejected

- GIVEN a project with id P exists
- WHEN `POST /api/v1/projects/P/structure` with `{ floors: 3, unitsPerFloor: 0 }` is received
- THEN HTTP 422 is returned
- AND no units are created

### Scenario PS-S05 — Conflict: structure already defined

- GIVEN a project with id P already has units (structure was previously defined)
- WHEN `POST /api/v1/projects/P/structure` is called again
- THEN HTTP 409 Conflict is returned
- AND no new units are created
- AND no new outbox rows are written

### Scenario PS-S06 — UnitCreatedEvent carries real UnitId (bug fix)

- GIVEN a project with id P exists and has zero units
- WHEN `POST /api/v1/projects/P/structure` is processed and `SaveChanges` completes
- THEN each `UnitCreatedEvent` published to RabbitMQ carries a `UnitId` value greater than zero
- AND no `UnitCreatedEvent` with `UnitId = 0` is ever written to the outbox

### Scenario PS-S07 — Unauthorized role is rejected

- GIVEN the caller has the Owner role (not Builder)
- WHEN `POST /api/v1/projects/P/structure` is called
- THEN HTTP 403 Forbidden is returned
- AND no units are created

---

## Out of scope for this spec

- Builder subscription-gate before structure definition (separate change).
- Modification or deletion of structure after initial definition.
- Owner self-service unit claiming UI.
- Backfill of units for projects created before this change.
