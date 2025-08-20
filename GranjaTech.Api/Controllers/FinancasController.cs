using GranjaTech.Application.DTOs;
using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FinancasController : ControllerBase
    {
        private readonly IFinancasService _financasService;

        public FinancasController(IFinancasService financasService)
        {
            _financasService = financasService;
        }

        [Authorize(Roles = "Administrador,Financeiro")]
        [HttpGet]
        public async Task<IActionResult> GetTransacoes()
        {
            var transacoes = await _financasService.GetAllAsync();
            return Ok(transacoes);
        }

        [Authorize(Roles = "Administrador,Financeiro")]
        [HttpPost]
        public async Task<IActionResult> PostTransacao(CreateTransacaoDto transacaoDto) // Parâmetro alterado
        {
            await _financasService.AddAsync(transacaoDto);
            return Ok(new { message = "Transação registada com sucesso." });
        }

        [Authorize(Roles = "Administrador,Financeiro")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransacao(int id, UpdateTransacaoDto transacaoDto)
        {
            try
            {
                var sucesso = await _financasService.UpdateAsync(id, transacaoDto);
                if (!sucesso) return NotFound("Transação não encontrada.");
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransacao(int id)
        {
            var sucesso = await _financasService.DeleteAsync(id);
            if (!sucesso)
            {
                return NotFound("Transação não encontrada.");
            }
            return NoContent();
        }
    }
}
