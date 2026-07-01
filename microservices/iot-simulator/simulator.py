"""
IoT Telemetry Simulator for IoBuild.

Dynamic device set: devices are discovered at runtime via retained MQTT
registry/{deviceId} messages published by the Devices service
(OutboxWorker hook + DeviceRegistryAnnouncer). For each known device the
simulator publishes telemetry/{deviceId} every 5 seconds and subscribes
to commands/{deviceId} (QoS 1, retain). On a command it applies the
desired attributes and immediately echoes telemetry so the backend can
reconcile reported state.

Registry contract:
  registry/{deviceId}  (retained, QoS 1)
    non-empty {"deviceId": <int>, "type": "<code>"}  -> device present
    empty payload                                    -> tombstone (remove)

Environment variables:
  MQTT_HOST  - MQTT broker host (default: localhost)
  MQTT_PORT  - MQTT broker port (default: 1883)
"""

import os
import json
import time
import random
import threading
from datetime import datetime, timezone

import paho.mqtt.client as mqtt

MQTT_HOST = os.getenv("MQTT_HOST", "localhost")
MQTT_PORT = int(os.getenv("MQTT_PORT", "1883"))

LOCATIONS = ["Sector-A", "Sector-B", "Sector-C", "Sector-D", "Sector-E"]
STATUS_WEIGHTS = ["online"] * 3 + ["idle"]  # 75% online, 25% idle

# Dynamic device registry: id -> {"type": str}. Guarded by _lock because paho
# callbacks run on the network-loop thread while the telemetry loop runs on main.
_devices: dict[int, dict] = {}
# Per-device desired state applied from commands/{deviceId}.
_device_desired: dict[int, dict] = {}
_lock = threading.Lock()


def generate_payload(device_id: int, desired: dict) -> dict:
    # Scale energy by intensity/brightness if set (0-100 range, default 100).
    # Allows owner-controlled dimming commands to reflect in telemetry consumption.
    intensity = desired.get('intensity', 100)
    brightness = desired.get('brightness', intensity)  # fallback alias
    scale = max(0.0, min(1.0, brightness / 100.0))
    base_energy = random.uniform(0.5, 3.0)
    energy_kwh = round(base_energy * scale if scale < 1.0 else base_energy, 2)

    payload = {
        "deviceId": device_id,
        "timestamp": datetime.now(timezone.utc).isoformat(),
        "energy_kwh": energy_kwh,
        "temperature_c": round(random.uniform(18.0, 35.0), 1),
        "voltage_v": round(random.uniform(215.0, 230.0), 1),
        "status": random.choice(STATUS_WEIGHTS),
        "location": LOCATIONS[(device_id - 1) % len(LOCATIONS)],
    }
    if desired:
        payload["reported"] = desired
    return payload


def on_connect(client, userdata, flags, rc, properties=None):
    if rc == 0:
        print(f"Connected to MQTT broker at {MQTT_HOST}:{MQTT_PORT}")
        # Discover devices: retained registry messages are delivered on subscribe.
        client.subscribe("registry/#", qos=1)
        print("Subscribed to registry/#")
        # Restore command subscriptions for already-known devices after a reconnect.
        with _lock:
            known = list(_devices.keys())
        for device_id in known:
            client.subscribe(f"commands/{device_id}", qos=1)
    else:
        print(f"Failed to connect to MQTT broker, return code {rc}")


def _handle_registry(client, device_id: int, raw: bytes):
    if not raw:  # tombstone -> remove device
        with _lock:
            _devices.pop(device_id, None)
            _device_desired.pop(device_id, None)
        client.unsubscribe(f"commands/{device_id}")
        print(f"[REGISTRY] device {device_id} removed (tombstone)")
        return
    info = json.loads(raw.decode("utf-8"))
    dtype = info.get("type", "unknown")
    with _lock:
        is_new = device_id not in _devices
        _devices[device_id] = {"type": dtype}
    client.subscribe(f"commands/{device_id}", qos=1)
    if is_new:
        print(f"[REGISTRY] device {device_id} registered type={dtype}; subscribed commands/{device_id}")


def _handle_command(client, device_id: int, raw: bytes):
    command = json.loads(raw.decode("utf-8"))
    with _lock:
        _device_desired.setdefault(device_id, {}).update(command)
        desired = dict(_device_desired[device_id])
    applied = ", ".join(f"{k}={v}" for k, v in command.items())
    print(f"[CMD] device={device_id} applied: {applied} | desired={desired}")
    ack = generate_payload(device_id, desired)
    client.publish(f"telemetry/{device_id}", json.dumps(ack), qos=1)
    print(f"[ACK] Published updated telemetry to telemetry/{device_id}")


def on_message(client, userdata, message):
    topic = message.topic
    try:
        parts = topic.split("/")
        if len(parts) != 2:
            print(f"[WARN] Unexpected topic: {topic}")
            return
        kind, id_str = parts
        device_id = int(id_str)
        if kind == "registry":
            _handle_registry(client, device_id, message.payload)
        elif kind == "commands":
            _handle_command(client, device_id, message.payload)
        else:
            print(f"[WARN] Unhandled topic kind: {topic}")
    except (ValueError, json.JSONDecodeError) as exc:
        print(f"[ERROR] Failed to process message on {topic}: {exc}")


def main():
    client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2)
    client.on_connect = on_connect
    client.on_message = on_message

    try:
        client.connect(MQTT_HOST, MQTT_PORT, keepalive=60)
        client.loop_start()
    except Exception as e:
        print(f"Error connecting to MQTT broker: {e}")
        return

    print("Starting dynamic simulator (devices discovered via registry/#)...")

    while True:
        with _lock:
            snapshot = [(d, dict(_device_desired.get(d, {}))) for d in _devices]
        for device_id, desired in snapshot:
            payload = generate_payload(device_id, desired)
            client.publish(f"telemetry/{device_id}", json.dumps(payload), qos=1)
            print(
                f"[{payload['timestamp']}] Published to telemetry/{device_id}: "
                f"energy={payload['energy_kwh']}kWh, temp={payload['temperature_c']}C, "
                f"voltage={payload['voltage_v']}V, status={payload['status']}"
                + (f", reported={payload['reported']}" if "reported" in payload else "")
            )
        time.sleep(5)


if __name__ == "__main__":
    main()
