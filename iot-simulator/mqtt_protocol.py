import socket
import struct


def mqtt_string(value):
    encoded = value.encode("utf-8")
    return struct.pack("!H", len(encoded)) + encoded


def encode_remaining_length(length):
    encoded = bytearray()
    while True:
        digit = length % 128
        length //= 128
        if length > 0:
            digit |= 0x80
        encoded.append(digit)
        if length == 0:
            return bytes(encoded)


def connect_mqtt(host, port, client_id):
    sock = socket.create_connection((host, port), timeout=10)
    sock.settimeout(10)

    variable_header = mqtt_string("MQTT") + bytes([4, 2]) + struct.pack("!H", 60)
    payload = mqtt_string(client_id)
    packet = bytes([0x10]) + encode_remaining_length(len(variable_header) + len(payload)) + variable_header + payload

    sock.sendall(packet)
    response = sock.recv(4)
    if len(response) != 4 or response[0] != 0x20 or response[3] != 0:
        raise RuntimeError(f"MQTT CONNACK rejected: {response!r}")

    return sock


def publish(sock, topic, payload):
    topic_bytes = mqtt_string(topic)
    payload_bytes = payload.encode("utf-8")
    packet = bytes([0x30]) + encode_remaining_length(len(topic_bytes) + len(payload_bytes)) + topic_bytes + payload_bytes
    sock.sendall(packet)
