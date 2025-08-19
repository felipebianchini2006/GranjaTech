using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [Authorize] // Protege o controller inteiro
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
        public async Task<IActionResult> PostTransacao(TransacaoFinanceira transacao)
        {
            await _financasService.AddAsync(transacao);
            return Ok(new { message = "Transação registrada com sucesso." });
        }

        // NOVO ENDPOINT DE EXCLUSÃO
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