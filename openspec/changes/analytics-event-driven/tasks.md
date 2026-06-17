# Tasks: Event-Driven Analytics Read Model

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | 1 200 – 1 600 |
| 400-line budget risk | High |
| Chained PRs recommended | Yes |
| Suggested split | PR 1 → Shared + Outbox infra · PR 2 → Devices + Projects producers · PR 3 → Analytics consumer + query rewrite · PR 4 → docker-compose + cleanup |
| Delivery strategy | ask-on-risk |
| Chain strategy | pending |

Decision needed before apply: Yes
Chained PRs recommended: Yes
Chain strategy: pending
400-line budget risk: High

### Suggested Work Units

| Unit | Goal | Likely PR | Notes |
|------|------|-----------|-------|
| 1 | Shared event contracts + RabbitMQ publisher | PR 1 | Foundation; all other units depend on it |
| 2 | Devices + Projects outbox plumbing | PR 2 | Mirrors Subscriptions; depends on PR 1 |
| 3 | Analytics consumer + projection tables + query rewrite | PR 3 | Depends on PR 1 (event types); can start from PR 2 branch |
| 4 | docker-compose + cleanup (remove ACL facades) | PR 4 | Integration wiring; depends on PR 2 + PR 3 |

---

## Phase 1 — Tests: Shared event contracts (RED first) [REQ-DE-01, REQ-DE-09]

> TDD: write failing tests before any implementation.

- [x] 1.1 **[RED]** Write xUnit test `DomainEventTests` in a new test project or `IoBuild.Shared.Tests`: assert `DeviceCreatedEvent`, `ProjectCreatedEvent`, `UnitCreatedEvent` implement `IEvent`, have non-empty `EventId`, and `OccurredOn` is UTC. → `tests/IoBuild.Shared.Tests/Domain/Model/Events/DomainEventTests.cs` (REQ-DE-01)
- [x] 1.2 **[RED]** Write xUnit test `OutboxWriteInTransactionTests`: mock `DevicesDbContext`, assert `Handle(CreateDevice)` adds exactly one `OutboxMessage` row AND one `Device` row before `SaveChangesAsync`. → `tests/IoBuild.Devices.Tests/Application/OutboxWriteInTransactionTests.cs` (REQ-DE-02, DE-S01)
- [x] 1.3 **[RED]** Write xUnit test `OutboxWorkerPublishTests`: stub `IDomainEventPublisher`, assert worker marks row `Processed` on success and increments `RetryCount` on failure without throwing. → `tests/IoBuild.Devices.Tests/Workers/OutboxWorkerPublishTests.cs` (REQ-DE-03, DE-S03)
- [x] 1.4 **[RED]** Write xUnit test `CircuitBreakerTests`: configure Polly pipeline with low threshold, assert that after N failures the breaker opens and `RabbitMqDomainEventPublisher.PublishAsync` throws `BrokenCircuitException`, and pending rows stay `Pending`. → `tests/IoBuild.Shared.Tests/Infrastructure/CircuitBreakerTests.cs` (REQ-DE-06, DE-S04)

## Phase 2 — Tests: Analytics consumer + query (RED first) [REQ-RM-03, REQ-AQ-01, REQ-AQ-03]

- [ ] 2.1 **[RED]** Write `ConsumerIdempotencyTests`: deliver same `DeviceCreatedEvent` twice via `AnalyticsEventConsumer`; assert exactly one `DeviceProjection` row exists. → `tests/IoBuild.Analytics.Tests/Infrastructure/ConsumerIdempotencyTests.cs` (REQ-RM-03, RM-S02)
- [ ] 2.2 **[RED]** Write `StaleEventDiscardTests`: set `last_event_at = T2`; deliver event with `OccurredAt = T1 < T2`; assert row unchanged. → `tests/IoBuild.Analytics.Tests/Infrastructure/StaleEventDiscardTests.cs` (REQ-RM-03, RM-S02b)
- [ ] 2.3 **[RED]** Write `DeviceDeletedProjectionTests`: upsert a `DeviceProjection` row, then deliver `DeviceDeleted`; assert row is absent from query results. → `tests/IoBuild.Analytics.Tests/Infrastructure/DeviceDeletedProjectionTests.cs` (REQ-RM-04, AQ-S05)
- [ ] 2.4 **[RED]** Write `EmptyReadModelQueryTests`: call `AnalyticsQueryService` with empty tables; assert 200-equivalent result with all counts = 0 and no exception. → `tests/IoBuild.Analytics.Tests/Application/EmptyReadModelQueryTests.cs` (REQ-AQ-03, AQ-S02)
- [ ] 2.5 **[RED]** Write `NoHttpCallQueryTests`: inject mock `IDevicesContextFacade`; call `AnalyticsQueryService`; assert zero calls to the facade. → `tests/IoBuild.Analytics.Tests/Application/NoHttpCallQueryTests.cs` (REQ-AQ-01, AQ-S03)

## Phase 3 — Implementation: IoBuild.Shared [REQ-DE-01, REQ-DE-05, REQ-DE-06]

- [x] 3.1 Add `RabbitMQ.Client 7.0.0` and `Polly 8.5.x` to `microservices/src/IoBuild.Shared/IoBuild.Shared.csproj`. (ADR-1, ADR-10)
- [x] 3.2 Create `DomainEvent` abstract record implementing `IEvent` with `EventId`, `OccurredOn`, and abstract `RoutingKey`. → `IoBuild.Shared/Domain/Model/Events/DomainEvent.cs` (REQ-DE-01)
- [x] 3.3 Create the 6 concrete event records: `DeviceCreatedEvent`, `DeviceUpdatedEvent`, `DeviceDeletedEvent`, `ProjectCreatedEvent`, `ProjectUpdatedEvent`, `UnitCreatedEvent` with fields per ADR-4. → `IoBuild.Shared/Domain/Model/Events/` (REQ-DE-01, DE-S01, DE-S06, DE-S07)
- [x] 3.4 Create `IDomainEventPublisher` interface with `PublishAsync(DomainEvent, CancellationToken)`. → `IoBuild.Shared/Domain/Services/IDomainEventPublisher.cs` (ADR-8)
- [x] 3.5 Create `RabbitMqDomainEventPublisher : IDomainEventPublisher`: singleton connection + per-publish channel, declares exchange `iobuild.domain.events` (topic, durable), publisher confirms, sets `event-type` header and routing key from `DomainEvent.RoutingKey`. → `IoBuild.Shared/Infrastructure/Messaging/RabbitMqDomainEventPublisher.cs` (REQ-DE-05, REQ-DE-06)
- [x] 3.6 Create `AddDomainEventPublishing(IConfiguration)` DI extension registering the Polly pipeline (keyed) and `IDomainEventPublisher → RabbitMqDomainEventPublisher` (singleton), reading `RabbitMq:ConnectionString`. → `IoBuild.Shared/Infrastructure/Messaging/DomainEventPublishingExtensions.cs` (ADR-8)
- [x] 3.7 **[GREEN]** Run `dotnet test` — Phase 1 tests 1.1 and 1.4 MUST pass now. (19/19 PASS)

## Phase 4 — Implementation: IoBuild.Devices outbox [REQ-DE-02, REQ-DE-03, ADR-8b]

- [ ] 4.1 Add `RabbitMQ.Client 7.0.0` to `IoBuild.Devices.csproj`. (ADR-10)
- [ ] 4.2 Copy `OutboxMessage` entity from Subscriptions + add `EventId` (Guid) field. → `IoBuild.Devices/Domain/Model/Entities/OutboxMessage.cs` (ADR-8b)
- [ ] 4.3 Copy `IOutboxMessageRepository` and `OutboxMessageRepository` from Subscriptions, change DbContext type to `DevicesDbContext`. → `IoBuild.Devices/Domain/Repositories/IOutboxMessageRepository.cs` + `IoBuild.Devices/Infrastructure/Persistence/EFC/Repositories/OutboxMessageRepository.cs` (ADR-8b)
- [ ] 4.4 Add `DbSet<OutboxMessage> OutboxMessages` to `DevicesDbContext` + EF config block (ADR-8b columns + `(Status, CreatedAt)` index). → `IoBuild.Devices/Infrastructure/Persistence/EFC/DevicesDbContext.cs`
- [ ] 4.5 Generate EF migration `AddOutboxMessages` for `iobuild_devices`. → `IoBuild.Devices/Infrastructure/Persistence/EFC/Migrations/`
- [ ] 4.6 Modify `DeviceCommandService.Handle(Create)`: build `DeviceCreatedEvent`, serialize to JSON, `outboxRepo.AddAsync(new OutboxMessage(...){ EventId })`, keep single `SaveChangesAsync()`. (REQ-DE-02, DE-S01)
- [ ] 4.7 Modify `DeviceCommandService.Handle(Update)`: same pattern → `DeviceUpdatedEvent`. (REQ-DE-02)
- [ ] 4.8 Modify `DeviceCommandService.Handle(Delete)`: same pattern → `DeviceDeletedEvent`. (REQ-DE-02, DE-S06)
- [ ] 4.9 Create `OutboxWorker : BackgroundService` mirroring `Subscriptions/Workers/OutboxWorker.cs` + adds `IDomainEventPublisher` publish call + Polly wrapping; `RetryCount++` on failure. → `IoBuild.Devices/Workers/OutboxWorker.cs` (REQ-DE-03, REQ-DE-06)
- [ ] 4.10 Register in `IoBuild.Devices/Program.cs`: `AddScoped<IOutboxMessageRepository, OutboxMessageRepository>()`, `AddDomainEventPublishing(...)`, `AddHostedService<OutboxWorker>()`. (ADR-8)
- [ ] 4.11 **[GREEN]** Run `dotnet test` — Phase 1 tests 1.2 and 1.3 MUST pass now.

## Phase 5 — Implementation: IoBuild.Projects outbox [REQ-DE-02, REQ-DE-03, DE-S07]

- [ ] 5.1 Add `RabbitMQ.Client 7.0.0` to `IoBuild.Projects.csproj`. (ADR-10)
- [ ] 5.2 Copy `OutboxMessage` entity (same as Devices + `EventId`). → `IoBuild.Projects/Domain/Model/Entities/OutboxMessage.cs`
- [ ] 5.3 Copy `IOutboxMessageRepository` + `OutboxMessageRepository` changing DbContext type. → `IoBuild.Projects/Domain/Repositories/` + `IoBuild.Projects/Infrastructure/Persistence/EFC/Repositories/`
- [ ] 5.4 Add `DbSet<OutboxMessage>` + EF config to `ProjectsDbContext`.
- [ ] 5.5 Generate EF migration `AddOutboxMessages` for `iobuild_projects`. → `IoBuild.Projects/Infrastructure/Persistence/EFC/Migrations/`
- [ ] 5.6 Modify `ProjectCommandService.Handle(Create)`: `ProjectCreatedEvent` → outbox row in same `IUnitOfWork.CompleteAsync()`. (REQ-DE-02, DE-S07)
- [ ] 5.7 Modify `ProjectCommandService.Handle(Update)`: `ProjectUpdatedEvent` → outbox row. (REQ-DE-02)
- [ ] 5.8 Modify `UnitCommandService.Handle(Create)`: `UnitCreatedEvent` → outbox row in same `CompleteAsync()`. (REQ-DE-02, DE-S07)
- [ ] 5.9 Create `OutboxWorker : BackgroundService` mirroring Devices worker. → `IoBuild.Projects/Workers/OutboxWorker.cs` (REQ-DE-03)
- [ ] 5.10 Register in `IoBuild.Projects/Program.cs`: outbox repo, publisher extension, `AddHostedService<OutboxWorker>()`. (ADR-8)

## Phase 6 — Implementation: IoBuild.Analytics projections + consumer [REQ-RM-01, REQ-RM-02, REQ-RM-03]

- [ ] 6.1 Add `RabbitMQ.Client 7.0.0` to `IoBuild.Analytics.csproj`. (ADR-1)
- [ ] 6.2 Create EF entity `DeviceProjection` (columns per ADR-6: `DeviceId` PK, `OwnerUserId`, `ProjectId?`, `UnitId?`, `DeviceType`, `Status`, `LastEventAt`). → `IoBuild.Analytics/Domain/Model/Projections/DeviceProjection.cs`
- [ ] 6.3 Create EF entity `ProjectProjection` (columns: `ProjectId` PK, `BuilderUserId`, `Name`, `Status`, `LastEventAt`). → `IoBuild.Analytics/Domain/Model/Projections/ProjectProjection.cs`
- [ ] 6.4 Create EF entity `UnitProjection` (columns: `UnitId` PK, `ProjectId`, `BuilderUserId`, `OwnerUserId?`, `Status`, `LastEventAt`). → `IoBuild.Analytics/Domain/Model/Projections/UnitProjection.cs`
- [ ] 6.5 Add `DbSet` for all three projections to `AnalyticsDbContext` + EF config (snake_case, indexes on `OwnerUserId`/`BuilderUserId`). → `IoBuild.Analytics/Infrastructure/Persistence/EFC/AnalyticsDbContext.cs`
- [ ] 6.6 Generate EF migration `AddProjectionTables` that adds `device_projection`, `project_projection`, `unit_projection` AND drops `builder_metrics`, `owner_metrics` tables (and removes their seed). → `IoBuild.Analytics/Infrastructure/Persistence/EFC/Migrations/`
- [ ] 6.7 Create `AnalyticsEventConsumer : BackgroundService`: on startup declare queue `analytics.read-model` (durable) + bindings `device.#`, `project.#` to exchange `iobuild.domain.events`; consume loop: read `event-type` header, deserialize to correct record, apply idempotent upsert with `last_event_at` LWW guard, `BasicAck` on success, `BasicNack(requeue:true)` on transient DB error, `BasicNack(requeue:false)` on poison/unknown type. → `IoBuild.Analytics/Infrastructure/Messaging/AnalyticsEventConsumer.cs` (REQ-RM-02, REQ-RM-03, REQ-RM-04, REQ-RM-05, RM-S01..S08)
- [ ] 6.8 Create `AddAnalyticsEventConsumer(IConfiguration)` DI extension registering `AnalyticsEventConsumer` as `IHostedService`. → `IoBuild.Analytics/Infrastructure/Messaging/AnalyticsConsumerExtensions.cs`
- [ ] 6.9 Register in `IoBuild.Analytics/Program.cs`: call `AddAnalyticsEventConsumer(...)`. Remove ACL HTTP client registrations for query services (`IDevicesContextFacade`, `IProjectsContextFacade`). (REQ-AQ-01)
- [ ] 6.10 **[GREEN]** Run `dotnet test` — Phase 2 tests 2.1, 2.2, 2.3 MUST pass now.

## Phase 7 — Implementation: AnalyticsQueryService rewrite [REQ-AQ-01, REQ-AQ-02, REQ-AQ-03]

- [ ] 7.1 Rewrite `AnalyticsQueryService.Handle(GetBuilderDashboard)`: read only from `DeviceProjection`, `ProjectProjection`, `UnitProjection` by `BuilderUserId`; compute all `BuilderMetrics` fields per ADR-6; return zeroed struct when tables empty. → `IoBuild.Analytics/Application/Services/AnalyticsQueryService.cs` (REQ-AQ-02, REQ-AQ-03, AQ-S01, AQ-S02)
- [ ] 7.2 Rewrite `AnalyticsQueryService.Handle(GetOwnerDashboard)`: read from projections by `OwnerUserId`; compute `OwnerMetrics` fields; zeroed on empty. (REQ-AQ-02, REQ-AQ-03, AQ-S02)
- [ ] 7.3 Rewrite `AnalyticsQueryService.Handle(GetHistoricalData)`: return empty/zeroed result (telemetry out of scope; remove ACL call). Add `// Eventually consistent — telemetry out of scope` comment. (REQ-AQ-04)
- [ ] 7.4 Remove or guard `DevicesContextFacade` / `ProjectsContextFacade` constructor injection from `AnalyticsQueryService` — verify zero transitive HTTP calls. (REQ-AQ-01, AQ-S03)
- [ ] 7.5 **[GREEN]** Run `dotnet test` — Phase 2 tests 2.4 and 2.5 MUST pass now.

## Phase 8 — Infrastructure: docker-compose [ADR-9]

- [ ] 8.1 Add `rabbitmq` service (`rabbitmq:4-management`, ports 5672/15672, env credentials, healthcheck, `iobuild-net`) to `docker-compose.yml`. (ADR-9)
- [ ] 8.2 Add the same `rabbitmq` service to `docker-compose.override.yml` (dev override) if present.
- [ ] 8.3 Add to `docker-compose.prod.yml`: same service, management port omitted externally.
- [ ] 8.4 Add `RabbitMq__ConnectionString` env var and `depends_on: rabbitmq: { condition: service_healthy }` to Devices, Projects, and Analytics service definitions in all compose files. (ADR-9, REQ-DE-07)

## Phase 9 — Cleanup + REFACTOR [REQ-AQ-01, REQ-DE-09, REQ-AQ-06]

- [ ] 9.1 Remove (or comment out with `// ACL — orphaned, kept for rollback reference`) `DevicesContextFacade` and `ProjectsContextFacade` HTTP client registrations from `IoBuild.Analytics/Program.cs`. (REQ-AQ-01)
- [ ] 9.2 Remove `builder_metrics`/`owner_metrics` EF seed classes and their `OnModelCreating` mapping from `AnalyticsDbContext`. (ADR-6)
- [ ] 9.3 Add XML doc comment `/// <remarks>Metrics are eventually consistent with source services (Transactional Outbox, ~5 s lag).</remarks>` to `AnalyticsQueryService` class. (REQ-AQ-04)
- [ ] 9.4 **[REFACTOR]** Run `dotnet build microservices/IoBuild.sln` — MUST succeed with zero errors. Fix any remaining build issues. (REQ-DE-09, REQ-AQ-06)
- [ ] 9.5 **[REFACTOR]** Run `dotnet test` from `microservices/` — ALL tests MUST pass (including pre-existing). (REQ-DE-09, REQ-RM-07, REQ-AQ-06)
