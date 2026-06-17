# Apply Progress: analytics-event-driven

**Batch**: 2 of 5 (updated — merging batch 1 + batch 2)
**Mode**: Strict TDD (RED → GREEN → REFACTOR)
**Delivery**: single PR with `size:exception` on branch `feat/analytics-event-driven`
**Date**: 2026-06-17

---

## TDD Cycle Evidence

| Task | RED | GREEN | REFACTOR | Status |
|------|-----|-------|----------|--------|
| 1.1 DomainEventTests | Build fails — types missing | 3.2/3.3 events created | n/a | DONE |
| 1.2 OutboxWriteInTransactionTests (Devices) | Build fails — OutboxMessage missing | DeviceCommandService wired with IOutboxMessageRepository | Tested with RunOneCycleAsync | DONE |
| 1.3 OutboxWorkerPublishTests | Build fails — OutboxWorker missing | OutboxWorker created with RunOneCycleAsync | Refactored StartAsync timing → RunOneCycleAsync (deterministic) | DONE |
| 1.4 CircuitBreakerTests | Build fails — IDomainEventPublisher missing | DomainEventPublishingExtensions.BuildResiliencePipeline() created | n/a | DONE |
| 3.1–3.7 Implementation | (from RED test evidence above) | All 19 IoBuild.Shared.Tests pass | n/a | DONE |
| 4.1–4.10 (partial) | OutboxMessage/Repo/Worker | Devices OutboxMessage entity, IOutboxMessageRepository, OutboxMessageRepository, DevicesDbContext updated, DeviceCommandService modified, OutboxWorker created, Program.cs registered | n/a | DONE |
| B2-RED OutboxWriteInTransactionTests (Projects) | Build fails — IOutboxMessageRepository missing on Projects | ProjectCommandService + UnitCommandService wired with IOutboxMessageRepository | n/a | DONE |
| B2-GREEN 5.1–5.10 | Tests RED → GREEN after all Projects outbox files created | 70/70 pass | n/a | DONE |

---

## Completed Tasks

### Batch 1 — Phase 1 (TDD RED — tests)
- [x] 1.1 `tests/IoBuild.Shared.Tests/Domain/Model/Events/DomainEventTests.cs` — 16 test assertions on events
- [x] 1.2 `tests/IoBuild.Devices.Tests/Application/OutboxWriteInTransactionTests.cs` — 3 tests on transactional outbox write
- [x] 1.3 `tests/IoBuild.Devices.Tests/Workers/OutboxWorkerPublishTests.cs` — 3 tests on OutboxWorker behavior
- [x] 1.4 `tests/IoBuild.Shared.Tests/Infrastructure/CircuitBreakerTests.cs` — 3 tests on Polly pipeline

### Batch 1 — Phase 3 (IoBuild.Shared implementation)
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

### Batch 1 — Phase 4 (IoBuild.Devices outbox)
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

### Batch 1 — Pre-existing bug fix
- [x] Fixed `tests/IoBuild.Subscriptions.Tests/OutboxPaymentTests.cs` — `StripePaymentService` constructor stale signature fixed.

### Batch 2 — Phase 5 (IoBuild.Projects outbox) — NEW
- [x] 5-RED `tests/IoBuild.Projects.Tests/Application/OutboxWriteInTransactionTests.cs` — 3 tests (CreateProject, UpdateProject, CreateUnit all write exactly 1 outbox row before CompleteAsync)
- [x] 5.1 `Polly 8.5.2` + `RabbitMQ.Client 7.0.0` added to `IoBuild.Projects.csproj`
- [x] 5.2 `IoBuild.Projects/Domain/Model/Entities/OutboxMessage.cs` — mirrors Devices entity
- [x] 5.3 `IoBuild.Projects/Domain/Repositories/IOutboxMessageRepository.cs` + `IoBuild.Projects/Infrastructure/Repositories/OutboxMessageRepository.cs`
- [x] 5.4 `AppDbContext` updated: `DbSet<OutboxMessage>` + EF config block + `(Status, CreatedAt)` index
- [x] 5.6 `ProjectCommandService.Handle(Create)` → `ProjectCreatedEvent` → outbox row in same `CompleteAsync()`
- [x] 5.7 `ProjectCommandService.Handle(Update)` → `ProjectUpdatedEvent` → outbox row in same `CompleteAsync()`
- [x] 5.8 `UnitCommandService.Handle(Create)` → `UnitCreatedEvent` → outbox row in same `CompleteAsync()`
- [x] 5.9 `IoBuild.Projects/Workers/OutboxWorker.cs` — mirrors Devices worker; EventTypeMap: ProjectCreated/Updated + UnitCreated
- [x] 5.10 `IoBuild.Projects/Program.cs` — registered `IOutboxMessageRepository`, `AddDomainEventPublishing`, `AddHostedService<OutboxWorker>()`

---

## Architecture Decisions

### Circuit-Breaker Location (RESOLVED — batch 1)

**Decision**: The Polly circuit-breaker pipeline lives in the **OutboxWorker**, NOT in the publisher.

**Rationale**: Publisher is clean transport (throws on failure, no Polly dep). Worker resolves keyed pipeline from DI and wraps publish call. `AddDomainEventPublishing` registers both publisher singleton AND keyed `ResiliencePipeline`.

### BuilderUserId on UnitCreatedEvent (RESOLVED — batch 2)

**Decision**: `UnitCommandService.Handle(CreateUnit)` resolves `BuilderUserId` by loading the parent `Project` via `IProjectRepository.FindByIdAsync(command.ProjectId)` and reading `Project.BuilderId`.

**Rationale**: The `Unit` aggregate carries no `BuilderId`. Rather than leaving it as 0, we do a synchronous look-up of the parent project in the same transaction scope (before `CompleteAsync()`). This keeps the event accurate at publish time. If the parent project is not found (edge case), `BuilderUserId` defaults to 0 — Analytics can resolve it later via `project_projection.builder_user_id`.

**Consequence**: `UnitCommandService` now takes `IProjectRepository` as a constructor dependency. DI registration updated in `Program.cs`.

### Schema creation strategy: EnsureCreated (NOT migrations)

**Decision**: Projects uses `db.Database.EnsureCreated()` in `Program.cs` (not `Migrate()`). Tasks 5.5 and 4.5 (EF migrations) were deferred. The `outbox_messages` table is created automatically by EnsureCreated on first run because the EF config is in `OnModelCreating`. No manual migration needed to match the existing schema strategy.

---

## Files Created / Modified

### Batch 1

| File | Action |
|------|--------|
| `microservices/tests/IoBuild.Shared.Tests/IoBuild.Shared.Tests.csproj` | Created |
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
| `microservices/src/IoBuild.Devices/Infrastructure/Persistence/EFC/DbContext/DevicesDbContext.cs` | Modified |
| `microservices/src/IoBuild.Devices/Application/Internal/CommandServices/DeviceCommandService.cs` | Modified |
| `microservices/src/IoBuild.Devices/Workers/OutboxWorker.cs` | Created |
| `microservices/src/IoBuild.Devices/Program.cs` | Modified |
| `microservices/IoBuild.sln` | Modified (added IoBuild.Shared.Tests) |
| `microservices/tests/IoBuild.Subscriptions.Tests/OutboxPaymentTests.cs` | Modified (bug fix) |

### Batch 2

| File | Action |
|------|--------|
| `microservices/tests/IoBuild.Projects.Tests/Application/OutboxWriteInTransactionTests.cs` | Created (3 TDD tests) |
| `microservices/src/IoBuild.Projects/IoBuild.Projects.csproj` | Modified (added Polly 8.5.2, RabbitMQ.Client 7.0.0) |
| `microservices/src/IoBuild.Projects/Domain/Model/Entities/OutboxMessage.cs` | Created |
| `microservices/src/IoBuild.Projects/Domain/Repositories/IOutboxMessageRepository.cs` | Created |
| `microservices/src/IoBuild.Projects/Infrastructure/Repositories/OutboxMessageRepository.cs` | Created |
| `microservices/src/IoBuild.Projects/Infrastructure/Persistence/AppDbContext.cs` | Modified (OutboxMessages DbSet + EF config) |
| `microservices/src/IoBuild.Projects/Application/Services/ProjectCommandService.cs` | Modified (outbox writes for Create + Update) |
| `microservices/src/IoBuild.Projects/Application/Services/UnitCommandService.cs` | Modified (outbox write + IProjectRepository dep) |
| `microservices/src/IoBuild.Projects/Workers/OutboxWorker.cs` | Created |
| `microservices/src/IoBuild.Projects/Program.cs` | Modified (outbox repo + publisher + worker registration) |
| `openspec/changes/analytics-event-driven/tasks.md` | Updated (tasks 4.x and 5.x marked `[x]`) |

---

## Build & Test Results

### Batch 1
```
dotnet build → SUCCESS (0 errors)
dotnet test  → 67/67 PASS
  IoBuild.Shared.Tests:       19/19
  IoBuild.Devices.Tests:      34/34
  IoBuild.IAM.Tests:           3/3
  IoBuild.Projects.Tests:      3/3
  IoBuild.Subscriptions.Tests: 8/8
```

### Batch 2
```
dotnet build microservices/IoBuild.sln → SUCCESS (0 errors, pre-existing warnings only)
dotnet test  microservices/IoBuild.sln → ALL PASS
  IoBuild.Shared.Tests:       19/19
  IoBuild.Devices.Tests:      34/34
  IoBuild.IAM.Tests:           3/3
  IoBuild.Projects.Tests:      6/6  (+3 new outbox tests)
  IoBuild.Subscriptions.Tests: 8/8
  TOTAL: 70/70
```

---

## Deviations from Design

1. **`Device.OwnerUserId` not on aggregate**: `DeviceCreatedEvent.OwnerUserId` is set to `0` as a placeholder. The `Device` aggregate only carries `ProjectId`, not `OwnerUserId`. Analytics can resolve it via ProjectId if needed.

2. **EF migrations deferred (4.5 / 5.5)**: Both Devices and Projects use `db.Database.EnsureCreated()`, NOT `Migrate()`. The `outbox_messages` table is created automatically on first run via EF's `OnModelCreating` config. No explicit migration was added — this MATCHES the existing schema-creation strategy for both services.

3. **`UnitCommandService` now depends on `IProjectRepository`**: Required to resolve `BuilderUserId` from the parent project for `UnitCreatedEvent`. DI is wired in Program.cs (already registered `IProjectRepository`).

4. **Circuit-breaker location**: As documented in batch 1 — breaker is in the worker, not the publisher.

---

## Remaining Tasks (Batches 3–5)

### Batch 3 — IoBuild.Analytics consumer + projection tables
- [ ] 2.1–2.5 Phase 2 RED tests (Analytics consumer + query)
- [ ] 6.1–6.10 Analytics consumer + projection tables + AnalyticsEventConsumer

### Batch 4 — AnalyticsQueryService rewrite
- [ ] 7.1–7.5 AnalyticsQueryService rewrite (read from projections, remove ACL)

### Batch 5 — docker-compose + cleanup
- [ ] 8.1–8.4 RabbitMQ in compose files
- [ ] 9.1–9.5 Cleanup, refactor, final build + test verification
