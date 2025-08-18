using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    // Protege o controller inteiro para os perfis especificados
    [Authorize(Roles = "Administrador,Financeiro")]
    [ApiController]
    [Route("api/[controller]")]
    public class FinancasController : ControllerBase
    {
        private readonly IFinancasService _financasService;

        public FinancasController(IFinancasService financasService)
        {
            _financasService = financasService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransacoes()
        {
            var transacoes = await _financasService.GetAllAsync();
            return Ok(transacoes);
        }

        [HttpPost]
        public async Task<IActionResult> PostTransacao(TransacaoFinanceira transacao)
        {
            await _financasService.AddAsync(transacao);
            return Ok(new { message = "Transação registrada com sucesso." });
        }
    }
}