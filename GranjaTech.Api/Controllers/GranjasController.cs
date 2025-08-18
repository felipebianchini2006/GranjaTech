using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [Authorize] // <-- ESTA LINHA PROTEGE TODOS OS ENDPOINTS DESTE CONTROLLER
    [ApiController]
    [Route("api/[controller]")]
    public class GranjasController : ControllerBase
    {
        private readonly IGranjaService _granjaService;

        public GranjasController(IGranjaService granjaService)
        {
            _granjaService = granjaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Granja>>> GetGranjas()
        {
            var granjas = await _granjaService.GetAllAsync();
            return Ok(granjas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Granja>> GetGranja(int id)
        {
            var granja = await _granjaService.GetByIdAsync(id);

            if (granja == null)
            {
                return NotFound();
            }

            return Ok(granja);
        }

        [HttpPost]
        public async Task<ActionResult<Granja>> PostGranja(Granja granja)
        {
            await _granjaService.AddAsync(granja);
            return CreatedAtAction(nameof(GetGranja), new { id = granja.Id }, granja);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutGranja(int id, Granja granja)
        {
            if (id != granja.Id)
            {
                return BadRequest("O ID da rota não corresponde ao ID do objeto.");
            }

            var granjaExistente = await _granjaService.GetByIdAsync(id);
            if (granjaExistente == null)
            {
                return NotFound("Granja não encontrada.");
            }

            await _granjaService.UpdateAsync(granja);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGranja(int id)
        {
            var granjaExistente = await _granjaService.GetByIdAsync(id);
            if (granjaExistente == null)
            {
                return NotFound("Granja não encontrada.");
            }

            await _granjaService.DeleteAsync(id);
            return NoContent();
        }
    }
}