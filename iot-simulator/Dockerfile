FROM python:3.12-slim

WORKDIR /firmware

COPY *.py .

ENV MQTT_HOST=mqtt-broker \
    MQTT_PORT=1883 \
    DEVICE_ID=aviario-01 \
    MQTT_TOPIC_TEMPLATE=granjatech/iot/{device_id}/telemetry \
    PUBLISH_INTERVAL_SECONDS=5 \
    FIRMWARE_VERSION=granjatech-fw-1.0.0

CMD ["python", "firmware.py"]
