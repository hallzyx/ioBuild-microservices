# Spec: analytics-read-model

**Change**: analytics-event-driven
**Capability**: NEW `analytics-read-model`
**Status**: draft (revised — hardened idempotency for at-least-once delivery)

---

## Context

`IoBuild.Analytics` will own local projection tables inside the `iobuild_analytics` database schema. A `BackgroundService` consumer subscribes to the RabbitMQ exchange, receives domain events published by Devices and Projects, and upserts rows into those local tables. This eliminates the runtime HTTP dependency on the source services and makes Analytics self-sufficient for query serving.

The consumer pattern mirrors the existing `TelemetryWorker` already present in Analytics.

**Delivery model**: Events are produced by a Transactional Outbox (`domain-events` spec). The outbox guarantees **at-least-once delivery** — the same event MAY arrive more than once (e.g., when the `OutboxWorker` restarts after publishing but before marking the row processed). Idempotency in this consumer is therefore **not optional** — duplicate events WILL occur in normal operation.

---

## Requirements

### REQ-RM-01 — Analytics-owned tables

The Analytics service MUST own read-model tables within its own database (`iobuild_analytics`). No read-model table SHALL reside in a database owned by another service.

Minimum table coverage required:

| Table (logical) | Projected from events |
|---|---|
| `DeviceProjection` | `DeviceCreated`, `DeviceUpdated`, `DeviceDeleted` |
| `ProjectProjection` | `ProjectCreated`, `ProjectUpdated` |
| `UnitProjection` | `UnitCreated` |

Exact schema (columns, types, indexes) is a design-phase decision. This spec requires only that the tables hold enough data to answer the queries currently served by `analytics-query`.

### REQ-RM-02 — Consumer BackgroundService

Analytics MUST include a long-running `BackgroundService` that:
- Connects to the RabbitMQ exchange on startup.
- Receives domain events matching `DeviceCreated`, `DeviceUpdated`, `DeviceDeleted`, `ProjectCreated`, `ProjectUpdated`, `UnitCreated`.
- Deserializes each event.
- Upserts the corresponding projection row.
- Acknowledges the message to the broker after a successful upsert.

The consumer MUST NOT acknowledge a message before the upsert is confirmed by the database.

### REQ-RM-03 — Idempotency (at-least-once delivery — REQUIRED, not best-effort)

The consumer MUST be fully idempotent. The `domain-events` Transactional Outbox guarantees at-least-once delivery; duplicate event arrivals are a normal operating condition, not an edge case.

Processing the same event (same `Id`) more than once MUST yield **identical final projection state** as processing it exactly once. No exception, no duplicate row, no partial update, no data corruption is acceptable.

The idempotency strategy MUST combine both of the following guards:

1. **Upsert by natural key**: all writes to projection tables MUST use insert-or-update keyed on the source entity identifier (e.g., `DeviceId`, `ProjectId`, `UnitId`). A re-delivered event for an already-projected entity MUST update in place, never insert a second row.

2. **last_event_at guard**: each projection row MUST store the `OccurredAt` timestamp of the last event that updated it. An incoming event with `OccurredAt` older than or equal to the stored `last_event_at` MUST be discarded without mutating the row. This prevents out-of-order or stale re-deliveries from overwriting newer state.

The event `Id` field MAY additionally be used for coarse-grained duplicate detection (e.g., a short-lived seen-event cache or a processed-events table), but it is NOT a substitute for the upsert + timestamp guard above.

### REQ-RM-04 — Delete projection

When a `DeviceDeleted` event is received, the corresponding `DeviceProjection` row MUST be removed or marked as deleted such that subsequent queries do not include that device in active counts.

The chosen deletion strategy (hard delete vs. soft delete flag) is a design-phase decision. Either is acceptable provided the query layer reflects the correct count.

### REQ-RM-05 — Consumer resilience

A transient failure during upsert (e.g., DB timeout) MUST NOT cause the consumer to terminate. The consumer MUST retry or re-queue the message and continue processing subsequent messages.

A persistent failure (e.g., DB unreachable for an extended period) MUST be logged and SHOULD cause the consumer's health check to reflect unhealthy status; it MUST NOT bring down the entire Analytics process.

### REQ-RM-06 — Empty read model is a valid state

The read model MAY be empty at startup (no events received yet). An empty read model MUST NOT cause consumer startup to fail. See `analytics-query` spec for query-layer behavior when the model is empty.

### REQ-RM-07 — Build and test integrity

After this capability is implemented:
- `dotnet build` MUST succeed with zero errors.
- `dotnet test` MUST pass.
- The following behaviors MUST each have a covering xUnit test:
  - Upsert-on-duplicate (REQ-RM-03, guard 1): same event delivered twice produces one row, not two.
  - Stale-event discard (REQ-RM-03, guard 2): a re-delivered event with an older `OccurredAt` does NOT overwrite the current projection state.
  - DeviceDeleted removes the row from active query results (REQ-RM-04).
  - Transient DB failure does not terminate the consumer (REQ-RM-05).

---

## Scenarios

### Scenario RM-S01 — DeviceCreated event upserts a new projection row

```
Given the DeviceProjection table has no row with DeviceId = X
When the consumer receives a DeviceCreated event with Id = E1 and DeviceId = X
Then a row is inserted into DeviceProjection with DeviceId = X and the event's payload fields
And the message is acknowledged to the broker
```

### Scenario RM-S02 — Duplicate DeviceCreated event is idempotent (at-least-once re-delivery)

```
Given the OutboxWorker published DeviceCreated with Id = E1 and DeviceId = X
And the consumer already processed that event (DeviceProjection row exists for DeviceId = X)
And the OutboxWorker crashed before marking the outbox row Processed, so it re-publishes E1
When the consumer receives the same DeviceCreated event (Id = E1, same OccurredAt) a second time
Then the DeviceProjection row for DeviceId = X is unchanged (no field mutations)
And no duplicate row is created
And the message is acknowledged to the broker
```

### Scenario RM-S02b — Stale re-delivered DeviceUpdated event does not overwrite newer state

```
Given DeviceProjection for DeviceId = X has last_event_at = T2 (set by a newer DeviceUpdated event)
When the consumer receives a DeviceUpdated event for DeviceId = X with OccurredAt = T1 (where T1 < T2)
Then the DeviceProjection row for DeviceId = X is NOT updated (stale event discarded)
And the row retains the state set by the T2 event
And the message is acknowledged to the broker (discard is silent, not an error)
```

### Scenario RM-S03 — DeviceUpdated event overwrites projection fields

```
Given the DeviceProjection table has a row with DeviceId = X and Name = "Old Name"
When the consumer receives a DeviceUpdated event with DeviceId = X and Name = "New Name"
Then the DeviceProjection row for DeviceId = X has Name = "New Name"
And no additional row is created
```

### Scenario RM-S04 — DeviceDeleted event removes or marks the projection row

```
Given the DeviceProjection table has a row with DeviceId = X
When the consumer receives a DeviceDeleted event with DeviceId = X
Then DeviceId = X is no longer included in active device query results
And the message is acknowledged to the broker
```

### Scenario RM-S05 — ProjectCreated event upserts a new projection row

```
Given the ProjectProjection table has no row with ProjectId = P
When the consumer receives a ProjectCreated event with ProjectId = P
Then a row is inserted into ProjectProjection with ProjectId = P and the event's payload fields
And the message is acknowledged to the broker
```

### Scenario RM-S06 — UnitCreated event upserts a new projection row

```
Given the UnitProjection table has no row with UnitId = U
When the consumer receives a UnitCreated event with UnitId = U
Then a row is inserted into UnitProjection with UnitId = U and the event's payload fields
And the message is acknowledged to the broker
```

### Scenario RM-S07 — Transient DB failure does not terminate consumer

```
Given the database throws a transient exception during upsert for event E
When the consumer handles the exception
Then the consumer does not terminate its processing loop
And the failure is logged
And the consumer continues to process subsequent events
```

### Scenario RM-S08 — Consumer starts with empty read model

```
Given no events have ever been published
When the Analytics BackgroundService starts
Then the consumer starts without error
And the read-model tables exist but contain zero rows
```

---

## Out of scope for this spec

- Backfill / replay of historical state that predates the first event.
- Strictly ordered event delivery (the outbox is FIFO per publisher but cross-publisher ordering is not guaranteed; the `last_event_at` guard in REQ-RM-03 is the mitigation for out-of-order delivery).
- Multi-consumer scaling / competing consumers.
- Dead-letter queue configuration.
- Processed-event deduplication table (optional optimization; the upsert + timestamp guard is the mandatory baseline).
