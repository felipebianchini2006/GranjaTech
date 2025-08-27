using System;
using System.Collections.Generic;

namespace GranjaTech.Application.DTOs
{
    public class AnaliseConsumoDto
    {
        public int LoteId { get; set; }
        public string LoteIdentificador { get; set; } = string.Empty;
        public int IdadeAtualDias { get; set; }
        
        // Consumo de Ração
        public decimal ConsumoTotalRacaoKg { get; set; }
        public decimal ConsumoMedioRacaoPorAve { get; set; }
        public decimal ConsumoMedioRacaoPorDia { get; set; }
        public decimal ConsumoAcumuladoRacao { get; set; }
        
        // Consumo de Água
        public decimal ConsumoTotalAguaLitros { get; set; }
        public decimal ConsumoMedioAguaPorAve { get; set; }
        public decimal ConsumoMedioAguaPorDia { get; set; }
        public decimal ConsumoAcumuladoAgua { get; set; }
        
        // Relações
        public decimal RelacaoAguaRacao { get; set; }
        public decimal RelacaoConsumoIdeal { get; set; } = 2.0m; // Padrão da indústria
        
        // Fases de Ração
        public List<ConsumoFaseDto> ConsumosPorFase { get; set; } = new List<ConsumoFaseDto>();
        
        // Eficiência
        public decimal EficienciaConversao { get; set; }
        public string StatusConsumo { get; set; } = string.Empty; // Normal, Alto, Baixo
        
        // Previsões
        public decimal ConsumoPrevistoTotal { get; set; }
        public decimal CustoEstimadoRacao { get; set; }
    }

    public class ConsumoFaseDto
    {
        public string Fase { get; set; } = string.Empty; // Inicial, Crescimento, Terminação
        public int DiaInicio { get; set; }
        public int DiaFim { get; set; }
        public decimal QuantidadeKg { get; set; }
        public decimal PercentualTotal { get; set; }
        public decimal ConsumoMedioPorAve { get; set; }
    }

    public class RegistroMortalidadePorFaseDto
    {
        public string Fase { get; set; } = string.Empty; // Inicial(1-7), Crescimento(8-21), Terminação(22+)
        public int DiaInicio { get; set; }
        public int DiaFim { get; set; }
        public int TotalMortes { get; set; }
        public decimal PercentualFase { get; set; }
        public decimal PercentualAcumulado { get; set; }
        public List<string> PrincipaisCausas { get; set; } = new List<string>();
    }

    public class ResumoSanitarioDto
    {
        public int LoteId { get; set; }
        public string LoteIdentificador { get; set; } = string.Empty;
        
        // Estatísticas Gerais
        public int TotalEventos { get; set; }
        public int TotalVacinacoes { get; set; }
        public int TotalMedicacoes { get; set; }
        public int TotalDoencas { get; set; }
        
        // Custos
        public decimal CustoTotalSanitario { get; set; }
        public decimal CustoPorAve { get; set; }
        
        // Eventos por Tipo
        public List<EventoSanitarioResumoDto> EventosPorTipo { get; set; } = new List<EventoSanitarioResumoDto>();
        
        // Cronograma de Vacinação
        public List<VacinacaoScheduleDto> CronogramaVacinacao { get; set; } = new List<VacinacaoScheduleDto>();
        
        // Alertas
        public List<string> AlertasSanitarios { get; set; } = new List<string>();
        
        // Próximas Ações
        public List<ProximaAcaoSanitariaDto> ProximasAcoes { get; set; } = new List<ProximaAcaoSanitariaDto>();
    }

    public class EventoSanitarioResumoDto
    {
        public string TipoEvento { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal CustoTotal { get; set; }
        public DateTime UltimaOcorrencia { get; set; }
    }

    public class VacinacaoScheduleDto
    {
        public string Vacina { get; set; } = string.Empty;
        public DateTime DataPrevista { get; set; }
        public DateTime? DataRealizada { get; set; }
        public bool Realizada { get; set; }
        public string Status { get; set; } = string.Empty; // Pendente, Realizada, Atrasada
    }

    public class ProximaAcaoSanitariaDto
    {
        public string Acao { get; set; } = string.Empty;
        public DateTime DataPrevista { get; set; }
        public string Prioridade { get; set; } = string.Empty; // Alta, Media, Baixa
        public string Descricao { get; set; } = string.Empty;
    }

    public class ProjecaoAbateDto
    {
        public int LoteId { get; set; }
        public string LoteIdentificador { get; set; } = string.Empty;
        
        // Datas
        public DateTime DataAbatePrevista { get; set; }
        public int IdadeAbateDias { get; set; }
        
        // Pesos
        public decimal PesoMedioAtualGramas { get; set; }
        public decimal PesoMedioProjetadoGramas { get; set; }
        public decimal PesoTotalProjetadoKg { get; set; }
        
        // Quantidades
        public int QuantidadeAvesProjetada { get; set; }
        public int QuantidadeAvesAtual { get; set; }
        public decimal MortalidadeProjetadaPercentual { get; set; }
        
        // Rendimentos
        public decimal RendimentoCarcacaEstimado { get; set; } = 75m; // % padrão
        public decimal PesoCarcacaProjetadoKg { get; set; }
        
        // Métricas Finais
        public decimal ConversaoAlimentarProjetada { get; set; }
        public decimal IEPProjetado { get; set; }
        public decimal ViabilidadeProjetada { get; set; }
        
        // Financeiro
        public decimal ValorEstimadoPorKg { get; set; }
        public decimal ReceitaBrutaEstimada { get; set; }
        public decimal CustoProducaoEstimado { get; set; }
        public decimal LucroEstimado { get; set; }
        
        // Status
        public string StatusProjecao { get; set; } = string.Empty; // OnTrack, Atraso, Adiantado
        public List<string> Observacoes { get; set; } = new List<string>();
    }
}
