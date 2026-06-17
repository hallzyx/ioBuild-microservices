# Spec: domain-events

**Change**: analytics-event-driven
**Capability**: NEW `domain-events`
**Status**: draft (revised — Transactional Outbox delivery model)

---

## Context

`IoBuild.Shared` owns a marker interface `IEvent`. This capability extends Shared with typed event records and a **Transactional Outbox** delivery mechanism that guarantees at-least-once event delivery to RabbitMQ with zero event loss.

The original best-effort publish-after-commit design is replaced. In the new model:

1. The command handler writes state AND an outbox row **in a single DB transaction**.
2. A separate `OutboxWorker` (`BackgroundService`) polls pending outbox rows and publishes them to RabbitMQ.
3. The broker publish path is decoupled from the command path — a broker outage does NOT affect command execution.

Publishing services (`IoBuild.Devices`, `IoBuild.Projects`) depend on this abstraction.

---

## Requirements

### REQ-DE-01 — Event contract

All domain events MUST implement `IoBuild.Shared.IEvent`.

Each event MUST carry:
- `Guid Id` — stable unique identifier for the event occurrence; used by downstream consumers for de-duplication.
- `DateTime OccurredOn` — UTC timestamp set at event creation. (Note: code uses `OccurredOn`; spec aligned to implementation — batch 5 correction.)
- A typed payload with the resource state at the time of occurrence.

Events defined in this change:

| Event name | Publisher | Trigger |
|---|---|---|
| `DeviceCreated` | IoBuild.Devices | Device persisted for the first time |
| `DeviceUpdated` | IoBuild.Devices | Device fields updated and persisted |
| `DeviceDeleted` | IoBuild.Devices | Device removed and persisted |
| `ProjectCreated` | IoBuild.Projects | Project persisted for the first time |
| `ProjectUpdated` | IoBuild.Projects | Project fields updated and persisted |
| `UnitCreated` | IoBuild.Projects | Unit persisted for the first time |

### REQ-DE-02 — Transactional Outbox write

The domain event MUST be written as an outbox row **in the same database transaction** as the command's state change.

The outbox row MUST be committed atomically with the entity mutation. If the transaction commits successfully, exactly one outbox row for that event EXISTS. If the transaction rolls back for any reason, NO outbox row SHALL exist — and therefore no event will ever be published for that operation.

The command handler MUST NOT attempt to publish to RabbitMQ directly. The only I/O the command handler performs is the single DB transaction (state + outbox row).

### REQ-DE-03 — OutboxWorker background delivery

An `OutboxWorker` `BackgroundService` MUST be registered in each publishing service (`IoBuild.Devices`, `IoBuild.Projects`).

The worker MUST:
- Poll the outbox table at a configurable interval for rows with `Status = Pending`.
- For each pending row, publish the serialized event to RabbitMQ.
- On successful publish: mark the row `Status = Processed` (or equivalent terminal state).
- On publish failure (broker unreachable, timeout, or any AMQP error): leave the row in `Status = Pending` so it is retried in the next poll cycle.

The worker MUST process pending rows in ascending `CreatedAt` order (FIFO within a publisher).

The poll interval and batch size MUST be configurable without code changes (i.e., via configuration).

### REQ-DE-04 — At-least-once delivery guarantee

Because the `OutboxWorker` may retry a row that was published but not yet marked processed (e.g., the process crashed between publish and the status-update commit), the same outbox row MAY be published more than once.

This is the **at-least-once delivery** guarantee. Downstream consumers (see `analytics-read-model` spec) MUST be idempotent.

### REQ-DE-05 — RabbitMQ transport

The `OutboxWorker` MUST deliver events to a RabbitMQ exchange using AMQP. Exchange naming and routing conventions are a design-phase decision; this spec requires only that the exchange and routing key are configurable without code changes.

### REQ-DE-06 — Circuit breaker on the OutboxWorker publish path

The `OutboxWorker`'s broker publish call MUST be wrapped by a circuit breaker.

- While the circuit is open (consecutive publish failures exceed the configured threshold), the worker MUST skip the current poll cycle without throwing an unhandled exception, and MUST NOT change outbox row status (rows remain `Pending`).
- When the circuit transitions to Half-Open and a probe publish succeeds, normal processing resumes.
- A publish failure MUST be logged at `Warning` level; a circuit-open event MUST be logged at `Error` level.
- Failure threshold and break duration MUST be configurable without code changes.

### REQ-DE-07 — Broker outage does not affect commands

A RabbitMQ outage (broker unreachable, circuit open, any AMQP error) MUST NOT cause a create, update, or delete command to fail. The command's DB transaction (state + outbox row) commits independently of broker availability. Delivery is deferred to the next `OutboxWorker` poll cycle after recovery.

### REQ-DE-08 — No synchronous HTTP from publishing path

Neither the command handler nor the `OutboxWorker` MAY make HTTP calls to any other service. Their only external I/O targets are the local database and the RabbitMQ broker.

### REQ-DE-09 — Build and test integrity

After this capability is implemented:
- `dotnet build` MUST succeed with zero errors across the solution.
- `dotnet test` MUST pass (all previously passing tests continue to pass; new tests added by this capability must pass).
- At minimum, the following behaviors MUST have covering xUnit tests: REQ-DE-02 (rollback scenario), REQ-DE-03 (retry on broker failure), and REQ-DE-06 (circuit breaker, command still succeeds).

---

## Scenarios

### Scenario DE-S01 — Happy path: outbox row created and published after device creation

```
Given a valid CreateDevice command
And the RabbitMQ broker is reachable
When DeviceCommandService processes the command and the DB transaction commits
Then exactly one outbox row with Status = Pending is written to the outbox table
And the OutboxWorker polls and publishes the row to RabbitMQ
And the row is marked Status = Processed after successful publish
And the HTTP response to the caller is 201 Created
And the event Id is a non-empty Guid
And the event OccurredOn is a UTC timestamp within 1 second of the command
```

### Scenario DE-S02 — Rolled-back command produces no outbox row and no publish

```
Given a CreateDevice command
And the database throws an exception during the transaction (before or during commit)
When the transaction rolls back
Then zero outbox rows exist for that command
And zero events are published to RabbitMQ
And the HTTP response to the caller reflects the failure (4xx or 5xx as appropriate)
```

### Scenario DE-S03 — Broker down: outbox row stays pending and is retried after recovery

```
Given the RabbitMQ broker is unreachable
And a CreateDevice command completes and the DB transaction commits
When the OutboxWorker polls and attempts to publish the pending outbox row
Then the publish attempt fails
And the outbox row Status remains Pending
And a Warning log entry is recorded for the publish failure
When the RabbitMQ broker becomes reachable again
And the OutboxWorker polls in the next cycle
Then the same outbox row is published successfully
And the row is marked Status = Processed
And the HTTP response to the original caller was 201 Created at command time (unaffected by broker state)
```

### Scenario DE-S04 — Circuit breaker open: worker skips cycle, rows remain pending

```
Given consecutive publish failures have exceeded the circuit breaker failure threshold
And the circuit breaker is in the Open state
When the OutboxWorker runs a poll cycle
Then the worker does not attempt to publish any rows (broker I/O is short-circuited)
And all pending outbox rows remain Status = Pending
And an Error log entry is recorded indicating the circuit is open
```

### Scenario DE-S05 — Duplicate publish (at-least-once): consumer must handle

```
Given an outbox row for DeviceCreated (Id = E1, DeviceId = X) was published successfully
But the OutboxWorker crashed before marking the row Processed
When the OutboxWorker restarts and polls the same Pending row
Then the row is published to RabbitMQ a second time (duplicate delivery)
And the downstream consumer (analytics-read-model) handles it idempotently
```

### Scenario DE-S06 — Delete event outbox row created and published after device deletion

```
Given an existing Device with id X
And the RabbitMQ broker is reachable
When DeviceCommandService processes DeleteDevice for id X and the DB transaction commits
Then exactly one outbox row for DeviceDeleted is written with DeviceId = X
And the OutboxWorker publishes it
And the event payload contains DeviceId = X
```

### Scenario DE-S07 — Project and Unit events flow through outbox

```
Given a valid CreateProject command
And the RabbitMQ broker is reachable
When ProjectCommandService processes the command and the DB transaction commits
Then exactly one outbox row for ProjectCreated is written
And the OutboxWorker publishes it to RabbitMQ

Given a valid CreateUnit command within an existing project
And the RabbitMQ broker is reachable
When UnitCommandService processes the command and the DB transaction commits
Then exactly one outbox row for UnitCreated is written
And the OutboxWorker publishes it to RabbitMQ
```

---

## Out of scope for this spec

- Outbox table schema and EF Core mapping (design-phase decision).
- Dead-letter queue configuration.
- Event schema versioning.
- Backfill of historical state.
- Ordered delivery guarantees across publishers (each publisher is FIFO internally; cross-service ordering is not guaranteed).
- Outbox row retention / cleanup policy for processed rows.
