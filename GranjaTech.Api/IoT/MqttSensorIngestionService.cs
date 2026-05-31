using System.Text;
using System.Text.Json;
using GranjaTech.Domain;
using GranjaTech.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace GranjaTech.Api.IoT
{
    public sealed class MqttSensorIngestionService : BackgroundService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptions<MqttSensorOptions> _options;
        private readonly ILogger<MqttSensorIngestionService> _logger;
        private readonly IotIngestionState _state;
        private readonly SemaphoreSlim _provisionLock = new(1, 1);
        private IMqttClient? _client;

        public MqttSensorIngestionService(
            IServiceScopeFactory scopeFactory,
            IOptions<MqttSensorOptions> options,
            ILogger<MqttSensorIngestionService> logger,
            IotIngestionState state)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
            _state = state;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var options = _options.Value;
            if (!options.Enabled)
            {
                _logger.LogInformation("MQTT sensor ingestion is disabled.");
                return;
            }

            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
            _client.ApplicationMessageReceivedAsync += HandleMessageAsync;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!_client.IsConnected)
                    {
                        await ConnectAndSubscribeAsync(_client, options, stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _state.RegisterError(ex.Message);
                    _logger.LogWarning(ex, "Could not connect to MQTT broker {Host}:{Port}. Retrying...", options.Host, options.Port);
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_client?.IsConnected == true)
            {
                await _client.DisconnectAsync(cancellationToken: cancellationToken);
            }

            await base.StopAsync(cancellationToken);
        }

        private async Task ConnectAndSubscribeAsync(IMqttClient client, MqttSensorOptions options, CancellationToken cancellationToken)
        {
            var clientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(options.ClientId)
                .WithTcpServer(options.Host, options.Port)
                .WithCleanSession();

            if (!string.IsNullOrWhiteSpace(options.Username))
            {
                clientOptionsBuilder.WithCredentials(options.Username, options.Password);
            }

            await client.ConnectAsync(clientOptionsBuilder.Build(), cancellationToken);

            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(options.TelemetryTopic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await client.SubscribeAsync(topicFilter, cancellationToken);
            _logger.LogInformation("MQTT sensor ingestion connected to {Host}:{Port} and subscribed to {Topic}.", options.Host, options.Port, options.TelemetryTopic);
        }

        private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            var topic = args.ApplicationMessage.Topic ?? string.Empty;
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);

            try
            {
                var telemetry = JsonSerializer.Deserialize<IotTelemetryPayload>(payload, JsonOptions);
                if (telemetry == null || string.IsNullOrWhiteSpace(telemetry.DeviceId))
                {
                    _logger.LogWarning("Ignoring MQTT telemetry without deviceId. Topic: {Topic}", topic);
                    return;
                }

                var timestamp = telemetry.Timestamp?.UtcDateTime ?? DateTime.UtcNow;

                await using var scope = _scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<GranjaTechDbContext>();

                var readingsSaved = 0;
                readingsSaved += await SaveReadingAsync(db, telemetry.DeviceId, "Temperatura", "temperature", telemetry.TemperatureC, timestamp);
                readingsSaved += await SaveReadingAsync(db, telemetry.DeviceId, "Umidade", "humidity", telemetry.HumidityPercent, timestamp);
                readingsSaved += await SaveReadingAsync(db, telemetry.DeviceId, "Luminosidade", "luminosity", telemetry.LuminosityLux, timestamp);

                if (readingsSaved > 0)
                {
                    _state.RegisterMessage(telemetry.DeviceId, topic);
                    _logger.LogInformation(
                        "Stored {Count} MQTT sensor readings from {DeviceId} firmware {FirmwareVersion}.",
                        readingsSaved,
                        telemetry.DeviceId,
                        telemetry.FirmwareVersion ?? "unknown");
                }
            }
            catch (Exception ex)
            {
                _state.RegisterError(ex.Message);
                _logger.LogError(ex, "Could not process MQTT telemetry from topic {Topic}. Payload: {Payload}", topic, payload);
            }
        }

        private async Task<int> SaveReadingAsync(
            GranjaTechDbContext db,
            string deviceId,
            string sensorType,
            string sensorSuffix,
            decimal? value,
            DateTime timestamp)
        {
            if (!value.HasValue)
            {
                return 0;
            }

            var sensorIdentifier = $"{deviceId}-{sensorSuffix}";
            var sensor = await GetOrCreateSensorAsync(db, sensorIdentifier, sensorType);
            if (sensor == null)
            {
                return 0;
            }

            await db.LeiturasSensores.AddAsync(new LeituraSensor
            {
                SensorId = sensor.Id,
                Valor = decimal.Round(value.Value, 2),
                Timestamp = timestamp
            });

            await db.SaveChangesAsync();
            return 1;
        }

        private async Task<Sensor?> GetOrCreateSensorAsync(GranjaTechDbContext db, string identifier, string sensorType)
        {
            var sensor = await db.Sensores.FirstOrDefaultAsync(s => s.IdentificadorUnico == identifier);
            if (sensor != null)
            {
                return sensor;
            }

            var options = _options.Value;
            if (!options.AutoProvision)
            {
                _logger.LogWarning("Sensor {Identifier} does not exist and MQTT auto provisioning is disabled.", identifier);
                return null;
            }

            await _provisionLock.WaitAsync();
            try
            {
                sensor = await db.Sensores.FirstOrDefaultAsync(s => s.IdentificadorUnico == identifier);
                if (sensor != null)
                {
                    return sensor;
                }

                var granja = await GetOrCreateDefaultFarmAsync(db, options);
                if (granja == null)
                {
                    return null;
                }

                sensor = new Sensor
                {
                    Tipo = sensorType,
                    IdentificadorUnico = identifier,
                    GranjaId = granja.Id
                };

                await db.Sensores.AddAsync(sensor);
                await db.SaveChangesAsync();
                _logger.LogInformation("Auto provisioned MQTT sensor {Identifier} ({Type}) in farm {FarmId}.", identifier, sensorType, granja.Id);
                return sensor;
            }
            finally
            {
                _provisionLock.Release();
            }
        }

        private async Task<Granja?> GetOrCreateDefaultFarmAsync(GranjaTechDbContext db, MqttSensorOptions options)
        {
            var granja = await db.Granjas.FirstOrDefaultAsync(g => g.Codigo == options.DefaultFarmCode);
            if (granja != null)
            {
                return granja;
            }

            var ownerExists = await db.Usuarios.AnyAsync(u => u.Id == options.DefaultOwnerUserId);
            if (!ownerExists)
            {
                _logger.LogError("Cannot auto provision MQTT farm because user {UserId} does not exist.", options.DefaultOwnerUserId);
                return null;
            }

            granja = new Granja
            {
                Codigo = options.DefaultFarmCode,
                Nome = options.DefaultFarmName,
                Localizacao = options.DefaultFarmLocation,
                UsuarioId = options.DefaultOwnerUserId
            };

            await db.Granjas.AddAsync(granja);
            await db.SaveChangesAsync();
            _logger.LogInformation("Auto provisioned MQTT demo farm {FarmCode} for user {UserId}.", options.DefaultFarmCode, options.DefaultOwnerUserId);
            return granja;
        }

        private sealed class IotTelemetryPayload
        {
            public string? DeviceId { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
            public string? FirmwareVersion { get; set; }
            public decimal? TemperatureC { get; set; }
            public decimal? HumidityPercent { get; set; }
            public decimal? LuminosityLux { get; set; }
        }
    }
}
