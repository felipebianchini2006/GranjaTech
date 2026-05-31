# Simulador IoT via Docker

Este fluxo substitui o envio manual pelo Swagger por uma simulacao MQTT em containers.

## Arquitetura

1. `iot-simulator` executa o firmware Python e publica telemetria no broker MQTT.
2. `mqtt-broker` executa Eclipse Mosquitto na porta `1883`.
3. `backend` assina o topico `granjatech/iot/+/telemetry`, processa a mensagem e grava as leituras no PostgreSQL.
4. A tela `Sensores` continua lendo os dados pela API existente.

## Payload MQTT

Topico padrao:

```text
granjatech/iot/aviario-01/telemetry
```

Exemplo de mensagem:

```json
{
  "deviceId": "aviario-01",
  "firmwareVersion": "granjatech-fw-1.0.0",
  "timestamp": "2026-05-31T18:00:00Z",
  "temperatureC": 27.4,
  "humidityPercent": 62.8,
  "luminosityLux": 615.2
}
```

Com `Mqtt:AutoProvision=true`, a API cria automaticamente:

| Tipo | Identificador unico |
| --- | --- |
| Temperatura | `aviario-01-temperature` |
| Umidade | `aviario-01-humidity` |
| Luminosidade | `aviario-01-luminosity` |

Esses sensores ficam vinculados a granja `GRJ-IOT`.

## Como validar

Suba o ambiente completo:

```bash
docker compose up --build
```

Se a porta local `5432` ja estiver ocupada por outro PostgreSQL, use:

```bash
POSTGRES_HOST_PORT=15432 docker compose up --build
```

Confira os logs do firmware:

```bash
docker compose logs -f iot-simulator
```

Confira a ingestao da API:

```bash
docker compose logs -f backend
```

Abra o status publico da integracao:

```text
http://localhost:5099/api/iot/status
```

Abra o Swagger:

```text
http://localhost:5099/swagger
```

Na apresentacao, o caminho visual e:

1. Subir `docker compose up --build`.
2. Mostrar logs do `iot-simulator` publicando MQTT.
3. Abrir `http://localhost:5099/api/iot/status` e verificar `messagesReceived`.
4. Entrar no frontend em `http://localhost:3000`.
5. Abrir `Sensores`, selecionar `aviario-01-temperature`, `aviario-01-humidity` ou `aviario-01-luminosity` e atualizar o grafico.

## Simulacao manual para apresentacao

No Windows, use o executavel de atalho da raiz do projeto:

```text
abrir-simulador-iot.cmd
```

Pode abrir com duplo clique. Ele sobe `postgres`, `mqtt-broker`, `backend` e `frontend`, pausa o `iot-simulator` automatico e abre o menu manual.

Com o sistema ja rodando, pare o simulador automatico se quiser controlar os valores sem novas leituras aleatorias entrando a cada 5 segundos:

```bash
docker compose stop iot-simulator
```

Depois rode o menu manual:

```bash
docker compose run --rm iot-manual-simulator
```

O terminal vai mostrar:

```text
1) Temperatura (aviario-01-temperature)
2) Umidade (aviario-01-humidity)
3) Luminosidade (aviario-01-luminosity)
q) sair
```

Escolha uma opcao, digite o valor e pressione Enter. O script publica uma mensagem MQTT com apenas aquele sensor preenchido, por exemplo:

```json
{
  "deviceId": "aviario-01",
  "firmwareVersion": "granjatech-fw-1.0.0-manual",
  "timestamp": "2026-05-31T18:00:00Z",
  "temperatureC": 30.5
}
```

Na tela `Sensores`, selecione o sensor correspondente e clique em `Atualizar` para ver o novo ponto no grafico.

Para voltar ao fluxo automatico:

```bash
docker compose up -d iot-simulator
```

## Variaveis uteis

| Variavel | Padrao | Descricao |
| --- | --- | --- |
| `MQTT_HOST` | `mqtt-broker` | Host do broker MQTT |
| `MQTT_PORT` | `1883` | Porta do broker MQTT |
| `DEVICE_ID` | `aviario-01` | Identificador do equipamento simulado |
| `PUBLISH_INTERVAL_SECONDS` | `5` | Intervalo entre publicacoes |
| `FIRMWARE_VERSION` | `granjatech-fw-1.0.0` | Versao exibida nos logs da API |

As portas expostas no host tambem podem ser ajustadas sem editar o compose:

| Variavel | Padrao |
| --- | --- |
| `POSTGRES_HOST_PORT` | `5432` |
| `BACKEND_HOST_PORT` | `5099` |
| `FRONTEND_HOST_PORT` | `3000` |
| `MQTT_HOST_PORT` | `1883` |
