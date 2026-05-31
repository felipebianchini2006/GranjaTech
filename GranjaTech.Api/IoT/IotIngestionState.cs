namespace GranjaTech.Api.IoT
{
    public sealed class IotIngestionState
    {
        private readonly object _sync = new();
        private long _messagesReceived;
        private DateTimeOffset? _lastReceivedAt;
        private string? _lastDeviceId;
        private string? _lastTopic;
        private string? _lastError;

        public void RegisterMessage(string deviceId, string topic)
        {
            lock (_sync)
            {
                _messagesReceived++;
                _lastReceivedAt = DateTimeOffset.UtcNow;
                _lastDeviceId = deviceId;
                _lastTopic = topic;
                _lastError = null;
            }
        }

        public void RegisterError(string error)
        {
            lock (_sync)
            {
                _lastError = error;
            }
        }

        public IotIngestionSnapshot Snapshot()
        {
            lock (_sync)
            {
                return new IotIngestionSnapshot(
                    _messagesReceived,
                    _lastReceivedAt,
                    _lastDeviceId,
                    _lastTopic,
                    _lastError);
            }
        }
    }

    public sealed record IotIngestionSnapshot(
        long MessagesReceived,
        DateTimeOffset? LastReceivedAt,
        string? LastDeviceId,
        string? LastTopic,
        string? LastError);
}
