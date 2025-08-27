using System;

namespace GranjaTech.Application.DTOs
{
    public class AlertaParametroDto
    {
        public string TipoAlerta { get; set; } = string.Empty; // Temperatura, Umidade, Mortalidade, Consumo, etc.
        public string Severidade { get; set; } = string.Empty; // Baixa, Media, Alta, Critica
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorAtual { get; set; }
        public decimal? ValorMinimo { get; set; }
        public decimal? ValorMaximo { get; set; }
        public string Unidade { get; set; } = string.Empty;
        public DateTime DataOcorrencia { get; set; }
        public string? Recomendacao { get; set; }
        public bool Ativo { get; set; } = true;
    }

    public class ComparacaoIndustriaDto
    {
        public int LoteId { get; set; }
        public string LoteIdentificador { get; set; } = string.Empty;
        public int IdadeDias { get; set; }
        
        // Comparações principais
        public MetricaComparacaoDto ConversaoAlimentar { get; set; } = new MetricaComparacaoDto();
        public MetricaComparacaoDto GanhoMedioDiario { get; set; } = new MetricaComparacaoDto();
        public MetricaComparacaoDto Viabilidade { get; set; } = new MetricaComparacaoDto();
        public MetricaComparacaoDto IEP { get; set; } = new MetricaComparacaoDto();
        public MetricaComparacaoDto PesoMedio { get; set; } = new MetricaComparacaoDto();
        
        public string ClassificacaoGeral { get; set; } = string.Empty; // Excelente, Bom, Regular, Ruim
        public decimal PontuacaoGeral { get; set; } // 0-100
    }

    public class MetricaComparacaoDto
    {
        public string Nome { get; set; } = string.Empty;
        public decimal ValorAtual { get; set; }
        public decimal ValorPadraoIndustria { get; set; }
        public decimal ValorPadraoExcelencia { get; set; }
        public decimal PercentualDiferenca { get; set; }
        public string Status { get; set; } = string.Empty; // Acima, Dentro, Abaixo
        public string Unidade { get; set; } = string.Empty;
    }
}
