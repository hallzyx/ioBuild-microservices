# Dynamic Simulator Device Registry — Design

**Date:** 2026-06-22
**Status:** Approved (pending spec review)
**Author:** brainstorming session (owner: Brayan)

## Problem

The IoT simulator (`microservices/iot-simulator/simulator.py`) simulates a **static** set of
devices: `range(1, DEVICE_COUNT + 1)` with `DEVICE_COUNT=5`. It publishes `telemetry/{id}` and
subscribes to `commands/{id}` for IDs 1–5 only.

Owner-controllable devices created through the builder flow (floor defaults, unit/room defaults)
and devices added by an owner get real IDs **outside** that range (observed: 13, 16). When an owner
issues a command, the backend correctly publishes to `commands/13`, but **no simulated device is
subscribed there**, so the command→ack round-trip never closes for those devices.

This was discovered during the live E2E that validated Change B (owner controls unit devices): the
MQTT publish path is proven (`commands/13 {"mode":"cooling"}` confirmed on the broker), but the
simulator ack cannot happen for owner devices.

## Goal

Make the simulator **dynamic and event-driven**: every device that exists in the system —
seeded, floor-provisioned, unit-provisioned, or owner-added — is automatically simulated, so the
command↔device match always works and the round-trip closes for any device.

Out of scope (YAGNI): type-specific telemetry. The simulator keeps its current generic telemetry
(`energy_kwh` / `temperature_c` / `voltage_v` + `reported` echo). The device `type` is used only
for registration/observability, not to tailor metrics.

## Architecture

```
Device created (floor default / unit default / owner-added / HasData seed)
        │ outbox row: DeviceCreatedEvent
        ▼
   OutboxWorker ──────────► RabbitMQ (unchanged)
        │
        └── (new hook) ──► IMqttPublisher: retained publish
                            registry/13  {"deviceId":13,"type":"AirConditioner"}
                                   │
   DeviceRegistryAnnouncer ───────┤  (startup: announces ALL DB devices, retained)
   (covers seeds + broker-restart recovery, independent of outbox)
                                   ▼
                         Simulator (subscribed to registry/#)
                            → adds device to dynamic set
                            → subscribe(commands/13)        ✓ match
                            → telemetry loop now includes 13

Owner command → POST /devices/13/command → shadow + outbox + MQTT commands/13
        → simulator (now subscribed) applies desired → publishes telemetry/13 (ack)
        → TelemetryWorker reconciles reported shadow.  Round-trip closed.
```

**Topic contract**
- `registry/{deviceId}` — retained, QoS 1.
  - Non-empty payload `{"deviceId": <int>, "type": "<catalogCode>"}` → device present.
  - **Empty payload** → tombstone (device removed; clears the retained message).
- `commands/{deviceId}` — unchanged (retained, QoS 1).
- `telemetry/{deviceId}` — unchanged.

## Components

### A. Backend — `IoBuild.Devices`

**A1. `IMqttPublisher` becomes topic-generic.**
Today `EnqueueAsync(deviceId, payload)` always builds `commands/{id}`. The internal channel tuple
changes from `(string DeviceId, string Payload)` to `(string Topic, string Payload, bool Retain)`.
- Existing command publish keeps a thin helper that builds `commands/{id}` (retain=true) — call sites unchanged.
- New method `EnqueueRawAsync(string topic, string payloadJson, bool retain, CancellationToken)` for registry publishes.
- `DrainLoopAsync` uses the tuple's topic + retain instead of the hardcoded `commands/` prefix.

**A2. `OutboxWorker` registry hook.**
In `RunOneCycleAsync`, after a domain event is successfully published to RabbitMQ:
- `DeviceCreatedEvent` → `EnqueueRawAsync("registry/{DeviceId}", {"deviceId","type"}, retain:true)`.
- `DeviceDeletedEvent` → `EnqueueRawAsync("registry/{DeviceId}", "", retain:true)` (tombstone).
- Best-effort: wrapped in try/catch; a registry-publish failure must NOT change the outbox row
  status (the row is already Processed for the RabbitMQ contract). Log and continue.

**A3. `DeviceRegistryAnnouncer` (new `BackgroundService`).**  *(load-bearing — not optional)*
On startup (after `db.Database.Migrate()` and seed application), query all `Devices` and
`EnqueueRawAsync("registry/{id}", {"deviceId","type"}, retain:true)` for each.
- Covers HasData seeds whose `OutboxBackfill` no-ops once the outbox has history.
- Covers recovery when the broker loses retained messages (e.g. `down -v` of the mosquitto volume).
- Enqueues through the channel, so it does not depend on the MQTT connection being up yet.
- Idempotent with A2 (same retained payload) — duplicate announces are harmless.

### B. Simulator — `simulator.py`

**B1. Dynamic device set.** Replace the static `range(1, DEVICE_COUNT+1)` with
`_devices: dict[int, dict]` (id → `{type}`) guarded by a `threading.Lock` (paho callbacks run on
the network-loop thread; the telemetry loop runs on the main thread).

**B2. Subscriptions.** In `on_connect`, subscribe to `registry/#` (and re-subscribe to
`commands/{id}` for every currently known device, so reconnects restore command subscriptions).

**B3. `on_message` routing.**
- `registry/{id}`: non-empty → add/update device, `subscribe(commands/{id})`; empty (tombstone) →
  remove device, `unsubscribe(commands/{id})`, drop its desired state.
- `commands/{id}`: existing ack logic (merge desired, echo telemetry).

**B4. Telemetry loop** iterates a snapshot of the dynamic set (under lock) instead of the range.
Static seeding and the `DEVICE_COUNT` env var are removed.

## Data flow (end to end)

1. Builder defines structure / owner adds device → device row persisted → `DeviceCreatedEvent` outbox row.
2. `OutboxWorker` publishes to RabbitMQ + retained `registry/{id}`.
3. Simulator receives `registry/{id}` → registers device + subscribes `commands/{id}`.
4. Owner sends command → `POST /devices/{id}/command` → shadow upsert + `commands/{id}` MQTT publish.
5. Simulator applies desired → publishes `telemetry/{id}` (ack) → `TelemetryWorker` reconciles reported shadow.

## Error handling

- **Registry publish (backend):** best-effort; never fails the outbox row. Retained messages + the
  startup announcer re-establish state after transient failures or broker restarts.
- **Simulator:** malformed registry/command payload → log and skip. All access to `_devices` under lock.
- **Duplicate announces:** harmless (retained + idempotent payload).

## Testing

- **Backend (strict TDD):**
  - `OutboxWorker` hook: after a `DeviceCreatedEvent` is published, a retained `registry/{id}` is
    enqueued with the correct `{deviceId,type}`; after `DeviceDeletedEvent`, an empty retained
    payload is enqueued. Use the existing `MqttPublisherService` internal test seam / a fake
    `IMqttPublisher`.
  - `DeviceRegistryAnnouncer`: announces a retained `registry/{id}` for every device in the DB.
  - `IMqttPublisher` refactor: command publishes still target `commands/{id}` with retain=true.
- **Simulator:** no Python test harness exists today → validated by **live E2E**.
- **Live E2E (validation gate):** rebuild `devices` + `simulator`; subscribe to `registry/#` and
  confirm announces for existing devices; create a new device and confirm its `registry/{id}`
  appears; from the owner UI send a command to device 13/16 and confirm the simulator ack on
  `telemetry/{id}` and the reconciled reported shadow.

## Affected files

- `microservices/src/IoBuild.Devices/Infrastructure/Mqtt/IMqttPublisher.cs` (+ `EnqueueRawAsync`)
- `microservices/src/IoBuild.Devices/Infrastructure/Mqtt/MqttPublisherService.cs` (channel tuple + drain)
- `microservices/src/IoBuild.Devices/Workers/OutboxWorker.cs` (registry hook)
- `microservices/src/IoBuild.Devices/Infrastructure/Mqtt/DeviceRegistryAnnouncer.cs` (new)
- `microservices/src/IoBuild.Devices/Program.cs` (register the announcer hosted service)
- `microservices/iot-simulator/simulator.py` (dynamic set + registry subscription)
- `microservices/docker-compose.yml` (drop simulator `DEVICE_COUNT`; no new ports)

## Open questions / risks

- Mosquitto retained-message persistence: the startup announcer makes this non-blocking, but a
  broker restart **without** a devices-service restart leaves a window where only already-running
  simulators retain their in-memory set. Acceptable; mosquitto's persistence volume is the backstop.
- `OutboxBackfill`'s outbox-empty guard means it is no longer the seed→simulator path; the announcer
  is. The backfill remains solely for populating the Analytics read model on first run.
