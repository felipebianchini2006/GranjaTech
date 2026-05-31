using GranjaTech.Api.IoT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GranjaTech.Api.Controllers
{
    [ApiController]
    [Route("api/iot")]
    public class IotController : ControllerBase
    {
        private readonly IOptions<MqttSensorOptions> _options;
        private readonly IotIngestionState _state;

        public IotController(IOptions<MqttSensorOptions> options, IotIngestionState state)
        {
            _options = options;
            _state = state;
        }

        [AllowAnonymous]
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var options = _options.Value;
            var snapshot = _state.Snapshot();

            return Ok(new
            {
                mqtt = new
                {
                    options.Enabled,
                    options.Host,
                    options.Port,
                    options.TelemetryTopic,
                    options.AutoProvision
                },
                simulator = new
                {
                    options.DefaultDeviceId,
                    ExpectedSensorIdentifiers = new[]
                    {
                        $"{options.DefaultDeviceId}-temperature",
                        $"{options.DefaultDeviceId}-humidity",
                        $"{options.DefaultDeviceId}-luminosity"
                    }
                },
                ingestion = snapshot
            });
        }
    }
}
