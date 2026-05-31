import json
import os
import time
from datetime import datetime, timezone

from mqtt_protocol import connect_mqtt, publish


MQTT_HOST = os.getenv("MQTT_HOST", "mqtt-broker")
MQTT_PORT = int(os.getenv("MQTT_PORT", "1883"))
DEVICE_ID = os.getenv("DEVICE_ID", "aviario-01")
TOPIC_TEMPLATE = os.getenv("MQTT_TOPIC_TEMPLATE", "granjatech/iot/{device_id}/telemetry")
FIRMWARE_VERSION = os.getenv("FIRMWARE_VERSION", "granjatech-fw-1.0.0")

SENSORS = {
    "1": {
        "label": "Temperatura",
        "field": "temperatureC",
        "unit": "C",
        "identifier": f"{DEVICE_ID}-temperature",
    },
    "2": {
        "label": "Umidade",
        "field": "humidityPercent",
        "unit": "%",
        "identifier": f"{DEVICE_ID}-humidity",
    },
    "3": {
        "label": "Luminosidade",
        "field": "luminosityLux",
        "unit": "lux",
        "identifier": f"{DEVICE_ID}-luminosity",
    },
}


def print_menu():
    print("")
    print("=== GranjaTech IoT - Simulador manual ===")
    print(f"Dispositivo: {DEVICE_ID}")
    print(f"Broker MQTT: {MQTT_HOST}:{MQTT_PORT}")
    print("")
    for key, sensor in SENSORS.items():
        print(f"{key}) {sensor['label']} ({sensor['identifier']})")
    print("q) sair")
    print("")


def read_value(sensor):
    while True:
        raw_value = input(f"Valor para {sensor['label']} ({sensor['unit']}): ").strip().replace(",", ".")
        try:
            return round(float(raw_value), 2)
        except ValueError:
            print("Digite um numero valido. Exemplo: 28.5")


def build_payload(sensor, value):
    payload = {
        "deviceId": DEVICE_ID,
        "firmwareVersion": f"{FIRMWARE_VERSION}-manual",
        "timestamp": datetime.now(timezone.utc).isoformat(),
        sensor["field"]: value,
    }
    return json.dumps(payload, separators=(",", ":"))


def connect_with_retry():
    client_id = f"{DEVICE_ID}-manual-simulator"
    while True:
        try:
            sock = connect_mqtt(MQTT_HOST, MQTT_PORT, client_id)
            print(f"Conectado em mqtt://{MQTT_HOST}:{MQTT_PORT} como {client_id}")
            return sock
        except Exception as exc:
            print(f"Falha ao conectar no MQTT: {exc}. Tentando novamente em 3s...")
            time.sleep(3)


def main():
    topic = TOPIC_TEMPLATE.format(device_id=DEVICE_ID)
    sock = connect_with_retry()

    while True:
        print_menu()
        choice = input("Escolha o sensor: ").strip().lower()

        if choice in {"q", "sair", "exit"}:
            print("Encerrando simulador manual.")
            break

        sensor = SENSORS.get(choice)
        if sensor is None:
            print("Opcao invalida.")
            continue

        value = read_value(sensor)
        payload = build_payload(sensor, value)

        try:
            publish(sock, topic, payload)
        except Exception:
            try:
                sock.close()
            except OSError:
                pass
            sock = connect_with_retry()
            publish(sock, topic, payload)

        print("")
        print(f"Publicado: {sensor['label']} = {value} {sensor['unit']}")
        print(f"Topico: {topic}")
        print(f"Payload: {payload}")
        print("Abra a tela Sensores e atualize o sensor correspondente.")

    try:
        sock.close()
    except OSError:
        pass


if __name__ == "__main__":
    main()
