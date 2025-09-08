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

        // ===================== MORTALIDADE =====================

        /// <summary>Registra mortalidade no lote.</summary>
        [HttpPost("{id}/mortalidades")]
        [Authorize(Roles = "Administrador,Produtor")]
        public async Task<ActionResult<RegistroMortalidadeDto>> RegistrarMortalidade(int id, [FromBody] CreateRegistroMortalidadeDto dto)
        {
            var registro = await _loteService.RegistrarMortalidadeAsync(dto, id);

            var resp = new RegistroMortalidadeDto
            {
                Id = registro.Id,
                LoteId = registro.LoteId,
                Data = registro.Data,
                Quantidade = registro.Quantidade,
                Motivo = registro.Motivo,
                Setor = registro.Setor,
                Observacoes = registro.Observacoes,
                ResponsavelRegistro = registro.ResponsavelRegistro,
                PercentualMortalidadeDia = registro.PercentualMortalidadeDia,
                IdadeDias = registro.IdadeDias
            };

            return CreatedAtAction(nameof(ListarMortalidades), new { id }, resp);
        }

        /// <summary>Lista mortalidades do lote.</summary>
        [HttpGet("{id}/mortalidades")]
        [Authorize(Roles = "Administrador,Produtor")]
        public async Task<ActionResult<IEnumerable<RegistroMortalidadeDto>>> ListarMortalidades(int id)
        {
            var itens = await _loteService.ListarMortalidadesAsync(id);

            var resp = new List<RegistroMortalidadeDto>();
            foreach (var r in itens)
            {
                resp.Add(new RegistroMortalidadeDto
                {
                    Id = r.Id,
                    LoteId = r.LoteId,
                    Data = r.Data,
                    Quantidade = r.Quantidade,
                    Motivo = r.Motivo,
                    Setor = r.Setor,
                    Observacoes = r.Observacoes,
                    ResponsavelRegistro = r.ResponsavelRegistro,
                    PercentualMortalidadeDia = r.PercentualMortalidadeDia,
                    IdadeDias = r.IdadeDias
                });
            }

            return Ok(resp);
        }
    }
}
