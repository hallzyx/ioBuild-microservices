"""
IoT Telemetry Simulator for IoBuild.

Publishes realistic telemetry data for 5 devices to an MQTT broker.
Each device publishes every 5 seconds with random sensor values.

Environment variables:
  MQTT_HOST — MQTT broker host (default: localhost)
  MQTT_PORT — MQTT broker port (default: 1883)
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


def generate_payload(device_id: int) -> dict:
    """Generate realistic telemetry data for a device."""
    return {
        "deviceId": device_id,
        "timestamp": datetime.now(timezone.utc).isoformat(),
        "energy_kwh": round(random.uniform(0.5, 3.0), 2),
        "temperature_c": round(random.uniform(18.0, 35.0), 1),
        "voltage_v": round(random.uniform(215.0, 230.0), 1),
        "status": random.choice(STATUS_WEIGHTS),
        "location": LOCATIONS[(device_id - 1) % len(LOCATIONS)],
    }


def on_connect(client, userdata, flags, rc, properties=None):
    if rc == 0:
        print(f"Connected to MQTT broker at {MQTT_HOST}:{MQTT_PORT}")
    else:
        print(f"Failed to connect to MQTT broker, return code {rc}")


def main():
    client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2)
    client.on_connect = on_connect

    try:
        client.connect(MQTT_HOST, MQTT_PORT, keepalive=60)
        client.loop_start()
    except Exception as e:
        print(f"Error connecting to MQTT broker: {e}")
        return

    print(f"Starting simulator for {DEVICE_COUNT} devices...")
    print(f"Topics: telemetry/1 through telemetry/{DEVICE_COUNT}")

    while True:
        for device_id in range(1, DEVICE_COUNT + 1):
            payload = generate_payload(device_id)
            topic = f"telemetry/{device_id}"
            result = client.publish(topic, json.dumps(payload), qos=1)
            print(
                f"[{payload['timestamp']}] Published to {topic}: "
                f"energy={payload['energy_kwh']}kWh, "
                f"temp={payload['temperature_c']}C, "
                f"voltage={payload['voltage_v']}V, "
                f"status={payload['status']}"
            )

        time.sleep(5)


if __name__ == "__main__":
    main()
