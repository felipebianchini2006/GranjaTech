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

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new { status = "ok", service = "relatorios", timestamp = DateTime.UtcNow });
        }

        [HttpGet("debug/memory")]
        [AllowAnonymous]
        public IActionResult MemoryInfo()
        {
            var gcMemory = GC.GetTotalMemory(false);
            var process = System.Diagnostics.Process.GetCurrentProcess();
            return Ok(new { 
                gcMemoryBytes = gcMemory,
                gcMemoryMB = gcMemory / 1024 / 1024,
                workingSetMB = process.WorkingSet64 / 1024 / 1024,
                timestamp = DateTime.UtcNow 
            });
        }

        [HttpGet("debug/test-basic")]
        [AllowAnonymous]
        public async Task<IActionResult> TestBasic()
        {
            try
            {
                _logger.LogInformation("Teste básico iniciado");
                
                // Usar datas fixas dos últimos 7 dias
                var dataFim = DateTime.Today;
                var dataInicio = dataFim.AddDays(-7);
                
                _logger.LogInformation("Testando com datas: {DataInicio} a {DataFim}", dataInicio, dataFim);
                
                var result = await _relatorioService.GetRelatorioFinanceiroSimplificadoAsync(dataInicio, dataFim, null);
                
                _logger.LogInformation("Teste básico concluído com sucesso");
                return Ok(new { 
                    message = "Teste básico concluído com sucesso",
                    periodo = $"{dataInicio:yyyy-MM-dd} a {dataFim:yyyy-MM-dd}",
                    transacoesCount = result.Transacoes.Count,
                    totalEntradas = result.TotalEntradas,
                    totalSaidas = result.TotalSaidas,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no teste básico");
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("debug/simple")]
        [AllowAnonymous]
        public async Task<IActionResult> SimpleTest(
            [FromQuery] string dataInicio,
            [FromQuery] string dataFim)
        {
            try
            {
                _logger.LogInformation("Teste simples iniciado para período {DataInicio} a {DataFim}", dataInicio, dataFim);
                
                // Validar e converter datas
                if (!DateTime.TryParse(dataInicio, out DateTime dataInicioDate))
                {
                    return BadRequest(new { message = "Data de início inválida. Use formato: 2025-01-01" });
                }
                
                if (!DateTime.TryParse(dataFim, out DateTime dataFimDate))
                {
                    return BadRequest(new { message = "Data de fim inválida. Use formato: 2025-01-01" });
                }
                
                // Query simples sem navegações
                var result = await _relatorioService.GetRelatorioFinanceiroSimplificadoAsync(dataInicioDate, dataFimDate, null);
                
                _logger.LogInformation("Teste simples concluído");
                return Ok(new { 
                    message = "Teste concluído com sucesso",
                    timestamp = DateTime.UtcNow,
                    transacoesCount = result.Transacoes.Count,
                    totalEntradas = result.TotalEntradas,
                    totalSaidas = result.TotalSaidas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no teste simples");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("financeiro-simplificado")]
        public async Task<IActionResult> GetRelatorioFinanceiroSimplificado(
            [FromQuery, Required] DateTime dataInicio,
            [FromQuery, Required] DateTime dataFim,
            [FromQuery] int? granjaId)
        {
            try
            {
                var dataInicioUtc = DateTime.SpecifyKind(dataInicio, DateTimeKind.Utc);
                var dataFimUtc = DateTime.SpecifyKind(dataFim, DateTimeKind.Utc);

                if (dataInicioUtc > dataFimUtc)
                {
                    return BadRequest(new { message = "A data de início não pode ser posterior à data de fim." });
                }

                if ((dataFimUtc - dataInicioUtc).TotalDays > 365)
                {
                    return BadRequest(new { message = "O período do relatório não pode exceder 365 dias." });
                }

                var relatorio = await _relatorioService.GetRelatorioFinanceiroSimplificadoAsync(dataInicioUtc, dataFimUtc, granjaId);
                _logger.LogInformation("Retornando relatório financeiro SIMPLIFICADO com {TransacoesCount} transações", relatorio.Transacoes.Count);
                return Ok(relatorio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao gerar relatório financeiro simplificado");
                return StatusCode(500, new { message = $"Erro interno do servidor: {ex.Message}" });
            }
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

                // Validação adicional no servidor para evitar períodos muito grandes (proteção de performance)
                if ((dataFimUtc - dataInicioUtc).TotalDays > 365)
                {
                    return BadRequest(new { message = "O período do relatório não pode exceder 365 dias." });
                }

                var relatorio = await _relatorioService.GetRelatorioFinanceiroAsync(dataInicioUtc, dataFimUtc, granjaId);
                var transacoesCount = relatorio?.Transacoes?.Count() ?? 0;
                _logger.LogInformation("Retornando relatório financeiro com {TransacoesCount} transações", transacoesCount);
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

                // Validação adicional no servidor para evitar períodos muito grandes (proteção de performance)
                if ((dataFimUtc - dataInicioUtc).TotalDays > 365)
                {
                    return BadRequest(new { message = "O período do relatório não pode exceder 365 dias." });
                }

                var relatorio = await _relatorioService.GetRelatorioProducaoAsync(dataInicioUtc, dataFimUtc, granjaId);
                var lotesCount = relatorio?.Lotes?.Count() ?? 0;
                _logger.LogInformation("Retornando relatório de produção com {LotesCount} lotes", lotesCount);
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
