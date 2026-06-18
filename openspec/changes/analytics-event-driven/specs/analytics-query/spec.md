# Spec: analytics-query

**Change**: analytics-event-driven
**Capability**: MODIFIED `analytics-query`
**Status**: draft (revised — note at-least-once delivery contract)

---

## Context

`IoBuild.Analytics` currently calls `DevicesContextFacade` and `ProjectsContextFacade` (HTTP ACL clients) at query time to retrieve device and project counts. Those HTTP calls target endpoints that do not exist, causing the facades to silently return empty data. This spec replaces that broken path: after this change, all query logic reads exclusively from the local read-model tables introduced by the `analytics-read-model` capability.

**Upstream delivery contract**: events are produced via a Transactional Outbox (at-least-once). Idempotency is enforced at the read-model layer (`analytics-read-model` REQ-RM-03); by the time the query layer reads from the projection tables, duplicate events have already been collapsed. The query layer has no idempotency responsibility of its own.

---

## Requirements

### REQ-AQ-01 — No outbound HTTP calls from Analytics query path

After this change, Analytics query services MUST NOT make outbound HTTP calls to any other service (`IoBuild.Devices`, `IoBuild.Projects`, or any other) during query execution.

`DevicesContextFacade`, `ProjectsContextFacade`, and any other HTTP ACL client used solely by the query path MUST be removed or made unreachable from query service code. Their removal MUST be verified: no query code path MAY transitively invoke an HTTP client.

### REQ-AQ-02 — Query source is exclusively the local read model

All Analytics query services MUST read device, project, and unit data exclusively from the local `iobuild_analytics` read-model tables (`DeviceProjection`, `ProjectProjection`, `UnitProjection` as defined in `analytics-read-model/spec.md`).

No direct cross-database queries (e.g., querying into `iobuild_devices` or `iobuild_projects` from Analytics) are permitted.

### REQ-AQ-03 — Empty read model yields zeroed/empty metrics, not an error

If the read-model tables are empty (no events received yet), Analytics query endpoints MUST return a valid response with zeroed or empty metric values (e.g., `totalDevices: 0`, `projects: []`).

An empty read model MUST NOT cause any query endpoint to return HTTP 500, throw an unhandled exception, or surface an error to the caller.

### REQ-AQ-04 — Eventual consistency is documented behavior

Analytics metrics MUST be documented (at minimum in code/API comments) as eventually consistent with the source services. The query layer MUST NOT implement any compensating synchronous call to "fill in" missing data at query time.

### REQ-AQ-05 — Existing query API surface preserved

The HTTP endpoints exposed by Analytics (routes, request/response shapes) MUST NOT change as a result of this modification. Only the data source of those endpoints changes.

### REQ-AQ-06 — Build and test integrity

After this capability is modified:
- `dotnet build` MUST succeed with zero errors.
- `dotnet test` MUST pass.
- A test MUST assert that when the read-model tables are empty, the query response is a valid (non-error) zeroed result.
- A test MUST assert that no HTTP client is invoked during a query service call (e.g., by injecting a mock and verifying zero calls, or by asserting the dependency is not registered).

---

## Scenarios

### Scenario AQ-S01 — Query returns data from local read model

```
Given the DeviceProjection table contains 3 rows
And the ProjectProjection table contains 2 rows
And no HTTP ACL client is registered in the Analytics DI container for query services
When a GET request is made to the Analytics device-count or summary endpoint
Then the response body reflects 3 devices and 2 projects
And zero HTTP calls are made to IoBuild.Devices or IoBuild.Projects
```

### Scenario AQ-S02 — Empty read model returns zeroed metrics, not an error

```
Given all read-model tables (DeviceProjection, ProjectProjection, UnitProjection) are empty
When a GET request is made to any Analytics metrics endpoint
Then the HTTP response status is 200 OK
And the response body contains zeroed or empty metric values (e.g. count = 0, list = [])
And the response does not contain an error message or stack trace
```

### Scenario AQ-S03 — ACL facades are removed from query code paths

```
Given the Analytics service is built and its DI container is configured
When any Analytics query service method is invoked
Then no call is made to DevicesContextFacade or ProjectsContextFacade
And no outbound HTTP request to another microservice is issued
```

### Scenario AQ-S04 — Read model populated, query reflects current state

```
Given a DeviceCreated event was consumed and DeviceProjection now has 1 row for DeviceId = X
When a GET request is made to the Analytics device-count endpoint
Then the response includes DeviceId = X in its result set (or count = 1)
And the result is served without any HTTP call to IoBuild.Devices
```

### Scenario AQ-S05 — Deleted device is not counted

```
Given DeviceProjection previously had a row for DeviceId = X
And a DeviceDeleted event was consumed, removing or deactivating DeviceId = X
When a GET request is made to the Analytics device-count endpoint
Then DeviceId = X is NOT included in the active count or result set
```

---

## Out of scope for this spec

- Changes to Analytics HTTP endpoint routes or response DTOs.
- Caching or pagination of query results.
- Telemetry / energy metrics (InfluxDB path) — unaffected by this change.
- Compensating read-through to source services (explicitly prohibited by REQ-AQ-04).
