using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
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
        public async Task<ActionResult<Lote>> PostLote(Lote lote)
        {
            await _loteService.AddAsync(lote);
            return CreatedAtAction(nameof(GetLote), new { id = lote.Id }, lote);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLote(int id, Lote lote)
        {
            if (id != lote.Id)
            {
                return BadRequest();
            }

            var loteExistente = await _loteService.GetByIdAsync(id);
            if (loteExistente == null)
            {
                return NotFound();
            }

            await _loteService.UpdateAsync(lote);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLote(int id)
        {
            var loteExistente = await _loteService.GetByIdAsync(id);
            if (loteExistente == null)
            {
                return NotFound();
            }

            await _loteService.DeleteAsync(id);
            return NoContent();
        }
    }
}