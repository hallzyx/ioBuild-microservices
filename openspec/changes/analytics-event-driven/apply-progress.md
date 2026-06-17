# Apply Progress: analytics-event-driven

**Batch**: 4 of 5 (updated — merging batches 1 + 2 + 3 + 4)
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
| B3-RED ConsumerIdempotencyTests | Build fails — DeviceProjection / AnalyticsEventConsumer missing | Projection entities + consumer + query rewrite created | n/a | DONE |
| B3-RED DeviceDeletedProjectionTests | Build fails | Same GREEN sweep | n/a | DONE |
| B3-RED EmptyReadModelQueryTests | Build fails — AnalyticsQueryService constructor changed | AnalyticsQueryService rewritten (no facade deps) | n/a | DONE |
| B3-RED NoHttpCallQueryTests | Build fails | Same GREEN sweep | n/a | DONE |
| B3-GREEN 6.1–6.10, 7.1–7.5 | Tests RED → GREEN | 9/9 IoBuild.Analytics.Tests pass | n/a | DONE |

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

### Batch 2 — Phase 5 (IoBuild.Projects outbox)
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

### Batch 3 — Phase 2 (TDD RED — Analytics tests) — NEW
- [x] 2.1 `tests/IoBuild.Analytics.Tests/Infrastructure/ConsumerIdempotencyTests.cs` — 2 tests: same event twice → one row; stale event → row unchanged (LWW guard)
- [x] 2.3 `tests/IoBuild.Analytics.Tests/Infrastructure/DeviceDeletedProjectionTests.cs` — 2 tests: delete removes row; delete absent row is no-op
- [x] 2.4 `tests/IoBuild.Analytics.Tests/Application/EmptyReadModelQueryTests.cs` — 3 tests: empty builder dashboard; empty owner dashboard; builder device count from projections
- [x] 2.5 `tests/IoBuild.Analytics.Tests/Application/NoHttpCallQueryTests.cs` — 2 tests: structural assertion that AnalyticsQueryService has no facade deps; constructable with only db+logger
- [x] `tests/IoBuild.Analytics.Tests/IoBuild.Analytics.Tests.csproj` created (xUnit + Moq + FluentAssertions + EF InMemory)
- [x] `microservices/IoBuild.sln` — `IoBuild.Analytics.Tests` added

### Batch 3 — Phase 6 (IoBuild.Analytics projections + consumer)
- [x] 6.1 `RabbitMQ.Client 7.0.0` added to `IoBuild.Analytics.csproj`
- [x] 6.2 `IoBuild.Analytics/Domain/Model/Projections/DeviceProjection.cs` — `DeviceId` PK, `OwnerUserId` (always 0 — see owner gap decision), `ProjectId?`, `UnitId?`, `DeviceType`, `Status`, `LastEventAt`
- [x] 6.3 `IoBuild.Analytics/Domain/Model/Projections/ProjectProjection.cs` — `ProjectId` PK, `BuilderUserId`, `Name`, `Status`, `LastEventAt`
- [x] 6.4 `IoBuild.Analytics/Domain/Model/Projections/UnitProjection.cs` — `UnitId` PK, `ProjectId`, `BuilderUserId`, `OwnerUserId?`, `Status`, `LastEventAt`
- [x] 6.5 `IoBuild.Analytics/AnalyticsDbContext.cs` rewritten: DbSets for 3 projection types; `builder_metrics`/`owner_metrics` DbSets + seed removed; EF config + indexes; keyless DTOs retained
- [x] 6.6 EF migration deferred — Analytics uses `EnsureCreated` (matches existing strategy). Projection tables auto-created; snapshot tables dropped because removed from `OnModelCreating`.
- [x] 6.7 `IoBuild.Analytics/Infrastructure/Messaging/AnalyticsEventConsumer.cs` — BackgroundService; dual constructor (production DI + internal test constructor); `ApplyEventAsync` public for testing; exchange/queue/binding declare on startup; LWW upsert; delete-if-exists; ack/nack semantics; `InternalsVisibleTo` on csproj
- [x] 6.8 `IoBuild.Analytics/Infrastructure/Messaging/AnalyticsConsumerExtensions.cs` — `AddAnalyticsEventConsumer` DI extension
- [x] 6.9 `IoBuild.Analytics/Program.cs` — removed `AddHttpClient<IDevicesContextFacade>` and `AddHttpClient<IProjectsContextFacade>`; added `AddAnalyticsEventConsumer`
- [x] 6.10 `dotnet test` → 9/9 IoBuild.Analytics.Tests PASS

### Batch 4 — Phase 8 (docker-compose RabbitMQ wiring) — NEW
- [x] 8.1 `rabbitmq` service added to `docker-compose.yml`: `rabbitmq:4-management`, container `iobuild-rabbitmq`, ports 5672+15672, env `RABBITMQ_DEFAULT_USER/PASS` with `${RABBITMQ_USER:-iobuild}` defaults, healthcheck `rabbitmq-diagnostics -q ping`, `restart: unless-stopped`, `networks: iobuild-network`
- [x] 8.2 `docker-compose.override.yml` — rabbitmq stanza added (ports 5672 + 15672 explicit for local dev, comment for management UI URL)
- [x] 8.3 `docker-compose.prod.yml` — rabbitmq service added; management port 15672 NOT published externally; credentials from `${RABBITMQ_USER}` / `${RABBITMQ_PASS}` with NO defaults (real secrets required in prod)
- [x] 8.4 `RabbitMq__ConnectionString=amqp://${RABBITMQ_USER:-iobuild}:${RABBITMQ_PASS:-iobuild}@rabbitmq:5672/` injected into `devices`, `projects`, `analytics` in `docker-compose.yml`; same (without defaults) in `docker-compose.prod.yml`; `depends_on: rabbitmq: { condition: service_healthy }` added to all three services in both files
- [x] Config key confirmed: C# reads `RabbitMq:ConnectionString` → env var `RabbitMq__ConnectionString`. Exact match in `DomainEventPublishingExtensions.cs:36` and `AnalyticsEventConsumer.cs:54`

### Batch 3 — Phase 7 (AnalyticsQueryService rewrite)
- [x] 7.1 `AnalyticsQueryService.Handle(GetBuilderDashboard)`: builder device count via join on project_projection; active projects, units, occupancy rate (divide-by-zero safe), DevicesByType from projections only
- [x] 7.2 `AnalyticsQueryService.Handle(GetOwnerDashboard)`: owner devices by `owner_user_id`; `MyUnitsCount` from unit_projection; `MyUnitsDetails` with project name join
- [x] 7.3 `AnalyticsQueryService.Handle(GetHistoricalData)`: returns empty list; comment `// Eventually consistent — telemetry out of scope`
- [x] 7.4 Constructor no longer accepts `IDevicesContextFacade` / `IProjectsContextFacade` — verified by `NoHttpCallQueryTests`
- [x] 7.5 `dotnet test` → all Analytics tests including `EmptyReadModelQueryTests` and `NoHttpCallQueryTests` PASS

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

**Decision**: Analytics (and all other services) uses `db.Database.EnsureCreated()` in `Program.cs` (not `Migrate()`). The projection tables are created automatically by EnsureCreated on first run because the EF config is in `OnModelCreating`. The snapshot tables (`builder_metrics`, `owner_metrics`) are **dropped** because they are no longer in `OnModelCreating` — EnsureCreated does not drop removed tables on a running database; they must be cleaned up manually in production (dev DBs only hold seed data).

### Device.OwnerUserId gap (RESOLVED — batch 3)

**Decision**: The `Device` aggregate has no `owner` concept — it carries only `ProjectId`, not `OwnerUserId`. `DeviceCreatedEvent.OwnerUserId` is always 0. `DeviceProjection.OwnerUserId` is always 0.

**Consequence**: Owner dashboard device counts = 0 (NOT a regression — the old HTTP path hit non-existent endpoints and already returned 0). Builder dashboard device counts ARE computable by joining `device_projection.project_id` → `project_projection.builder_user_id`.

**Decision**: Do NOT widen scope into Shared/Devices to add `OwnerUserId` to the Device aggregate. Keep `DeviceCreatedEvent.OwnerUserId` as-is (already committed in batch 1). Document as a known source-data limitation.

### ACL Facade removal (RESOLVED — batch 3)

**Decision**: `IDevicesContextFacade` and `IProjectsContextFacade` are **removed from `AnalyticsQueryService` constructor** and **not registered in `Program.cs`**. The facade classes (`DevicesContextFacade.cs`, `ProjectsContextFacade.cs`) and interfaces (`IDevicesContextFacade.cs`, `IProjectsContextFacade.cs`) are retained in their existing files as orphaned dead code for rollback reference. Verified by `NoHttpCallQueryTests` structural test.

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

### Batch 4

| File | Action |
|------|--------|
| `microservices/docker-compose.yml` | Modified — added `rabbitmq` service; added `RabbitMq__ConnectionString` + `depends_on: rabbitmq` to devices/projects/analytics |
| `microservices/docker-compose.override.yml` | Modified — added `rabbitmq` ports stanza for local dev |
| `microservices/docker-compose.prod.yml` | Modified — added `rabbitmq` service (management port not published); added `RabbitMq__ConnectionString` + `depends_on: rabbitmq` to devices/projects/analytics |
| `openspec/changes/analytics-event-driven/tasks.md` | Updated (tasks 8.1–8.4 marked `[x]`) |

### Batch 3

| File | Action |
|------|--------|
| `microservices/tests/IoBuild.Analytics.Tests/IoBuild.Analytics.Tests.csproj` | Created |
| `microservices/tests/IoBuild.Analytics.Tests/Infrastructure/ConsumerIdempotencyTests.cs` | Created (2 tests) |
| `microservices/tests/IoBuild.Analytics.Tests/Infrastructure/DeviceDeletedProjectionTests.cs` | Created (2 tests) |
| `microservices/tests/IoBuild.Analytics.Tests/Application/EmptyReadModelQueryTests.cs` | Created (3 tests) |
| `microservices/tests/IoBuild.Analytics.Tests/Application/NoHttpCallQueryTests.cs` | Created (2 tests) |
| `microservices/src/IoBuild.Analytics/IoBuild.Analytics.csproj` | Modified (added RabbitMQ.Client 7.0.0, InternalsVisibleTo Analytics.Tests) |
| `microservices/src/IoBuild.Analytics/Domain/Model/Projections/DeviceProjection.cs` | Created |
| `microservices/src/IoBuild.Analytics/Domain/Model/Projections/ProjectProjection.cs` | Created |
| `microservices/src/IoBuild.Analytics/Domain/Model/Projections/UnitProjection.cs` | Created |
| `microservices/src/IoBuild.Analytics/AnalyticsDbContext.cs` | Rewritten (projection DbSets; snapshot tables + seed removed) |
| `microservices/src/IoBuild.Analytics/Infrastructure/Messaging/AnalyticsEventConsumer.cs` | Created |
| `microservices/src/IoBuild.Analytics/Infrastructure/Messaging/AnalyticsConsumerExtensions.cs` | Created |
| `microservices/src/IoBuild.Analytics/Application/Internal/QueryServices/AnalyticsQueryService.cs` | Rewritten (no facade deps; reads from projections) |
| `microservices/src/IoBuild.Analytics/Program.cs` | Modified (removed facade HttpClient; added consumer registration) |
| `microservices/IoBuild.sln` | Modified (added IoBuild.Analytics.Tests) |
| `openspec/changes/analytics-event-driven/tasks.md` | Updated (tasks 2.x, 6.x, 7.x marked `[x]`) |

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

### Batch 3
```
dotnet build microservices/IoBuild.sln → SUCCESS (0 errors, pre-existing warnings only)
dotnet test  microservices/IoBuild.sln → ALL PASS
  IoBuild.Shared.Tests:       19/19
  IoBuild.Devices.Tests:      34/34
  IoBuild.IAM.Tests:           3/3
  IoBuild.Projects.Tests:      6/6
  IoBuild.Subscriptions.Tests: 8/8
  IoBuild.Analytics.Tests:     9/9  (+9 new: consumer idempotency, delete, empty model, no-HTTP)
  TOTAL: 79/79
```

### Batch 4
```
dotnet build microservices/IoBuild.sln --no-restore → SUCCESS (0 errors, 46 pre-existing warnings only — no new warnings)
dotnet test  microservices/IoBuild.sln --no-build   → ALL PASS
  IoBuild.Shared.Tests:       19/19
  IoBuild.Devices.Tests:      34/34
  IoBuild.IAM.Tests:           3/3
  IoBuild.Projects.Tests:      6/6
  IoBuild.Subscriptions.Tests: 8/8
  IoBuild.Analytics.Tests:     9/9
  TOTAL: 79/79  (no regressions — only compose YAML changed, zero C# touched)
```

---

## Deviations from Design

1. **`Device.OwnerUserId` not on aggregate**: `DeviceCreatedEvent.OwnerUserId` is set to `0` as a placeholder. The `Device` aggregate only carries `ProjectId`, not `OwnerUserId`. Analytics owner device counts = 0. Documented as a known source-data limitation. Explicitly approved by user before batch 3.

2. **EF migrations deferred (4.5 / 5.5 / 6.6)**: All services use `db.Database.EnsureCreated()`, NOT `Migrate()`. Projection tables created automatically on first run via EF's `OnModelCreating` config. No explicit migration was added — this MATCHES the existing schema-creation strategy for all services. Snapshot tables (`builder_metrics`, `owner_metrics`) no longer appear in `OnModelCreating` and will not exist in new deployments; must be manually dropped on existing DBs.

3. **`UnitCommandService` now depends on `IProjectRepository`**: Required to resolve `BuilderUserId` from the parent project for `UnitCreatedEvent`. DI is wired in Program.cs (already registered `IProjectRepository`).

4. **Circuit-breaker location**: As documented in batch 1 — breaker is in the worker, not the publisher.

5. **`AnalyticsEventConsumer` dual constructor pattern**: The consumer has an `internal` test constructor (db + logger only) and a production constructor (scopeFactory + config + logger). `InternalsVisibleTo(IoBuild.Analytics.Tests)` is set on the csproj so tests can use the internal constructor. This avoids touching the production code path for unit tests.

6. **ACL facade classes retained as dead code**: `DevicesContextFacade.cs`, `ProjectsContextFacade.cs`, `IDevicesContextFacade.cs`, `IProjectsContextFacade.cs` remain in place for rollback reference but are not registered in DI.

---

## Remaining Tasks (Batches 4–5)

### Batch 4 — docker-compose + RabbitMQ service
- [x] 8.1–8.4 RabbitMQ in compose files (all three compose variants; env vars + depends_on for Devices, Projects, Analytics)

### Batch 5 — Cleanup + final verification
- [ ] 9.1 Remove (or comment) ACL facade DI registration from Analytics `Program.cs` *(already done in batch 3)*
- [ ] 9.2 Snapshot seed classes confirmed removed from `AnalyticsDbContext` *(done in batch 3)*
- [ ] 9.3 XML doc `<remarks>` added to `AnalyticsQueryService` *(done in batch 3)*
- [ ] 9.4 Final build verification — MUST succeed with zero errors
- [ ] 9.5 Final test run — ALL tests MUST pass
