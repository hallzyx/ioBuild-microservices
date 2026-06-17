# Archive Report: analytics-event-driven

**Date**: 2026-06-17
**Change**: `analytics-event-driven`
**Status**: ARCHIVED (closed — fully delivered and verified)

---

## Executive Summary

The `analytics-event-driven` change is now complete and closed. The event-driven analytics read model replaces a broken synchronous HTTP ACL integration with an asynchronous Transactional Outbox pattern, delivering three new capabilities (`domain-events`, `analytics-read-model`) and one modified capability (`analytics-query`). The implementation achieved 85/85 test pass (two critical bugs found and fixed during verify phase), zero build errors, and full spec compliance.

---

## Delivered Capabilities

### NEW: domain-events

Event publishing contract with Transactional Outbox delivery guarantees (at-least-once, zero loss).

**Artifacts**:
- `IoBuild.Shared/Domain/Model/Events/DomainEvent.cs` (abstract base)
- 6 concrete event records: `DeviceCreatedEvent`, `DeviceUpdatedEvent`, `DeviceDeletedEvent`, `ProjectCreatedEvent`, `ProjectUpdatedEvent`, `UnitCreatedEvent`
- `IoBuild.Shared/Domain/Services/IDomainEventPublisher.cs`
- `IoBuild.Shared/Infrastructure/Messaging/RabbitMqDomainEventPublisher.cs` (AMQP publisher with publisher confirms)
- `IoBuild.Shared/Infrastructure/Messaging/DomainEventPublishingExtensions.cs` (DI registration + Polly circuit breaker)

**Key features**:
- Raw `RabbitMQ.Client` 7.0.0 transport (no MassTransit abstraction)
- Polly 8.5.x circuit breaker on the publish path
- Events carry `EventId` (Guid) + `OccurredOn` (UTC datetime) for idempotency
- RabbitMQ exchange: `iobuild.domain.events` (topic, durable)
- Routing keys: `device.device.{created|updated|deleted}`, `project.project.{created|updated}`, `project.unit.created`

**Test coverage**: 19/19 tests in `IoBuild.Shared.Tests` (DomainEventTests, CircuitBreakerTests)

---

### NEW: analytics-read-model

Event consumer and projection tables (DeviceProjection, ProjectProjection, UnitProjection).

**Artifacts**:
- EF entities: `DeviceProjection`, `ProjectProjection`, `UnitProjection` in `IoBuild.Analytics/Domain/Model/Projections/`
- Consumer: `AnalyticsEventConsumer : BackgroundService` (idempotent upsert with `last_event_at` LWW guard)
- `AnalyticsConsumerExtensions.cs` (DI registration)
- Modified `AnalyticsDbContext.cs`: projection DbSets added, snapshot tables removed
- Removed `builder_metrics` and `owner_metrics` seed data

**Key features**:
- Idempotency via upsert-by-natural-key + `last_event_at` guard (absorbs at-least-once delivery)
- Manual ack semantics: `BasicAck` on success, `BasicNack(requeue: true)` on transient error, `BasicNack(requeue: false)` on poison
- Consumer startup declares queue `analytics.read-model` (durable) and bindings `device.#`, `project.#`
- Production DI path: scope factory + dynamic scope per message (fixed in batch 5)
- Empty read model is valid (returns zeroed metrics without error)

**Test coverage**: 11/11 tests in `IoBuild.Analytics.Tests` (consumer idempotency, delete, empty model, no HTTP, production path)

---

### MODIFIED: analytics-query

Query services now read exclusively from local read-model tables; HTTP ACL facades removed.

**Artifacts**:
- Rewritten `AnalyticsQueryService.Handle(GetBuilderDashboard)`: joins `device_projection` to `project_projection` by builder, computes counts from projections only
- Rewritten `AnalyticsQueryService.Handle(GetOwnerDashboard)`: reads `device_projection` and `unit_projection` by owner user ID
- Rewritten `AnalyticsQueryService.Handle(GetHistoricalData)`: returns empty (telemetry out of scope)
- Removed constructor injection of `IDevicesContextFacade` and `IProjectsContextFacade`
- ACL facade classes (`DevicesContextFacade.cs`, `ProjectsContextFacade.cs`, interfaces) retained as orphaned dead code for rollback reference

**Key features**:
- No HTTP calls during query execution
- Zeroed/empty response when read model is empty (divide-by-zero safe)
- Eventual consistency explicitly documented in XML remarks
- HTTP API surface unchanged (no route/DTO changes)

**Test coverage**: 9/9 tests asserting no HTTP facade calls, empty model handling, data source correctness

---

## Test Results (Final)

**Build**: `dotnet build microservices/IoBuild.sln` → 0 errors, 5 pre-existing warnings (no new)

**Tests**: `dotnet test microservices/IoBuild.sln` → **85/85 ALL PASS**

Breakdown by project:
- `IoBuild.Shared.Tests`: 19/19
- `IoBuild.Devices.Tests`: 36/36 (+2 persistence tests added in batch 5 cleanup)
- `IoBuild.IAM.Tests`: 3/3
- `IoBuild.Projects.Tests`: 8/8 (+2 persistence tests added in batch 6 cleanup)
- `IoBuild.Subscriptions.Tests`: 8/8 (pre-existing, fixed stale constructor signature in batch 1)
- `IoBuild.Analytics.Tests`: 11/11 (+2 production-path tests added in batch 5, +1 async fix test in batch 6)

---

## Findings and Resolutions

### Batch 5: Critical Bugs (found by verify phase, fixed in apply)

#### CRITICAL-1: AnalyticsEventConsumer production path crashed on every message
**Root cause**: Consumer constructor path with `_scopeFactory` never opened a DI scope; upsert methods called `GetDb()` which threw `InvalidOperationException`.
**Resolution**: Upsert methods now accept `AnalyticsDbContext` parameter. `ApplyEventAsync` opens scope from `_scopeFactory` (production) or uses `_directDb` (test). `HandleDeliveryAsync` passes scoped db into event handler.
**Evidence**: 2 RED tests (ProductionConsumerPathTests) → GREEN after fix. 11/11 Analytics tests PASS.

#### CRITICAL-2: OutboxWorker status never persisted (infinite re-delivery loop)
**Root cause**: `OutboxMessageRepository.UpdateAsync` called `context.OutboxMessages.Update(message)` but never called `SaveChangesAsync`. EF change tracker mutations were discarded at scope disposal.
**Resolution**: Added `await context.SaveChangesAsync()` to `UpdateAsync` in both Devices and Projects outbox repositories.
**Evidence**: 1 RED test (OutboxMessageRepositoryPersistenceTests) → GREEN after fix. 36/36 Devices.Tests + 8/8 Projects.Tests PASS.

### Batch 5: Warnings (found by verify phase, resolved in apply)

#### WARNING-1: Spec property name divergence (OccurredAt vs OccurredOn)
**Issue**: REQ-DE-01 spec text said `OccurredAt`, code uses `OccurredOn`.
**Resolution**: Spec updated to `OccurredOn` (code is source of truth). Aligned spec text in batch 5.

#### WARNING-2: Stale docker-compose env vars
**Issue**: Analytics service stanza still had `Services__DevicesApi` and `Services__ProjectsApi` env vars referencing removed HTTP ACL facades.
**Resolution**: Removed from both `docker-compose.yml` and `docker-compose.prod.yml` in batch 5.

### Batch 6: Residual Risks (resolved in cleanup)

#### WARNING-A / SUGGESTION-1: ApplyEventAsync scope lifetime
**Issue**: Public test-helper method (ApplyEventAsync) opened DI scope synchronously but returned non-awaited Task; scope could dispose before Task completed (with real DB providers only).
**Resolution**: Rewritten as `async Task`. Now `await`s inside the `using` scope block, keeping scope alive for the full operation. No impact on production (HandleDeliveryAsync was already correct).

#### WARNING-B / SUGGESTION-2: Missing Projects persistence tests
**Issue**: Only Devices had persistence tests for OutboxMessageRepository. Projects fix was confirmed by code inspection but not tested.
**Resolution**: Created `OutboxMessageRepositoryPersistenceTests.cs` in `IoBuild.Projects.Tests` with three-context pattern (write / worker / read), matching Devices tests.

---

## Known Limitations and Documented Trade-offs

1. **Device.OwnerUserId = 0**: The `Device` aggregate has no owner concept (only `ProjectId`). Owner device counts on the Owner dashboard = 0. Documented as source-data limitation; not a regression (HTTP path was already non-functional).

2. **Eventual consistency**: Analytics metrics lag Devices/Projects by ~5 seconds (outbox poll interval). Explicitly documented in query service XML remarks and design documents. Acceptable for an analytics surface.

3. **EnsureCreated + snapshot table cleanup**: Services use `EnsureCreated`, not `Migrate`. New projection tables are auto-created; old snapshot tables (`builder_metrics`, `owner_metrics`) are no longer in `OnModelCreating` and will NOT be dropped on existing DBs — must be manually dropped in production after deployment.

4. **No dead-letter queue**: Failed consumer messages are re-queued (BasicNack with requeue:true on transient error, requeue:false on poison). No DLQ wired; optional future enhancement.

5. **No processed-events dedup table**: Idempotency is achieved via upsert-by-natural-key + `last_event_at` guard. Optional optimization (dedup table) deferred.

---

## Artifact Merge Summary

**Delta specs** (from change) → **Main specs** (in openspec/specs/):

| Delta spec | Main spec | Status |
|---|---|---|
| `openspec/changes/analytics-event-driven/specs/domain-events/spec.md` | `openspec/specs/domain-events/spec.md` | Created |
| `openspec/changes/analytics-event-driven/specs/analytics-read-model/spec.md` | `openspec/specs/analytics-read-model/spec.md` | Created |
| `openspec/changes/analytics-event-driven/specs/analytics-query/spec.md` | `openspec/specs/analytics-query/spec.md` | Created |

Main specs now define the new domain-events and analytics-read-model capabilities, and the modified analytics-query capability. All three are active and ready for future use as reference specs.

---

## Change Folder Archive

The completed change folder is archived at:
```
openspec/changes/archive/2026-06-17-analytics-event-driven/
```

Contents:
- `proposal.md` — original business case and scope
- `design.md` — architecture decisions (ADRs 1-10)
- `specs/` — delta specs for three capabilities
- `tasks.md` — complete task list with all batches marked [x] and bug-fix notes
- `apply-progress.md` — full implementation record with TDD evidence
- `verify-report.md` — verification results, critical bugs found/fixed, re-verify pass
- `state.yaml` — final DAG state (all phases done)

The archive folder is immutable and serves as an audit trail.

---

## Observation IDs (Engram — not applicable)

**Note**: This project uses **openspec** artifact store (file-based), not Engram. No observation IDs exist. All artifacts are persisted as files in the git repository.

---

## Traceability

**Change**: `analytics-event-driven`
**Project**: `fundamentos_arq`
**Delivery**: Single PR (`feat/analytics-event-driven`) with size:exception, merged to main
**Verification**: PASS (85/85 tests, 0 build errors)
**Archive date**: 2026-06-17 ISO format
**Closed by**: sdd-archive phase executor

---

## Next Steps

None — change is fully closed. The three capabilities (domain-events, analytics-read-model, analytics-query) are now active and archived. Future changes may reference these specs as dependencies.

The dead-letter queue, bounded retry, and processed-events optimization are marked as future hardening (out of scope).

---

## Sign-off

Change `analytics-event-driven` is archived and ready for production deployment. All verification gates passed.
