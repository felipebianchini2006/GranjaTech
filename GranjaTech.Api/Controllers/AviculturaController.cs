using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Application.DTOs;
using System.Threading.Tasks;
using System;

namespace GranjaTech.Api.Controllers
{
    [Authorize(Roles = "Administrador,Produtor")]
    [ApiController]
    [Route("api/[controller]")]
    public class AviculturaController : ControllerBase
    {
        private readonly IAviculturaService _aviculturaService;

        public AviculturaController(IAviculturaService aviculturaService)
        {
            _aviculturaService = aviculturaService;
        }

        /// <summary>
        /// Obtém métricas principais de um lote
        /// </summary>
        [HttpGet("{loteId}/metricas")]
        public async Task<IActionResult> GetMetricasLote(int loteId)
        {
            try
            {
                var metricas = new
                {
                    IEP = await _aviculturaService.CalcularIEPAsync(loteId),
                    ConversaoAlimentar = await _aviculturaService.CalcularConversaoAlimentarAsync(loteId),
                    GanhoMedioDiario = await _aviculturaService.CalcularGanhoMedioDiarioAsync(loteId),
                    Viabilidade = await _aviculturaService.CalcularViabilidadeAsync(loteId),
                    Uniformidade = await _aviculturaService.CalcularUniformidadeAsync(loteId),
                    DensidadeAtual = await _aviculturaService.CalcularDensidadeAtualAsync(loteId)
                };

                return Ok(metricas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao calcular métricas", error = ex.Message });
            }
        }

        /// <summary>
        /// Análise detalhada de consumo de um lote
        /// </summary>
        [HttpGet("{loteId}/analise-consumo")]
        public async Task<IActionResult> GetAnaliseConsumo(int loteId)
        {
            try
            {
                var analise = await _aviculturaService.AnaliseConsumoDetalhada(loteId);
                return Ok(analise);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao analisar consumo", error = ex.Message });
            }
        }

        /// <summary>
        /// Curvas de crescimento e consumo do lote
        /// </summary>
        [HttpGet("{loteId}/curvas-crescimento")]
        public async Task<IActionResult> GetCurvasCrescimento(int loteId)
        {
            try
            {
                var curvas = await _aviculturaService.ObterCurvasCrescimentoAsync(loteId);
                return Ok(curvas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao obter curvas de crescimento", error = ex.Message });
            }
        }

        /// <summary>
        /// Resumo sanitário do lote
        /// </summary>
        [HttpGet("{loteId}/resumo-sanitario")]
        public async Task<IActionResult> GetResumoSanitario(int loteId)
        {
            try
            {
                var resumo = await _aviculturaService.ObterResumoSanitarioAsync(loteId);
                return Ok(resumo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao obter resumo sanitário", error = ex.Message });
            }
        }

        /// <summary>
        /// Alertas de parâmetros fora do padrão
        /// </summary>
        [HttpGet("{loteId}/alertas")]
        public async Task<IActionResult> GetAlertas(int loteId)
        {
            try
            {
                var alertas = await _aviculturaService.VerificarParametrosForaPadraoAsync(loteId);
                return Ok(alertas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao verificar alertas", error = ex.Message });
            }
        }

        /// <summary>
        /// Comparação com padrões da indústria
        /// </summary>
        [HttpGet("{loteId}/comparacao-industria")]
        public async Task<IActionResult> GetComparacaoIndustria(int loteId)
        {
            try
            {
                var comparacao = await _aviculturaService.CompararComPadroesIndustriaAsync(loteId);
                return Ok(comparacao);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao comparar com indústria", error = ex.Message });
            }
        }

        /// <summary>
        /// Projeção para abate
        /// </summary>
        [HttpGet("{loteId}/projecao-abate")]
        public async Task<IActionResult> GetProjecaoAbate(int loteId)
        {
            try
            {
                var projecao = await _aviculturaService.CalcularProjecaoAbateAsync(loteId);
                return Ok(projecao);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao calcular projeção de abate", error = ex.Message });
            }
        }

        /// <summary>
        /// Estimativa de peso médio para uma data específica
        /// </summary>
        [HttpGet("{loteId}/estimar-peso")]
        public async Task<IActionResult> EstimarPeso(int loteId, [FromQuery] DateTime dataAbate)
        {
            try
            {
                var pesoEstimado = await _aviculturaService.EstimarPesoMedioAbateAsync(loteId, dataAbate);
                return Ok(new { PesoEstimadoGramas = pesoEstimado, DataAbate = dataAbate });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao estimar peso", error = ex.Message });
            }
        }

        /// <summary>
        /// Dashboard completo de avicultura para um lote
        /// </summary>
        [HttpGet("{loteId}/dashboard")]
        public async Task<IActionResult> GetDashboardCompleto(int loteId)
        {
            try
            {
                var dashboard = new
                {
                    Metricas = new
                    {
                        IEP = await _aviculturaService.CalcularIEPAsync(loteId),
                        ConversaoAlimentar = await _aviculturaService.CalcularConversaoAlimentarAsync(loteId),
                        GanhoMedioDiario = await _aviculturaService.CalcularGanhoMedioDiarioAsync(loteId),
                        Viabilidade = await _aviculturaService.CalcularViabilidadeAsync(loteId)
                    },
                    Alertas = await _aviculturaService.VerificarParametrosForaPadraoAsync(loteId),
                    ComparacaoIndustria = await _aviculturaService.CompararComPadroesIndustriaAsync(loteId),
                    ResumoSanitario = await _aviculturaService.ObterResumoSanitarioAsync(loteId),
                    ProjecaoAbate = await _aviculturaService.CalcularProjecaoAbateAsync(loteId)
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao gerar dashboard", error = ex.Message });
            }
        }
    }
}
