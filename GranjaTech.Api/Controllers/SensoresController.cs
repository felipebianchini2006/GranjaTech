using GranjaTech.Application.DTOs;
using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [Authorize(Roles = "Administrador,Produtor")]
    [ApiController]
    [Route("api/[controller]")]
    public class SensoresController : ControllerBase
    {
        private readonly ISensorService _sensorService;

        public SensoresController(ISensorService sensorService)
        {
            _sensorService = sensorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSensores()
        {
            var sensores = await _sensorService.GetAllAsync();
            return Ok(sensores);
        }

        [HttpPost]
        public async Task<IActionResult> PostSensor(CreateSensorDto sensorDto)
        {
            try
            {
                // CORREÇÃO: Passamos o DTO diretamente para o serviço.
                // O serviço é que tem a responsabilidade de o converter para uma entidade.
                await _sensorService.AddAsync(sensorDto);
                return Ok(new { message = "Sensor adicionado com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSensor(int id)
        {
            var sucesso = await _sensorService.DeleteAsync(id);
            if (!sucesso) return NotFound();
            return NoContent();
        }

        [HttpGet("{sensorId}/leituras")]
        public async Task<IActionResult> GetLeituras(int sensorId)
        {
            var leituras = await _sensorService.GetLeiturasBySensorIdAsync(sensorId);
            return Ok(leituras);
        }
    }
}
