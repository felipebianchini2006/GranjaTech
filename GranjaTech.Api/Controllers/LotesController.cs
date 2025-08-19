using GranjaTech.Application.DTOs;
using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LotesController : ControllerBase
    {
        private readonly ILoteService _loteService;

        public LotesController(ILoteService loteService)
        {
            _loteService = loteService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lote>>> GetLotes()
        {
            var lotes = await _loteService.GetAllAsync();
            return Ok(lotes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Lote>> GetLote(int id)
        {
            var lote = await _loteService.GetByIdAsync(id);
            if (lote == null)
            {
                return NotFound();
            }
            return Ok(lote);
        }

        [HttpPost]
        public async Task<IActionResult> PostLote(CreateLoteDto loteDto)
        {
            await _loteService.AddAsync(loteDto);
            return Ok(new { message = "Lote criado com sucesso" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLote(int id, UpdateLoteDto loteDto)
        {
            var sucesso = await _loteService.UpdateAsync(id, loteDto);
            if (!sucesso)
            {
                return NotFound("Lote não encontrado ou permissão negada.");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLote(int id)
        {
            await _loteService.DeleteAsync(id);
            return NoContent();
        }
    }
}