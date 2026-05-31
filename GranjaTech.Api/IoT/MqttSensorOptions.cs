namespace GranjaTech.Api.IoT
{
    public sealed class MqttSensorOptions
    {
        public const string SectionName = "Mqtt";

        public bool Enabled { get; set; }
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 1883;
        public string ClientId { get; set; } = "granjatech-api";
        public string TelemetryTopic { get; set; } = "granjatech/iot/+/telemetry";
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool AutoProvision { get; set; } = true;
        public int DefaultOwnerUserId { get; set; } = 1;
        public string DefaultFarmCode { get; set; } = "GRJ-IOT";
        public string DefaultFarmName { get; set; } = "Granja IoT Simulada";
        public string DefaultFarmLocation { get; set; } = "Docker MQTT Simulator";
        public string DefaultDeviceId { get; set; } = "aviario-01";
    }
}
