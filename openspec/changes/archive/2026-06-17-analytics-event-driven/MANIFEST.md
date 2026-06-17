# Archive Manifest: analytics-event-driven

**Change**: `analytics-event-driven`
**Archive date**: 2026-06-17 (ISO format — UTC)
**Project**: `fundamentos_arq`
**Artifact store**: openspec (file-based)

---

## Archive Contents

This folder contains the complete record of the `analytics-event-driven` SDD change, closed on 2026-06-17.

### Primary Artifacts

1. **archive-report.md** — Final archive summary with deliverables, test results (85/85 PASS), findings resolution, and limitations
2. **proposal.md** — Original business case, scope, risks, and success criteria
3. **design.md** — Architecture-level decisions (ADRs 1-10) and design patterns (Transactional Outbox, Polly circuit breaker, CQRS read model)
4. **tasks.md** — Complete task list with all phases [x] marked complete; includes TDD cycle evidence and deviations from design

### Supporting Documents (delta specs)

- `specs/domain-events/spec.md` — Event publishing contract (archived reference)
- `specs/analytics-read-model/spec.md` — Event consumer and projections (archived reference)
- `specs/analytics-query/spec.md` — Query service rewrite (archived reference)

**Note**: Main capability specs are merged into `openspec/specs/` and are the authoritative source for future work.

### Implementation Evidence

- **apply-progress.md** — Full batched implementation record with:
  - TDD RED → GREEN → REFACTOR cycles
  - 6 implementation batches + 1 bug-fix batch + 1 cleanup batch
  - All files created/modified with line numbers
  - Build/test results per batch
  
- **verify-report.md** — Verification results with:
  - Build: 0 errors, no new warnings
  - Tests: 85/85 PASS (final state)
  - CRITICAL-1: AnalyticsEventConsumer production path (RESOLVED)
  - CRITICAL-2: OutboxWorker status persistence (RESOLVED)
  - WARNING-1: Spec property name alignment (RESOLVED)
  - WARNING-2: Stale docker-compose env vars (RESOLVED)
  - WARNING-A: ApplyEventAsync scope lifetime (RESOLVED in cleanup)
  - WARNING-B: Missing Projects persistence tests (RESOLVED in cleanup)

---

## Change Status

**CLOSED**: All phases complete. Change is archived and ready for production deployment.

| Phase | Status | Artifact |
|-------|--------|----------|
| Proposal | DONE | proposal.md |
| Spec | DONE | specs/ |
| Design | DONE | design.md |
| Tasks | DONE | tasks.md |
| Apply | DONE | apply-progress.md |
| Verify | DONE | verify-report.md |
| Archive | DONE | archive-report.md (this folder) |

---

## Key Deliverables

### NEW Capabilities
- **domain-events** — Event publishing with Transactional Outbox (RabbitMQ 7.0.0, Polly 8.5.x)
- **analytics-read-model** — Event consumer + 3 projection tables (DeviceProjection, ProjectProjection, UnitProjection)

### MODIFIED Capabilities
- **analytics-query** — Removed HTTP ACL, reads from local projections only

### Infrastructure
- **RabbitMQ** service in docker-compose (rabbitmq:4-management, AMQP 5672, UI 15672)
- **Outbox tables** in Devices and Projects DBs (mirroring Subscriptions pattern)

---

## Test Coverage

**Final**: 85/85 PASS (0 build errors, 5 pre-existing warnings)

- IoBuild.Shared.Tests: 19/19
- IoBuild.Devices.Tests: 36/36 (+2 persistence in batch 5/6)
- IoBuild.IAM.Tests: 3/3
- IoBuild.Projects.Tests: 8/8 (+2 persistence in batch 6)
- IoBuild.Subscriptions.Tests: 8/8
- IoBuild.Analytics.Tests: 11/11 (+2 production path in batch 5, +1 async fix in batch 6)

---

## Known Limitations

1. **Device.OwnerUserId = 0**: Device aggregate has no owner; Owner dashboard device count = 0 (documented)
2. **Eventual consistency**: ~5 second lag (outbox poll interval)
3. **EnsureCreated + snapshot cleanup**: Old tables not dropped on existing DBs; manual cleanup required in production
4. **No dead-letter queue**: Deferred enhancement
5. **No processed-events dedup**: Idempotency via upsert + LWW guard only

---

## Traceability

- **Proposal objectives**: All 5 success criteria met (✓)
- **Design ADRs**: All 10 decisions implemented and verified
- **Spec requirements**: All 26 requirements across 3 specs satisfied
- **Test assertions**: All required behaviors covered (39+ tests)
- **No regressions**: All pre-existing tests continue to pass

---

## Archive Closure

This change is now read-only and immutable. The archive folder serves as an audit trail and historical record. All active development continues from the main specs in `openspec/specs/`.

**Next steps**: None — change is complete. Future changes may reference domain-events, analytics-read-model, or analytics-query as dependencies or build upon them as extensions.

---

**Archived by**: sdd-archive phase executor
**Archive format**: openspec (per convention)
**Retention**: Permanent (immutable audit trail)
