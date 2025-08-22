using GranjaTech.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [Authorize(Roles = "Administrador,Financeiro,Produtor")]
    [ApiController]
    [Route("api/[controller]")]
    public class RelatoriosController : ControllerBase
    {
        private readonly IRelatorioService _relatorioService;
        private readonly ILogger<RelatoriosController> _logger;

        public RelatoriosController(IRelatorioService relatorioService, ILogger<RelatoriosController> logger)
        {
            _relatorioService = relatorioService;
            _logger = logger;
        }

        [HttpGet("financeiro")]
        public async Task<IActionResult> GetRelatorioFinanceiro(
            [FromQuery, Required] DateTime dataInicio,
            [FromQuery, Required] DateTime dataFim,
            [FromQuery] int? granjaId)
        {
            try
            {
                // CORREÇÃO: Especificamos que as datas devem ser tratadas como UTC.
                var dataInicioUtc = DateTime.SpecifyKind(dataInicio, DateTimeKind.Utc);
                var dataFimUtc = DateTime.SpecifyKind(dataFim, DateTimeKind.Utc);

                if (dataInicioUtc > dataFimUtc)
                {
                    return BadRequest(new { message = "A data de início não pode ser posterior à data de fim." });
                }

                var relatorio = await _relatorioService.GetRelatorioFinanceiroAsync(dataInicioUtc, dataFimUtc, granjaId);
                return Ok(relatorio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao gerar relatório financeiro");
                return StatusCode(500, new { message = $"Erro interno do servidor: {ex.Message}" });
            }
        }

        [HttpGet("producao")]
        public async Task<IActionResult> GetRelatorioProducao(
            [FromQuery, Required] DateTime dataInicio,
            [FromQuery, Required] DateTime dataFim,
            [FromQuery] int? granjaId)
        {
            try
            {
                // CORREÇÃO: Especificamos que as datas devem ser tratadas como UTC.
                var dataInicioUtc = DateTime.SpecifyKind(dataInicio, DateTimeKind.Utc);
                var dataFimUtc = DateTime.SpecifyKind(dataFim, DateTimeKind.Utc);

                if (dataInicioUtc > dataFimUtc)
                {
                    return BadRequest(new { message = "A data de início não pode ser posterior à data de fim." });
                }

                var relatorio = await _relatorioService.GetRelatorioProducaoAsync(dataInicioUtc, dataFimUtc, granjaId);
                return Ok(relatorio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao gerar relatório de produção");
                return StatusCode(500, new { message = $"Erro interno do servidor: {ex.Message}" });
            }
        }
    }
}
