# Owner Email Linking Specification

**Capability**: NEW `owner-email-linking`
**Change**: project-structure-owner-linking
**Status**: draft

---

## Purpose

Link IAM users to project units using email as the cross-service identity anchor. When a user with role `Owner` registers in IAM, an event flows (via the IAM outbox→RabbitMQ pipeline) to Projects, which finds all units whose `OwnerEmail` matches the registration email and backfills `OwnerId`. The same matching runs in the reverse direction: when a unit is assigned an `OwnerEmail` for a user who already exists in IAM, the match is applied immediately. The result is eventually consistent — the owner's first login succeeds instantly; unit association appears after the event is processed.

---

## Requirements

### REQ-OL-01 — IAM emits UserRegisteredEvent via outbox

IAM MUST write a `UserRegisteredEvent{UserId, Email, Role}` outbox row in the same database transaction as the new user row on every successful user registration. The `OutboxWorker` in IAM MUST deliver this event to RabbitMQ under routing key `iam.user.registered`.

IAM MUST NOT publish this event synchronously or via HTTP. The outbox+worker pattern MUST be followed to preserve the at-least-once guarantee already established in `domain-events`.

### REQ-OL-02 — Projects consumes UserRegisteredEvent

Projects MUST subscribe to `iam.user.registered` on a dedicated queue. On receipt of `UserRegisteredEvent` where `Role == "Owner"`, Projects MUST:
1. Query for all units where `OwnerEmail` matches the event `Email` (case-insensitive) and `OwnerId IS NULL`.
2. For each matching unit, set `OwnerId = UserId` and persist the change.
3. Emit a `UnitOwnerMatchedEvent{UnitId, ProjectId, OwnerUserId, OwnerEmail}` outbox row per matched unit.

Events where `Role != "Owner"` MUST be acknowledged and discarded without any unit mutation.

### REQ-OL-03 — Registration-first ordering (user registers before unit is email-assigned)

When a user with role `Owner` has already registered in IAM at the time a unit is assigned `OwnerEmail` via the define-structure command, the system MUST detect the match and set `OwnerId` within the same transaction as unit creation (or immediately after, as an inline lookup in the command handler before committing). The `UnitOwnerMatchedEvent` MUST still be emitted via outbox.

### REQ-OL-04 — Unit-first ordering (unit email-assigned before owner registers)

When a unit already exists with `OwnerEmail = "alice@test.com"` and `OwnerId IS NULL`, and later a `UserRegisteredEvent` arrives for `alice@test.com` with `Role = "Owner"`, the consumer MUST match and update `OwnerId`. This is the async path covered by REQ-OL-02.

### REQ-OL-05 — Case-insensitive email matching

All email comparisons between `Unit.OwnerEmail` and `UserRegisteredEvent.Email` MUST be case-insensitive. `"Alice@Test.com"` MUST match `"alice@test.com"`.

### REQ-OL-06 — Idempotent matching

Processing the same `UserRegisteredEvent` more than once (at-least-once delivery) MUST be idempotent. If `OwnerId` is already set on a unit (matching previously done), the consumer MUST NOT overwrite it with a conflicting value and MUST NOT emit a duplicate `UnitOwnerMatchedEvent` for that unit.

### REQ-OL-07 — First-login eventual-consistency UX

Owner authentication in IAM MUST succeed immediately upon registration regardless of unit-linking status. The owner's unit association is eventually consistent: during the async window between registration and `UnitOwnerMatchedEvent` processing, the owner's dashboard MAY show an empty or "linking…" state. Once `UnitOwnerMatchedEvent` is processed by the read model, the unit association MUST appear. No error or blocking behavior is acceptable during the linking window.

### REQ-OL-08 — Build and test integrity

- `dotnet build` MUST succeed with zero errors.
- `dotnet test` MUST pass; the following behaviors MUST each have a covering xUnit test: REQ-OL-01 (outbox row on registration), REQ-OL-02 (consumer matches unit and emits event), REQ-OL-03 (registration-first ordering), REQ-OL-04 (unit-first ordering), REQ-OL-06 (idempotency on re-delivery).

---

## Scenarios

### Scenario OL-S01 — Unit-first: owner registers after unit is email-assigned

- GIVEN a unit U exists with `OwnerEmail = "alice@test.com"` and `OwnerId = null`
- WHEN IAM persists a new user with `UserId = 42`, `Email = "alice@test.com"`, `Role = "Owner"`
- THEN a `UserRegisteredEvent{UserId: 42, Email: "alice@test.com", Role: "Owner"}` outbox row is written in IAM
- AND the OutboxWorker publishes it to routing key `iam.user.registered`
- AND the Projects consumer receives it and sets `Unit.OwnerId = 42`
- AND a `UnitOwnerMatchedEvent{UnitId: U, OwnerUserId: 42, OwnerEmail: "alice@test.com"}` outbox row is written in Projects
- AND the Projects OutboxWorker publishes it to routing key `project.unit.owner-matched`

### Scenario OL-S02 — Registration-first: unit email-assigned after owner already exists

- GIVEN a user with `UserId = 42`, `Email = "bob@test.com"`, `Role = "Owner"` already registered in IAM
- WHEN `POST /api/v1/projects/P/structure` assigns `OwnerEmail = "bob@test.com"` to unit U
- THEN the define-structure command detects the existing IAM user match
- AND `Unit.OwnerId` is set to 42 at creation time (or immediately after, within the same command scope)
- AND a `UnitOwnerMatchedEvent` is emitted via outbox for unit U

### Scenario OL-S03 — Case-insensitive match

- GIVEN a unit exists with `OwnerEmail = "Carol@Test.COM"` and `OwnerId = null`
- WHEN a `UserRegisteredEvent{Email: "carol@test.com", Role: "Owner"}` arrives
- THEN the unit is matched and `OwnerId` is set
- AND a `UnitOwnerMatchedEvent` is emitted

### Scenario OL-S04 — Non-owner role is discarded

- GIVEN a `UserRegisteredEvent{UserId: 99, Email: "builder@test.com", Role: "Builder"}` arrives in Projects
- WHEN the consumer processes it
- THEN no units are queried or mutated
- AND no `UnitOwnerMatchedEvent` is emitted
- AND the message is acknowledged

### Scenario OL-S05 — Idempotent re-delivery does not double-match

- GIVEN unit U already has `OwnerId = 42` (match previously applied)
- WHEN a `UserRegisteredEvent{UserId: 42, Email: "alice@test.com"}` is re-delivered (at-least-once)
- THEN no update is applied to unit U
- AND no duplicate `UnitOwnerMatchedEvent` is emitted
- AND the message is acknowledged

### Scenario OL-S06 — First-login during async linking window

- GIVEN a user registered in IAM and `UserRegisteredEvent` is in-flight (not yet processed by Projects)
- WHEN the owner calls the login endpoint
- THEN authentication succeeds and a valid token is returned
- AND the owner's unit-linked data in the read model is empty or shows a "pending" state (no error)
- AND once `UnitOwnerMatchedEvent` is processed by the Analytics consumer, the unit appears in the owner's read-model view

---

## Out of scope for this spec

- Owner self-service unit claiming UI.
- Backfill of historical units created before this change.
- Reconciliation of `Client` (builder CRM) records with IAM users.
- Multi-unit ownership (one owner linked to multiple units is supported mechanically but no aggregate view is specified here).
