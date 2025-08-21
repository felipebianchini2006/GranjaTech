using GranjaTech.Application.DTOs;
using GranjaTech.Domain;
using GranjaTech.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeiturasController : ControllerBase
    {
        private readonly GranjaTechDbContext _context;

        public LeiturasController(GranjaTechDbContext context)
        {
            _context = context;
        }

        // Este endpoint é público para que os dispositivos IoT possam enviar dados.
        // Numa aplicação real, ele seria protegido por uma chave de API (API Key).
        [HttpPost]
        public async Task<IActionResult> PostLeitura(CreateLeituraDto leituraDto)
        {
            var sensor = await _context.Sensores
                .FirstOrDefaultAsync(s => s.IdentificadorUnico == leituraDto.IdentificadorUnico);

            if (sensor == null)
            {
                return NotFound(new { message = "Sensor não encontrado." });
            }

            var novaLeitura = new LeituraSensor
            {
                SensorId = sensor.Id,
                Valor = leituraDto.Valor,
                Timestamp = DateTime.UtcNow
            };

            await _context.LeiturasSensores.AddAsync(novaLeitura);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Leitura registada com sucesso." });
        }
    }
}
