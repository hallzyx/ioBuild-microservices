# Proposal: Event-Driven Analytics Read Model

## Intent

`IoBuild.Analytics` currently depends on `IoBuild.Devices` and `IoBuild.Projects` via synchronous HTTP (ACL facades). This violates the hard constraint that **microservices must not have direct runtime dependencies on each other** (course requirement, treated as production). The HTTP fallback is also already broken — the ACL facades call endpoints that do not exist in the target controllers, so they return empty. We replace this fictional integration with an event-driven local read model, decoupling Analytics at runtime.

## Scope

### In Scope
- Publish domain events to **RabbitMQ** after state changes in Devices (Device created/updated/deleted) and Projects (Project created/updated, Unit created).
- Analytics consumes events via a `BackgroundService` and maintains its OWN read-model tables in `iobuild_analytics`.
- Analytics query path reads ONLY from its local DB — remove all HTTP ACL calls.
- **Circuit Breaker** on the RabbitMQ publish path (Polly or native resilience) so a broker outage cannot crash Devices/Projects.
- Add RabbitMQ container to docker-compose (dev + prod).
- Domain events derive from the existing `IoBuild.Shared.IEvent` marker.

### Out of Scope
- Telemetry (energy/status in InfluxDB via MQTT) — stays as-is.
- Backfill/replay of historical state (read model starts empty, fills forward).
- Migrating other services (IAM, Subscriptions, Profiles) to events.
- Resolving the Pomelo preview.2/preview.3 version drift (noted, deferred).

## Capabilities

### New Capabilities
- `domain-events`: shared event publishing contract (RabbitMQ publisher, exchange/routing conventions, circuit-breaker resilience) built on `IoBuild.Shared.IEvent`.
- `analytics-read-model`: Analytics-owned tables + event consumer (`BackgroundService`) that projects Devices/Projects events into local query data.

### Modified Capabilities
- `analytics-query`: query services MUST read only from the local read model; the HTTP ACL fallback is REMOVED.

## Approach

Each command service (`DeviceCommandService`, `ProjectCommandService`, `UnitCommandService`) publishes a domain event AFTER successful persistence (`SaveChangesAsync` / `IUnitOfWork` commit). A shared publisher (modeled on resilient patterns) wraps RabbitMQ behind a circuit breaker. Analytics runs a consumer `BackgroundService` (mirroring the proven `TelemetryWorker` pattern) that upserts projection rows. Queries read those rows directly. **Eventual consistency is the accepted, documented trade-off** — the dashboard may lag the source of truth by seconds.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `IoBuild.Shared` | Modified | Event contracts + RabbitMQ publisher abstraction |
| `IoBuild.Devices` | Modified | Publish Device events from command service; add RabbitMQ pkg |
| `IoBuild.Projects` | Modified | Publish Project/Unit events; add RabbitMQ pkg |
| `IoBuild.Analytics` | Modified | New read-model tables, consumer BackgroundService; remove ACL HTTP |
| `docker-compose.*` | Modified | New RabbitMQ service (AMQP, not Mosquitto) |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Broker outage drops events | Med | Circuit breaker isolates failure; events re-publish on recovery |
| Eventual consistency confuses graders | Med | Document explicitly as a deliberate decision |
| Read model empty until events flow | High | Documented; starts forward-filling on deploy |
| Version drift (Pomelo) complicates Shared pkg | Low | Pin RabbitMQ pkg version consistently across services |

## Rollback Plan

Revert the command-service publish calls and the Analytics consumer; restore (or leave dormant) the ACL facades. Remove the RabbitMQ compose service. Since the HTTP fallback was already non-functional, rollback degrades Analytics to its prior empty-snapshot state with no regression to Devices/Projects.

## Dependencies

- RabbitMQ broker container (AMQP) added to docker-compose.
- A single RabbitMQ client NuGet version agreed across the 3 publishing/consuming services.

## Success Criteria

- [x] Analytics makes ZERO runtime HTTP calls to Devices/Projects.
- [x] Devices/Projects publish events on create/update/delete after persistence.
- [x] Analytics read model populates from consumed events and serves queries locally.
- [x] Broker outage does not crash any service (circuit breaker proven).
- [x] `dotnet build` and `dotnet test` pass across the solution.
