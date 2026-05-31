import json
import math
import os
import random
import time
from datetime import datetime, timezone

from mqtt_protocol import connect_mqtt, publish


MQTT_HOST = os.getenv("MQTT_HOST", "mqtt-broker")
MQTT_PORT = int(os.getenv("MQTT_PORT", "1883"))
DEVICE_ID = os.getenv("DEVICE_ID", "aviario-01")
TOPIC_TEMPLATE = os.getenv("MQTT_TOPIC_TEMPLATE", "granjatech/iot/{device_id}/telemetry")
PUBLISH_INTERVAL_SECONDS = float(os.getenv("PUBLISH_INTERVAL_SECONDS", "5"))
FIRMWARE_VERSION = os.getenv("FIRMWARE_VERSION", "granjatech-fw-1.0.0")
RANDOM_SEED = os.getenv("RANDOM_SEED")

if RANDOM_SEED:
    random.seed(RANDOM_SEED)


def build_sensor_frame(sequence):
    elapsed = sequence * PUBLISH_INTERVAL_SECONDS
    cycle = math.sin(elapsed / 30.0)
    light_cycle = (math.sin(elapsed / 45.0) + 1.0) / 2.0

    temperature = 27.0 + (2.2 * cycle) + random.uniform(-0.35, 0.35)
    humidity = 63.0 - (5.0 * cycle) + random.uniform(-1.2, 1.2)
    luminosity = 260.0 + (620.0 * light_cycle) + random.uniform(-18.0, 18.0)

    return {
        "deviceId": DEVICE_ID,
        "firmwareVersion": FIRMWARE_VERSION,
        "timestamp": datetime.now(timezone.utc).isoformat(),
        "temperatureC": round(temperature, 2),
        "humidityPercent": round(max(0.0, min(100.0, humidity)), 2),
        "luminosityLux": round(max(0.0, luminosity), 2),
    }


def run():
    topic = TOPIC_TEMPLATE.format(device_id=DEVICE_ID)
    sequence = 0
    sock = None

    while True:
        try:
            if sock is None:
                client_id = f"{DEVICE_ID}-firmware"
                sock = connect_mqtt(MQTT_HOST, MQTT_PORT, client_id)
                print(f"[firmware] connected to mqtt://{MQTT_HOST}:{MQTT_PORT} as {client_id}", flush=True)

            frame = build_sensor_frame(sequence)
            payload = json.dumps(frame, separators=(",", ":"))
            publish(sock, topic, payload)
            print(f"[firmware] publish topic={topic} payload={payload}", flush=True)

            sequence += 1
            time.sleep(PUBLISH_INTERVAL_SECONDS)
        except Exception as exc:
            print(f"[firmware] mqtt error: {exc}. reconnecting in 3s", flush=True)
            try:
                if sock is not None:
                    sock.close()
            except OSError:
                pass
            sock = None
            time.sleep(3)


if __name__ == "__main__":
    run()
