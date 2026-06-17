# Apply Progress: analytics-event-driven

**Batch**: 1 of 5
**Mode**: Strict TDD (RED → GREEN → REFACTOR)
**Delivery**: single PR with `size:exception` on branch `feat/analytics-event-driven`
**Date**: 2026-06-17

---

## TDD Cycle Evidence

| Task | RED | GREEN | REFACTOR | Status |
|------|-----|-------|----------|--------|
| 1.1 DomainEventTests | Build fails — types missing | 3.2/3.3 events created | n/a | DONE |
| 1.2 OutboxWriteInTransactionTests | Build fails — OutboxMessage missing | DeviceCommandService wired with IOutboxMessageRepository | Tested with RunOneCycleAsync | DONE |
| 1.3 OutboxWorkerPublishTests | Build fails — OutboxWorker missing | OutboxWorker created with RunOneCycleAsync | Refactored StartAsync timing → RunOneCycleAsync (deterministic) | DONE |
| 1.4 CircuitBreakerTests | Build fails — IDomainEventPublisher missing | DomainEventPublishingExtensions.BuildResiliencePipeline() created | n/a | DONE |
| 3.1–3.7 Implementation | (from RED test evidence above) | All 19 IoBuild.Shared.Tests pass | n/a | DONE |
| 4.1–4.10 (partial) | OutboxMessage/Repo/Worker | Devices OutboxMessage entity, IOutboxMessageRepository, OutboxMessageRepository, DevicesDbContext updated, DeviceCommandService modified, OutboxWorker created, Program.cs registered | n/a | DONE |

---

## Completed Tasks

### Phase 1 (TDD RED — tests)
- [x] 1.1 `tests/IoBuild.Shared.Tests/Domain/Model/Events/DomainEventTests.cs` — 16 test assertions on events
- [x] 1.2 `tests/IoBuild.Devices.Tests/Application/OutboxWriteInTransactionTests.cs` — 3 tests on transactional outbox write
- [x] 1.3 `tests/IoBuild.Devices.Tests/Workers/OutboxWorkerPublishTests.cs` — 3 tests on OutboxWorker behavior
- [x] 1.4 `tests/IoBuild.Shared.Tests/Infrastructure/CircuitBreakerTests.cs` — 3 tests on Polly pipeline

### Phase 3 (IoBuild.Shared implementation)
- [x] 3.1 `RabbitMQ.Client 7.0.0` and `Polly 8.5.2` added to `IoBuild.Shared.csproj`
- [x] 3.2 `IoBuild.Shared/Domain/Model/Events/DomainEvent.cs` — abstract record base
- [x] 3.3 Six concrete event records created:
  - `IoBuild.Shared/Domain/Model/Events/DeviceCreatedEvent.cs`
  - `IoBuild.Shared/Domain/Model/Events/DeviceUpdatedEvent.cs`
  - `IoBuild.Shared/Domain/Model/Events/DeviceDeletedEvent.cs`
  - `IoBuild.Shared/Domain/Model/Events/ProjectCreatedEvent.cs`
  - `IoBuild.Shared/Domain/Model/Events/ProjectUpdatedEvent.cs`
  - `IoBuild.Shared/Domain/Model/Events/UnitCreatedEvent.cs`
- [x] 3.4 `IoBuild.Shared/Domain/Services/IDomainEventPublisher.cs`
- [x] 3.5 `IoBuild.Shared/Infrastructure/Messaging/RabbitMqDomainEventPublisher.cs`
- [x] 3.6 `IoBuild.Shared/Infrastructure/Messaging/DomainEventPublishingExtensions.cs`
- [x] 3.7 `dotnet test` — 19/19 IoBuild.Shared.Tests PASS

### Phase 4 (IoBuild.Devices outbox — partial, required for tasks 1.2/1.3 to go GREEN)
- [x] 4.1 (via 3.1) `Polly 8.5.2` added to `IoBuild.Devices.csproj`
- [x] 4.2 `IoBuild.Devices/Domain/Model/Entities/OutboxMessage.cs` — includes `EventId` field
- [x] 4.3 `IoBuild.Devices/Domain/Repositories/IOutboxMessageRepository.cs` + `IoBuild.Devices/Infrastructure/Persistence/EFC/Repositories/OutboxMessageRepository.cs`
- [x] 4.4 `DevicesDbContext` updated with `DbSet<OutboxMessage>` + EF config + `(Status, CreatedAt)` index
- [x] 4.6 `DeviceCommandService.Handle(Create)` writes outbox row in same `SaveChangesAsync`
- [x] 4.7 `DeviceCommandService.Handle(Update)` writes outbox row
- [x] 4.8 `DeviceCommandService.Handle(Delete)` writes outbox row
- [x] 4.9 `IoBuild.Devices/Workers/OutboxWorker.cs` with `RunOneCycleAsync` (testable entry point)
- [x] 4.10 `IoBuild.Devices/Program.cs` — registered `IOutboxMessageRepository`, `AddDomainEventPublishing`, `AddHostedService<OutboxWorker>()`
- [x] 4.11 `dotnet test` — 34/34 IoBuild.Devices.Tests PASS

### Pre-existing bug fix (out-of-scope but required for clean build)
- [x] Fixed `tests/IoBuild.Subscriptions.Tests/OutboxPaymentTests.cs` — `StripePaymentService` constructor had a stale signature (missing 3 required parameters). Fixed both test instances.

---

## Architecture Decision: Circuit-Breaker Location (RESOLVED)

**Decision**: The Polly circuit-breaker pipeline lives in the **OutboxWorker**, NOT in the publisher.

**Rationale** (ADR-2 revised):
- The publisher (`RabbitMqDomainEventPublisher`) is a clean transport — it throws on failure and has no Polly dependency. This makes it trivially testable (just mock `IDomainEventPublisher`).
- The worker resolves the pipeline from DI (keyed singleton `OutboxResiliencePipelineKey`) and wraps its publish call in `pipeline.ExecuteAsync(...)`.
- The `AddDomainEventPublishing` extension registers BOTH the publisher singleton AND the keyed `ResiliencePipeline` — workers resolve both from DI.
- `BuildResiliencePipeline()` is `public static` so tests can verify the ADR-2 thresholds build correctly without a real AMQP connection.

**Consequence for the OutboxWorker**:
- `OutboxWorker` takes `ResiliencePipeline?` as an optional constructor arg (from keyed DI), falling back to `BuildResiliencePipeline()` when not injected (unit-test scenario).
- `RunOneCycleAsync` is `public virtual` for direct unit-test invocation without hosting machinery.

---

## Files Created / Modified

| File | Action |
|------|--------|
| `microservices/tests/IoBuild.Shared.Tests/IoBuild.Shared.Tests.csproj` | Created (new test project) |
| `microservices/tests/IoBuild.Shared.Tests/Domain/Model/Events/DomainEventTests.cs` | Created |
| `microservices/tests/IoBuild.Shared.Tests/Infrastructure/CircuitBreakerTests.cs` | Created |
| `microservices/tests/IoBuild.Devices.Tests/Application/OutboxWriteInTransactionTests.cs` | Created |
| `microservices/tests/IoBuild.Devices.Tests/Workers/OutboxWorkerPublishTests.cs` | Created |
| `microservices/src/IoBuild.Shared/IoBuild.Shared.csproj` | Modified (added RabbitMQ.Client 7.0.0, Polly 8.5.2) |
| `microservices/src/IoBuild.Shared/Domain/Model/Events/DomainEvent.cs` | Created |
| `microservices/src/IoBuild.Shared/Domain/Model/Events/DeviceCreatedEvent.cs` | Created |
| `microservices/src/IoBuild.Shared/Domain/Model/Events/DeviceUpdatedEvent.cs` | Created |
| `microservices/src/IoBuild.Shared/Domain/Model/Events/DeviceDeletedEvent.cs` | Created |
| `microservices/src/IoBuild.Shared/Domain/Model/Events/ProjectCreatedEvent.cs` | Created |
| `microservices/src/IoBuild.Shared/Domain/Model/Events/ProjectUpdatedEvent.cs` | Created |
| `microservices/src/IoBuild.Shared/Domain/Model/Events/UnitCreatedEvent.cs` | Created |
| `microservices/src/IoBuild.Shared/Domain/Services/IDomainEventPublisher.cs` | Created |
| `microservices/src/IoBuild.Shared/Infrastructure/Messaging/RabbitMqDomainEventPublisher.cs` | Created |
| `microservices/src/IoBuild.Shared/Infrastructure/Messaging/DomainEventPublishingExtensions.cs` | Created |
| `microservices/src/IoBuild.Devices/IoBuild.Devices.csproj` | Modified (added Polly 8.5.2) |
| `microservices/src/IoBuild.Devices/Domain/Model/Entities/OutboxMessage.cs` | Created |
| `microservices/src/IoBuild.Devices/Domain/Repositories/IOutboxMessageRepository.cs` | Created |
| `microservices/src/IoBuild.Devices/Infrastructure/Persistence/EFC/Repositories/OutboxMessageRepository.cs` | Created |
| `microservices/src/IoBuild.Devices/Infrastructure/Persistence/EFC/DbContext/DevicesDbContext.cs` | Modified (OutboxMessages DbSet + EF config) |
| `microservices/src/IoBuild.Devices/Application/Internal/CommandServices/DeviceCommandService.cs` | Modified (added IOutboxMessageRepository, outbox writes) |
| `microservices/src/IoBuild.Devices/Workers/OutboxWorker.cs` | Created |
| `microservices/src/IoBuild.Devices/Program.cs` | Modified (registered outbox repo + publisher + worker) |
| `microservices/IoBuild.sln` | Modified (added IoBuild.Shared.Tests project) |
| `microservices/tests/IoBuild.Subscriptions.Tests/OutboxPaymentTests.cs` | Modified (pre-existing bug fix) |

---

## Build & Test Results

```
dotnet build microservices/IoBuild.sln → SUCCESS (0 errors, warnings only — pre-existing MQTTnet NU1603)
dotnet test microservices/IoBuild.sln  → ALL PASS
  IoBuild.Shared.Tests:      19/19 PASS
  IoBuild.Devices.Tests:     34/34 PASS  (28 pre-existing + 6 new)
  IoBuild.IAM.Tests:          3/3  PASS
  IoBuild.Projects.Tests:     3/3  PASS
  IoBuild.Subscriptions.Tests: 8/8  PASS
  TOTAL: 67/67
```

---

## Deviations from Design

1. **`Device.OwnerUserId` not on aggregate**: `DeviceCreatedEvent.OwnerUserId` is set to `0` as a placeholder. The `Device` aggregate only carries `ProjectId`, not `OwnerUserId`. This will need to be resolved in Batch 2 when wiring the actual aggregate — either by adding `OwnerUserId` to `Device` or deriving it from context. Marked with `// TODO (batch 2)` in code.

2. **EF migration for `outbox_messages` deferred**: Task 4.5 (generate EF migration) was not executed in this batch. `DevicesDbContext.EnsureCreated()` in `Program.cs` will create the table in dev on first run. Batch 2 should add the explicit migration if the service uses `database.Migrate()`. The EF config is already in place.

3. **Circuit-breaker location**: As documented above, the breaker is in the worker (not the publisher). This matches the REVISED ADR-2 design. The initial task description (task 1.4) said the breaker would be tested on `RabbitMqDomainEventPublisher.PublishAsync` — but the revised design places it in the worker. The test was written to match the actual design intent, testing the Polly pipeline in isolation and verifying the publisher's throw-on-failure contract.

---

## Remaining Tasks (Batches 2–5)

### Batch 2 — IoBuild.Devices (outbox migration) + IoBuild.Projects outbox
- [ ] 4.5 EF migration `AddOutboxMessages` for Devices
- [ ] 5.1–5.10 Full IoBuild.Projects outbox plumbing (same pattern as Devices)
- [ ] Map `OwnerUserId` on Device aggregate / command (or derive from context)

### Batch 3 — IoBuild.Analytics consumer + projection tables
- [ ] 2.1–2.5 Phase 2 RED tests (Analytics consumer + query)
- [ ] 6.1–6.10 Analytics consumer + projection tables + AnalyticsEventConsumer

### Batch 4 — AnalyticsQueryService rewrite
- [ ] 7.1–7.5 AnalyticsQueryService rewrite (read from projections, remove ACL)

### Batch 5 — docker-compose + cleanup
- [ ] 8.1–8.4 RabbitMQ in compose files
- [ ] 9.1–9.5 Cleanup, refactor, final build + test verification
