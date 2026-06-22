"""
IoT Telemetry Simulator for IoBuild.

Publishes realistic telemetry data for 5 devices to an MQTT broker.
Each device publishes every 5 seconds with random sensor values.

Subscribes to commands/{deviceId} (QoS 1, retain) for each device.
On receiving a command, applies the desired attributes to the device's
in-memory state and immediately publishes updated telemetry to
telemetry/{deviceId} so the backend can reconcile reported state.

Environment variables:
  MQTT_HOST    — MQTT broker host (default: localhost)
  MQTT_PORT    — MQTT broker port (default: 1883)
  DEVICE_COUNT — Number of simulated devices (default: 5)
"""

import os
import json
import time
import random
from datetime import datetime, timezone

import paho.mqtt.client as mqtt

MQTT_HOST = os.getenv("MQTT_HOST", "localhost")
MQTT_PORT = int(os.getenv("MQTT_PORT", "1883"))
DEVICE_COUNT = int(os.getenv("DEVICE_COUNT", "5"))

LOCATIONS = ["Sector-A", "Sector-B", "Sector-C", "Sector-D", "Sector-E"]
STATUS_WEIGHTS = ["online"] * 3 + ["idle"]  # 75% online, 25% idle

# Per-device desired state applied from commands/{deviceId}.
# Keys match DeviceCapabilityCatalog attribute names verbatim:
#   targetTemperature, mode, brightness, power
# Initialised empty; entries are added on first command receipt.
_device_desired: dict[int, dict] = {}


def _get_desired(device_id: int) -> dict:
    """Return the current desired-state overrides for a device (may be empty)."""
    return _device_desired.get(device_id, {})


def generate_payload(device_id: int) -> dict:
    """Generate telemetry data for a device, merging in any desired-state overrides."""
    desired = _get_desired(device_id)
    payload = {
        "deviceId": device_id,
        "timestamp": datetime.now(timezone.utc).isoformat(),
        "energy_kwh": round(random.uniform(0.5, 3.0), 2),
        "temperature_c": round(random.uniform(18.0, 35.0), 1),
        "voltage_v": round(random.uniform(215.0, 230.0), 1),
        "status": random.choice(STATUS_WEIGHTS),
        "location": LOCATIONS[(device_id - 1) % len(LOCATIONS)],
    }
    # Reflect desired controllable attributes in the telemetry payload so the
    # backend and TelemetryWorker can observe the reported state.
    if desired:
        payload["reported"] = desired
    return payload


def on_connect(client, userdata, flags, rc, properties=None):
    if rc == 0:
        print(f"Connected to MQTT broker at {MQTT_HOST}:{MQTT_PORT}")
        # Re-subscribe on every (re)connect so retained commands are delivered
        # even after a broker-initiated disconnect.
        for device_id in range(1, DEVICE_COUNT + 1):
            topic = f"commands/{device_id}"
            client.subscribe(topic, qos=1)
            print(f"Subscribed to {topic}")
    else:
        print(f"Failed to connect to MQTT broker, return code {rc}")


def on_message(client, userdata, message):
    """Handle incoming command messages on commands/{deviceId}."""
    topic = message.topic  # e.g. "commands/3"
    try:
        parts = topic.split("/")
        if len(parts) != 2 or parts[0] != "commands":
            print(f"[WARN] Unexpected topic: {topic}")
            return

        device_id = int(parts[1])
        command = json.loads(message.payload.decode("utf-8"))

        # Merge each attribute from the command into the device's desired state.
        if device_id not in _device_desired:
            _device_desired[device_id] = {}
        _device_desired[device_id].update(command)

        applied = ", ".join(f"{k}={v}" for k, v in command.items())
        print(f"[CMD] device={device_id} applied: {applied} | desired={_device_desired[device_id]}")

        # Immediately echo the updated state as telemetry so the backend can
        # reconcile the reported shadow without waiting for the next 5-s cycle.
        ack_payload = generate_payload(device_id)
        telemetry_topic = f"telemetry/{device_id}"
        client.publish(telemetry_topic, json.dumps(ack_payload), qos=1)
        print(f"[ACK] Published updated telemetry to {telemetry_topic}")

    except (ValueError, json.JSONDecodeError) as exc:
        print(f"[ERROR] Failed to process command on {topic}: {exc}")


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

    print(f"Starting simulator for {DEVICE_COUNT} devices...")
    print(f"Telemetry topics: telemetry/1 through telemetry/{DEVICE_COUNT}")
    print(f"Command topics:   commands/1 through commands/{DEVICE_COUNT}")

    while True:
        for device_id in range(1, DEVICE_COUNT + 1):
            payload = generate_payload(device_id)
            topic = f"telemetry/{device_id}"
            client.publish(topic, json.dumps(payload), qos=1)
            print(
                f"[{payload['timestamp']}] Published to {topic}: "
                f"energy={payload['energy_kwh']}kWh, "
                f"temp={payload['temperature_c']}C, "
                f"voltage={payload['voltage_v']}V, "
                f"status={payload['status']}"
                + (f", reported={payload['reported']}" if "reported" in payload else "")
            )

        time.sleep(5)


if __name__ == "__main__":
    main()
