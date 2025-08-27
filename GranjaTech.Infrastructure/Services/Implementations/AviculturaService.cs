using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Application.DTOs;
using GranjaTech.Domain;
using GranjaTech.Infrastructure;
using GranjaTech.Infrastructure.Services.Interfaces;

namespace GranjaTech.Infrastructure.Services.Implementations
{
    public class AviculturaService : IAviculturaService
    {
        private readonly GranjaTechDbContext _context;
        private readonly ILogger<AviculturaService> _logger;
        private readonly ICacheService _cacheService;

        // Padrões da indústria avícola brasileira
        private static readonly Dictionary<string, decimal> PadroesIndustria = new()
        {
            ["ConversaoAlimentar"] = 1.75m,      // CA padrão da indústria
            ["ConversaoAlimentarExcelencia"] = 1.60m,
            ["GanhoMedioDiario"] = 55m,          // GMD em gramas
            ["GanhoMedioDiarioExcelencia"] = 60m,
            ["Viabilidade"] = 95m,               // % de viabilidade
            ["ViabilidadeExcelencia"] = 97m,
            ["IEP"] = 350m,                      // Índice de Eficiência Produtiva
            ["IEPExcelencia"] = 400m,
            ["ConsumoAguaPorAve"] = 200m,        // ml/ave/dia
            ["ConsumoRacaoPorAve"] = 100m,       // g/ave/dia
            ["RelacaoAguaRacao"] = 2.0m,         // Litros água : kg ração
            ["MortalidadeMaxima"] = 5m,          // % máxima aceitável
            ["DensidadeMaxima"] = 18m,           // aves/m²
            ["PesoAbate42Dias"] = 2400m,         // gramas aos 42 dias
            ["TemperaturaIdeal"] = 24m,          // °C
            ["UmidadeIdeal"] = 60m,              // %
            ["NH3Maximo"] = 25m,                 // ppm
            ["CO2Maximo"] = 3000m                // ppm
        };

        public AviculturaService(GranjaTechDbContext context, ILogger<AviculturaService> logger, ICacheService cacheService)
        {
            _context = context;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<decimal> CalcularIEPAsync(int loteId)
        {
            try
            {
                var lote = await _context.Lotes
                    .Include(l => l.PesagensSemanais)
                    .Include(l => l.ConsumosRacao)
                    .FirstOrDefaultAsync(l => l.Id == loteId);

                if (lote == null) return 0;

                var pesagemMaisRecente = lote.PesagensSemanais
                    .OrderByDescending(p => p.DataPesagem)
                    .FirstOrDefault();

                if (pesagemMaisRecente == null || lote.IdadeAtualDias == 0) return 0;

                var ganhoPesoKg = (pesagemMaisRecente.PesoMedioGramas - 45) / 1000m; // 45g peso inicial
                var viabilidade = lote.Viabilidade;
                var conversaoAlimentar = await CalcularConversaoAlimentarAsync(loteId);

                if (conversaoAlimentar == 0) return 0;

                var iep = (ganhoPesoKg * viabilidade * 100) / (conversaoAlimentar * lote.IdadeAtualDias);
                
                _logger.LogInformation("IEP calculado para lote {LoteId}: {IEP}", loteId, iep);
                return Math.Round(iep, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular IEP para lote {LoteId}", loteId);
                return 0;
            }
        }

        public async Task<decimal> CalcularConversaoAlimentarAsync(int loteId)
        {
            try
            {
                var lote = await _context.Lotes
                    .Include(l => l.ConsumosRacao)
                    .Include(l => l.PesagensSemanais)
                    .FirstOrDefaultAsync(l => l.Id == loteId);

                if (lote == null) return 0;

                var totalRacaoKg = lote.ConsumosRacao.Sum(c => c.QuantidadeKg);
                var pesagemMaisRecente = lote.PesagensSemanais
                    .OrderByDescending(p => p.DataPesagem)
                    .FirstOrDefault();

                if (pesagemMaisRecente == null || totalRacaoKg == 0) return 0;

                var ganhoTotalPorAveGramas = pesagemMaisRecente.PesoMedioGramas - 45; // 45g peso inicial
                var ganhoTotalLoteKg = (ganhoTotalPorAveGramas * lote.QuantidadeAvesAtual) / 1000;

                var conversaoAlimentar = ganhoTotalLoteKg > 0 ? totalRacaoKg / ganhoTotalLoteKg : 0;
                
                _logger.LogInformation("Conversão Alimentar calculada para lote {LoteId}: {CA}", loteId, conversaoAlimentar);
                return Math.Round(conversaoAlimentar, 3);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular conversão alimentar para lote {LoteId}", loteId);
                return 0;
            }
        }

        public async Task<decimal> CalcularGanhoMedioDiarioAsync(int loteId)
        {
            try
            {
                var pesagensSemanais = await _context.PesagensSemanais
                    .Where(p => p.LoteId == loteId)
                    .OrderBy(p => p.SemanaVida)
                    .ToListAsync();

                if (pesagensSemanais.Count < 2) return 0;

                var gmdTotal = 0m;
                var semanasComGanho = 0;

                for (int i = 1; i < pesagensSemanais.Count; i++)
                {
                    var pesagemAnterior = pesagensSemanais[i - 1];
                    var pesagemAtual = pesagensSemanais[i];
                    
                    var ganhoSemanal = pesagemAtual.PesoMedioGramas - pesagemAnterior.PesoMedioGramas;
                    var gmdSemanal = ganhoSemanal / 7; // Dividir por 7 dias
                    
                    gmdTotal += gmdSemanal;
                    semanasComGanho++;
                }

                var gmdMedio = semanasComGanho > 0 ? gmdTotal / semanasComGanho : 0;
                
                _logger.LogInformation("GMD calculado para lote {LoteId}: {GMD}g/dia", loteId, gmdMedio);
                return Math.Round(gmdMedio, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular GMD para lote {LoteId}", loteId);
                return 0;
            }
        }

        public async Task<List<AlertaParametroDto>> VerificarParametrosForaPadraoAsync(int loteId)
        {
            var alertas = new List<AlertaParametroDto>();

            try
            {
                var lote = await _context.Lotes
                    .Include(l => l.MedicoesQualidadeAr)
                    .Include(l => l.RegistrosMortalidade)
                    .FirstOrDefaultAsync(l => l.Id == loteId);

                if (lote == null) return alertas;

                // Verificar mortalidade
                if (lote.PercentualMortalidadeAcumulada > PadroesIndustria["MortalidadeMaxima"])
                {
                    alertas.Add(new AlertaParametroDto
                    {
                        TipoAlerta = "Mortalidade",
                        Severidade = lote.PercentualMortalidadeAcumulada > 8 ? "Critica" : "Alta",
                        Descricao = "Mortalidade acumulada acima do padrão da indústria",
                        ValorAtual = lote.PercentualMortalidadeAcumulada,
                        ValorMaximo = PadroesIndustria["MortalidadeMaxima"],
                        Unidade = "%",
                        DataOcorrencia = DateTime.Now,
                        Recomendacao = "Investigar causas e implementar medidas sanitárias"
                    });
                }

                // Verificar densidade
                if (lote.DensidadeAtual > PadroesIndustria["DensidadeMaxima"])
                {
                    alertas.Add(new AlertaParametroDto
                    {
                        TipoAlerta = "Densidade",
                        Severidade = "Media",
                        Descricao = "Densidade de aves acima do recomendado",
                        ValorAtual = lote.DensidadeAtual,
                        ValorMaximo = PadroesIndustria["DensidadeMaxima"],
                        Unidade = "aves/m²",
                        DataOcorrencia = DateTime.Now,
                        Recomendacao = "Considerar ajustar número de aves ou área disponível"
                    });
                }

                // Verificar qualidade do ar nas últimas 24h
                var medicoeesRecentes = lote.MedicoesQualidadeAr
                    .Where(m => m.DataHora >= DateTime.Now.AddDays(-1))
                    .OrderByDescending(m => m.DataHora)
                    .ToList();

                foreach (var medicao in medicoeesRecentes.Take(5)) // Últimas 5 medições
                {
                    if (medicao.NH3_ppm > PadroesIndustria["NH3Maximo"])
                    {
                        alertas.Add(new AlertaParametroDto
                        {
                            TipoAlerta = "Amônia",
                            Severidade = medicao.NH3_ppm > 35 ? "Critica" : "Alta",
                            Descricao = "Nível de amônia (NH3) acima do limite",
                            ValorAtual = medicao.NH3_ppm ?? 0,
                            ValorMaximo = PadroesIndustria["NH3Maximo"],
                            Unidade = "ppm",
                            DataOcorrencia = medicao.DataHora,
                            Recomendacao = "Aumentar ventilação e verificar sistema de exaustão"
                        });
                    }

                    if (medicao.TemperaturaAr < 18 || medicao.TemperaturaAr > 33)
                    {
                        alertas.Add(new AlertaParametroDto
                        {
                            TipoAlerta = "Temperatura",
                            Severidade = (medicao.TemperaturaAr < 15 || medicao.TemperaturaAr > 35) ? "Critica" : "Media",
                            Descricao = "Temperatura fora da faixa ideal",
                            ValorAtual = medicao.TemperaturaAr ?? 0,
                            ValorMinimo = 18,
                            ValorMaximo = 33,
                            Unidade = "°C",
                            DataOcorrencia = medicao.DataHora,
                            Recomendacao = "Ajustar sistema de aquecimento/resfriamento"
                        });
                    }
                }

                _logger.LogInformation("Verificados {Count} alertas para lote {LoteId}", alertas.Count, loteId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar parâmetros para lote {LoteId}", loteId);
            }

            return alertas;
        }

        public async Task<ComparacaoIndustriaDto> CompararComPadroesIndustriaAsync(int loteId)
        {
            // Cache key para comparação com indústria
            var cacheKey = $"comparacao_industria_{loteId}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var comparacao = new ComparacaoIndustriaDto { LoteId = loteId };

                try
                {
                    var lote = await _context.Lotes.FindAsync(loteId);
                    if (lote == null) return comparacao;

                comparacao.LoteIdentificador = lote.Identificador;
                comparacao.IdadeDias = lote.IdadeAtualDias;

                // Conversão Alimentar
                var ca = await CalcularConversaoAlimentarAsync(loteId);
                comparacao.ConversaoAlimentar = new MetricaComparacaoDto
                {
                    Nome = "Conversão Alimentar",
                    ValorAtual = ca,
                    ValorPadraoIndustria = PadroesIndustria["ConversaoAlimentar"],
                    ValorPadraoExcelencia = PadroesIndustria["ConversaoAlimentarExcelencia"],
                    PercentualDiferenca = CalcularPercentualDiferenca(ca, PadroesIndustria["ConversaoAlimentar"]),
                    Status = ca <= PadroesIndustria["ConversaoAlimentarExcelencia"] ? "Excelente" :
                             ca <= PadroesIndustria["ConversaoAlimentar"] ? "Bom" : "Abaixo",
                    Unidade = ""
                };

                // GMD
                var gmd = await CalcularGanhoMedioDiarioAsync(loteId);
                comparacao.GanhoMedioDiario = new MetricaComparacaoDto
                {
                    Nome = "Ganho Médio Diário",
                    ValorAtual = gmd,
                    ValorPadraoIndustria = PadroesIndustria["GanhoMedioDiario"],
                    ValorPadraoExcelencia = PadroesIndustria["GanhoMedioDiarioExcelencia"],
                    PercentualDiferenca = CalcularPercentualDiferenca(gmd, PadroesIndustria["GanhoMedioDiario"]),
                    Status = gmd >= PadroesIndustria["GanhoMedioDiarioExcelencia"] ? "Excelente" :
                             gmd >= PadroesIndustria["GanhoMedioDiario"] ? "Bom" : "Abaixo",
                    Unidade = "g/dia"
                };

                // Viabilidade
                comparacao.Viabilidade = new MetricaComparacaoDto
                {
                    Nome = "Viabilidade",
                    ValorAtual = lote.Viabilidade,
                    ValorPadraoIndustria = PadroesIndustria["Viabilidade"],
                    ValorPadraoExcelencia = PadroesIndustria["ViabilidadeExcelencia"],
                    PercentualDiferenca = CalcularPercentualDiferenca(lote.Viabilidade, PadroesIndustria["Viabilidade"]),
                    Status = lote.Viabilidade >= PadroesIndustria["ViabilidadeExcelencia"] ? "Excelente" :
                             lote.Viabilidade >= PadroesIndustria["Viabilidade"] ? "Bom" : "Abaixo",
                    Unidade = "%"
                };

                // IEP
                var iep = await CalcularIEPAsync(loteId);
                comparacao.IEP = new MetricaComparacaoDto
                {
                    Nome = "Índice de Eficiência Produtiva",
                    ValorAtual = iep,
                    ValorPadraoIndustria = PadroesIndustria["IEP"],
                    ValorPadraoExcelencia = PadroesIndustria["IEPExcelencia"],
                    PercentualDiferenca = CalcularPercentualDiferenca(iep, PadroesIndustria["IEP"]),
                    Status = iep >= PadroesIndustria["IEPExcelencia"] ? "Excelente" :
                             iep >= PadroesIndustria["IEP"] ? "Bom" : "Abaixo",
                    Unidade = ""
                };

                // Calcular pontuação geral
                var metricas = new[] { comparacao.ConversaoAlimentar, comparacao.GanhoMedioDiario, 
                                     comparacao.Viabilidade, comparacao.IEP };
                var pontuacaoTotal = (decimal)metricas.Average(m => m.Status == "Excelente" ? 100 : 
                                                           m.Status == "Bom" ? 75 : 50);
                
                comparacao.PontuacaoGeral = Math.Round(pontuacaoTotal, 1);
                comparacao.ClassificacaoGeral = pontuacaoTotal >= 90 ? "Excelente" :
                                               pontuacaoTotal >= 75 ? "Bom" :
                                               pontuacaoTotal >= 60 ? "Regular" : "Ruim";

                    _logger.LogInformation("Comparação com indústria para lote {LoteId}: {Classificacao} ({Pontuacao})", 
                        loteId, comparacao.ClassificacaoGeral, comparacao.PontuacaoGeral);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao comparar com padrões da indústria para lote {LoteId}", loteId);
                }

                return comparacao;
            }, TimeSpan.FromMinutes(15)); // Cache por 15 minutos
        }

        private static decimal CalcularPercentualDiferenca(decimal valorAtual, decimal valorPadrao)
        {
            if (valorPadrao == 0) return 0;
            return Math.Round(((valorAtual - valorPadrao) / valorPadrao) * 100, 2);
        }

        // Implementações simplificadas dos outros métodos
        public Task<decimal> CalcularViabilidadeAsync(int loteId) => Task.FromResult(0m);
        public Task<decimal> CalcularUniformidadeAsync(int loteId) => Task.FromResult(0m);
        public Task<decimal> CalcularDensidadeAtualAsync(int loteId) => Task.FromResult(0m);
        public Task<decimal> CalcularConsumoMedioRacaoPorAveAsync(int loteId, DateTime? dataInicio = null, DateTime? dataFim = null) => Task.FromResult(0m);
        public Task<decimal> CalcularConsumoMedioAguaPorAveAsync(int loteId, DateTime? dataInicio = null, DateTime? dataFim = null) => Task.FromResult(0m);
        public Task<decimal> CalcularRelacaoAguaRacaoAsync(int loteId, DateTime? dataInicio = null, DateTime? dataFim = null) => Task.FromResult(0m);
        public Task<decimal> CalcularMortalidadeAcumuladaAsync(int loteId) => Task.FromResult(0m);
        public Task<decimal> CalcularMortalidadeSemanalAsync(int loteId, int semana) => Task.FromResult(0m);
        public Task<List<RegistroMortalidadePorFaseDto>> CalcularMortalidadePorFaseAsync(int loteId) => Task.FromResult(new List<RegistroMortalidadePorFaseDto>());
        public Task<CurvasCrescimentoDto> ObterCurvasCrescimentoAsync(int loteId) => Task.FromResult(new CurvasCrescimentoDto());
        public Task<AnaliseConsumoDto> AnaliseConsumoDetalhada(int loteId) => Task.FromResult(new AnaliseConsumoDto());
        public Task<ResumoSanitarioDto> ObterResumoSanitarioAsync(int loteId) => Task.FromResult(new ResumoSanitarioDto());
        public Task<ProjecaoAbateDto> CalcularProjecaoAbateAsync(int loteId) => Task.FromResult(new ProjecaoAbateDto());
        public Task<decimal> EstimarPesoMedioAbateAsync(int loteId, DateTime dataAbatePrevista) => Task.FromResult(0m);
    }
}
