using GranjaTech.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [Authorize(Roles = "Administrador,Financeiro,Produtor")]
    [ApiController]
    [Route("api/[controller]")]
    public class RelatoriosController : ControllerBase
    {
        private readonly IRelatorioService _relatorioService;

        public RelatoriosController(IRelatorioService relatorioService)
        {
            _relatorioService = relatorioService;
        }

        [HttpGet("financeiro")]
        public async Task<IActionResult> GetRelatorioFinanceiro([FromQuery] DateTime dataInicio, [FromQuery] DateTime dataFim, [FromQuery] int? granjaId)
        {
            try
            {
                var relatorio = await _relatorioService.GetRelatorioFinanceiroAsync(dataInicio, dataFim, granjaId);
                return Ok(relatorio);
            }
            catch (Exception ex)
            {
                // Se ocorrer um erro inesperado, retorna 500 com a mensagem do erro
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpGet("producao")]
        public async Task<IActionResult> GetRelatorioProducao([FromQuery] DateTime dataInicio, [FromQuery] DateTime dataFim, [FromQuery] int? granjaId)
        {
            try
            {
                var relatorio = await _relatorioService.GetRelatorioProducaoAsync(dataInicio, dataFim, granjaId);
                return Ok(relatorio);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }
    }
}
