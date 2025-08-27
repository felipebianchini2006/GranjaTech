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

        /// <summary>
        /// Relatório completo de avicultura com métricas específicas
        /// </summary>
        [HttpGet("avicultura")]
        public async Task<IActionResult> GetRelatorioAvicultura(
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim,
            [FromQuery] int? loteId = null)
        {
            try
            {
                var dataInicioUtc = dataInicio?.ToUniversalTime() ?? DateTime.UtcNow.AddMonths(-1);
                var dataFimUtc = dataFim?.ToUniversalTime() ?? DateTime.UtcNow;

                if (dataInicioUtc > dataFimUtc)
                {
                    return BadRequest(new { message = "A data de início não pode ser posterior à data de fim." });
                }

                var query = _context.Lotes
                    .Include(l => l.Granja)
                    .Include(l => l.ConsumosRacao)
                    .Include(l => l.ConsumosAgua)
                    .Include(l => l.PesagensSemanais)
                    .Include(l => l.EventosSanitarios)
                    .Include(l => l.RegistrosMortalidade)
                    .Include(l => l.MedicoesQualidadeAr)
                    .Where(l => l.DataEntrada >= dataInicioUtc && l.DataEntrada <= dataFimUtc);

                if (loteId.HasValue)
                    query = query.Where(l => l.Id == loteId.Value);

                var lotes = await query.ToListAsync();

                var relatorio = new
                {
                    PeriodoInicio = dataInicioUtc,
                    PeriodoFim = dataFimUtc,
                    TotalLotes = lotes.Count,
                    DataGeracao = DateTime.UtcNow,
                    
                    ResumoGeral = new
                    {
                        TotalAvesAlojadas = lotes.Sum(l => l.QuantidadeAvesInicial),
                        TotalAvesAtuais = lotes.Sum(l => l.QuantidadeAvesAtual),
                        MortalidadeMedia = lotes.Any() ? lotes.Average(l => l.PercentualMortalidadeAcumulada) : 0,
                        ViabilidadeMedia = lotes.Any() ? lotes.Average(l => l.Viabilidade) : 0,
                        
                        ConsumoTotalRacao = lotes.SelectMany(l => l.ConsumosRacao).Sum(c => c.QuantidadeKg),
                        ConsumoTotalAgua = lotes.SelectMany(l => l.ConsumosAgua).Sum(c => c.QuantidadeLitros),
                        
                        TotalEventosSanitarios = lotes.SelectMany(l => l.EventosSanitarios).Count(),
                        CustoTotalSanitario = lotes.SelectMany(l => l.EventosSanitarios).Sum(e => e.Custo ?? 0)
                    },
                    
                    DetalhesPorLote = lotes.Select(lote => new
                    {
                        LoteId = lote.Id,
                        Identificador = lote.Identificador,
                        Granja = lote.Granja.Nome,
                        DataEntrada = lote.DataEntrada,
                        IdadeAtualDias = lote.IdadeAtualDias,
                        Status = lote.Status,
                        
                        // Dados das aves
                        QuantidadeInicial = lote.QuantidadeAvesInicial,
                        QuantidadeAtual = lote.QuantidadeAvesAtual,
                        MortalidadePercentual = lote.PercentualMortalidadeAcumulada,
                        Viabilidade = lote.Viabilidade,
                        DensidadeAtual = lote.DensidadeAtual,
                        
                        // Crescimento
                        PesagemMaisRecente = lote.PesagensSemanais.OrderByDescending(p => p.DataPesagem).FirstOrDefault(),
                        GanhoMedioDiario = lote.PesagensSemanais.Any() ? 
                            lote.PesagensSemanais.Average(p => p.GanhoMedioDiario) : 0,
                        
                        // Consumo
                        ConsumoRacao = new
                        {
                            TotalKg = lote.ConsumosRacao.Sum(c => c.QuantidadeKg),
                            MediaPorAve = lote.ConsumosRacao.Any() ? 
                                lote.ConsumosRacao.Average(c => c.ConsumoPorAveGramas) : 0,
                            UltimoRegistro = lote.ConsumosRacao.OrderByDescending(c => c.Data).FirstOrDefault()?.Data
                        },
                        
                        ConsumoAgua = new
                        {
                            TotalLitros = lote.ConsumosAgua.Sum(c => c.QuantidadeLitros),
                            MediaPorAve = lote.ConsumosAgua.Any() ? 
                                lote.ConsumosAgua.Average(c => c.ConsumoPorAveMl) : 0,
                            UltimoRegistro = lote.ConsumosAgua.OrderByDescending(c => c.Data).FirstOrDefault()?.Data
                        },
                        
                        RelacaoAguaRacao = lote.ConsumosRacao.Sum(c => c.QuantidadeKg) > 0 ?
                            lote.ConsumosAgua.Sum(c => c.QuantidadeLitros) / lote.ConsumosRacao.Sum(c => c.QuantidadeKg) : 0,
                        
                        // Métricas calculadas
                        ConversaoAlimentar = lote.CalcularConversaoAlimentar(),
                        IEP = lote.CalcularIEP(),
                        
                        // Sanitário
                        EventosSanitarios = new
                        {
                            Total = lote.EventosSanitarios.Count,
                            Vacinacoes = lote.EventosSanitarios.Count(e => e.TipoEvento == "Vacinacao"),
                            Medicacoes = lote.EventosSanitarios.Count(e => e.TipoEvento == "Medicacao"),
                            CustoTotal = lote.EventosSanitarios.Sum(e => e.Custo ?? 0)
                        },
                        
                        // Qualidade do ar - últimas medições
                        QualidadeAr = lote.MedicoesQualidadeAr
                            .OrderByDescending(q => q.DataHora)
                            .Take(1)
                            .Select(q => new
                            {
                                q.DataHora,
                                q.TemperaturaAr,
                                q.UmidadeRelativa,
                                q.NH3_ppm,
                                q.CO2_ppm,
                                q.ParametrosAceitaveis
                            })
                            .FirstOrDefault()
                    }).ToList(),
                    
                    // Análises comparativas
                    Benchmarks = new
                    {
                        MelhorConversaoAlimentar = lotes.Any() ? lotes.Min(l => l.CalcularConversaoAlimentar()) : 0,
                        MelhorIEP = lotes.Any() ? lotes.Max(l => l.CalcularIEP()) : 0,
                        MelhorViabilidade = lotes.Any() ? lotes.Max(l => l.Viabilidade) : 0,
                        MenorMortalidade = lotes.Any() ? lotes.Min(l => l.PercentualMortalidadeAcumulada) : 0
                    }
                };

                _logger.LogInformation("Relatório de avicultura gerado com {TotalLotes} lotes", lotes.Count);
                return Ok(relatorio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório de avicultura");
                return StatusCode(500, new { message = $"Erro interno do servidor: {ex.Message}" });
            }
        }

        /// <summary>
        /// Relatório de desempenho por lote específico
        /// </summary>
        [HttpGet("desempenho-lote/{loteId}")]
        public async Task<IActionResult> GetRelatorioDesempenhoLote(int loteId)
        {
            try
            {
                var lote = await _context.Lotes
                    .Include(l => l.Granja)
                    .Include(l => l.ConsumosRacao)
                    .Include(l => l.ConsumosAgua)
                    .Include(l => l.PesagensSemanais)
                    .Include(l => l.EventosSanitarios)
                    .Include(l => l.RegistrosMortalidade)
                    .Include(l => l.MedicoesQualidadeAr)
                    .FirstOrDefaultAsync(l => l.Id == loteId);

                if (lote == null)
                {
                    return NotFound(new { message = "Lote não encontrado" });
                }

                var relatorio = new
                {
                    // Dados básicos
                    LoteId = lote.Id,
                    Identificador = lote.Identificador,
                    Granja = lote.Granja.Nome,
                    DataEntrada = lote.DataEntrada,
                    IdadeAtualDias = lote.IdadeAtualDias,
                    Status = lote.Status,
                    Linhagem = lote.Linhagem,
                    OrigemPintinhos = lote.OrigemPintinhos,
                    
                    // Performance
                    Performance = new
                    {
                        QuantidadeInicial = lote.QuantidadeAvesInicial,
                        QuantidadeAtual = lote.QuantidadeAvesAtual,
                        MortalidadeTotal = lote.MortalidadeTotalAcumulada,
                        MortalidadePercentual = lote.PercentualMortalidadeAcumulada,
                        Viabilidade = lote.Viabilidade,
                        DensidadeAtual = lote.DensidadeAtual,
                        ConversaoAlimentar = lote.CalcularConversaoAlimentar(),
                        IEP = lote.CalcularIEP()
                    },
                    
                    // Curva de crescimento
                    CurvaCrescimento = lote.PesagensSemanais
                        .OrderBy(p => p.SemanaVida)
                        .Select(p => new
                        {
                            Semana = p.SemanaVida,
                            IdadeDias = p.IdadeDias,
                            PesoMedio = p.PesoMedioGramas,
                            GanhoSemanal = p.GanhoSemanal,
                            GanhoMedioDiario = p.GanhoMedioDiario,
                            Uniformidade = p.CoeficienteVariacao.HasValue ? 100 - p.CoeficienteVariacao.Value : 0,
                            QuantidadeAmostrada = p.QuantidadeAmostrada
                        }).ToList(),
                    
                    // Consumo detalhado
                    ConsumoRacao = lote.ConsumosRacao
                        .OrderBy(c => c.Data)
                        .GroupBy(c => c.TipoRacao)
                        .Select(g => new
                        {
                            TipoRacao = g.Key,
                            TotalKg = g.Sum(c => c.QuantidadeKg),
                            MediaPorAve = g.Average(c => c.ConsumoPorAveGramas),
                            RegistrosPorDia = g.Select(c => new
                            {
                                Data = c.Data,
                                QuantidadeKg = c.QuantidadeKg,
                                AvesVivas = c.AvesVivas,
                                ConsumoPorAve = c.ConsumoPorAveGramas
                            }).ToList()
                        }).ToList(),
                    
                    ConsumoAgua = lote.ConsumosAgua
                        .OrderBy(c => c.Data)
                        .Select(c => new
                        {
                            Data = c.Data,
                            QuantidadeLitros = c.QuantidadeLitros,
                            AvesVivas = c.AvesVivas,
                            ConsumoPorAve = c.ConsumoPorAveMl,
                            TemperaturaAmbiente = c.TemperaturaAmbiente
                        }).ToList(),
                    
                    // Histórico sanitário
                    HistoricoSanitario = lote.EventosSanitarios
                        .OrderBy(e => e.Data)
                        .Select(e => new
                        {
                            Data = e.Data,
                            TipoEvento = e.TipoEvento,
                            Produto = e.Produto,
                            ViaAdministracao = e.ViaAdministracao,
                            AvesTratadas = e.AvesTratadas,
                            Custo = e.Custo,
                            PeriodoCarencia = e.PeriodoCarenciaDias,
                            Responsavel = e.ResponsavelAplicacao
                        }).ToList(),
                    
                    // Análise de mortalidade
                    AnaliseMortalidade = lote.RegistrosMortalidade
                        .OrderBy(m => m.Data)
                        .Select(m => new
                        {
                            Data = m.Data,
                            IdadeDias = m.IdadeDias,
                            QuantidadeMortas = m.QuantidadeMortas,
                            PercentualDia = m.PercentualMortalidadeDia,
                            CausaPrincipal = m.CausaPrincipal,
                            AvesVivas = m.AvesVivas
                        }).ToList(),
                    
                    // Qualidade ambiental
                    QualidadeAmbiental = lote.MedicoesQualidadeAr
                        .OrderBy(q => q.DataHora)
                        .Select(q => new
                        {
                            DataHora = q.DataHora,
                            TemperaturaAr = q.TemperaturaAr,
                            UmidadeRelativa = q.UmidadeRelativa,
                            NH3_ppm = q.NH3_ppm,
                            CO2_ppm = q.CO2_ppm,
                            O2_percentual = q.O2_percentual,
                            VelocidadeAr = q.VelocidadeAr_ms,
                            Luminosidade = q.Luminosidade_lux,
                            ParametrosOK = q.ParametrosAceitaveis
                        }).ToList(),
                    
                    DataGeracao = DateTime.UtcNow
                };

                return Ok(relatorio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório de desempenho do lote {LoteId}", loteId);
                return StatusCode(500, new { message = $"Erro interno do servidor: {ex.Message}" });
            }
        }
    }
}
