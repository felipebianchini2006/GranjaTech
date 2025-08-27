using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GranjaTech.Application.DTOs;

namespace GranjaTech.Application.Services.Interfaces
{
    public interface IAviculturaService
    {
        // Métricas de Performance
        Task<decimal> CalcularIEPAsync(int loteId);
        Task<decimal> CalcularConversaoAlimentarAsync(int loteId);
        Task<decimal> CalcularGanhoMedioDiarioAsync(int loteId);
        Task<decimal> CalcularViabilidadeAsync(int loteId);
        Task<decimal> CalcularUniformidadeAsync(int loteId);
        Task<decimal> CalcularDensidadeAtualAsync(int loteId);
        
        // Consumo
        Task<decimal> CalcularConsumoMedioRacaoPorAveAsync(int loteId, DateTime? dataInicio = null, DateTime? dataFim = null);
        Task<decimal> CalcularConsumoMedioAguaPorAveAsync(int loteId, DateTime? dataInicio = null, DateTime? dataFim = null);
        Task<decimal> CalcularRelacaoAguaRacaoAsync(int loteId, DateTime? dataInicio = null, DateTime? dataFim = null);
        
        // Mortalidade
        Task<decimal> CalcularMortalidadeAcumuladaAsync(int loteId);
        Task<decimal> CalcularMortalidadeSemanalAsync(int loteId, int semana);
        Task<List<RegistroMortalidadePorFaseDto>> CalcularMortalidadePorFaseAsync(int loteId);
        
        // Análises
        Task<CurvasCrescimentoDto> ObterCurvasCrescimentoAsync(int loteId);
        Task<AnaliseConsumoDto> AnaliseConsumoDetalhada(int loteId);
        Task<ResumoSanitarioDto> ObterResumoSanitarioAsync(int loteId);
        Task<List<AlertaParametroDto>> VerificarParametrosForaPadraoAsync(int loteId);
        
        // Projeções
        Task<ProjecaoAbateDto> CalcularProjecaoAbateAsync(int loteId);
        Task<decimal> EstimarPesoMedioAbateAsync(int loteId, DateTime dataAbatePrevista);
        
        // Comparação com padrões da indústria
        Task<ComparacaoIndustriaDto> CompararComPadroesIndustriaAsync(int loteId);
    }
}
