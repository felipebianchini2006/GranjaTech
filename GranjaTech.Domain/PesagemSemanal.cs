using System;
using System.ComponentModel.DataAnnotations;

namespace GranjaTech.Domain
{
    public class PesagemSemanal
    {
        public int Id { get; set; }
        
        [Required]
        public int LoteId { get; set; }
        public Lote Lote { get; set; } = null!;
        
        [Required]
        public DateTime DataPesagem { get; set; }
        
        /// <summary>
        /// Idade do lote em dias na data da pesagem
        /// </summary>
        public int IdadeDias { get; set; }
        
        /// <summary>
        /// Semana de vida das aves (1, 2, 3, etc.)
        /// </summary>
        public int SemanaVida { get; set; }
        
        /// <summary>
        /// Peso médio das aves em gramas
        /// </summary>
        [Range(0, 10000, ErrorMessage = "Peso deve estar entre 0 e 10000 gramas")]
        public decimal PesoMedioGramas { get; set; }
        
        /// <summary>
        /// Número de aves pesadas na amostra
        /// </summary>
        public int QuantidadeAmostrada { get; set; }
        
        /// <summary>
        /// Peso mínimo encontrado na amostra
        /// </summary>
        public decimal? PesoMinimo { get; set; }
        
        /// <summary>
        /// Peso máximo encontrado na amostra
        /// </summary>
        public decimal? PesoMaximo { get; set; }
        
        /// <summary>
        /// Desvio padrão do peso da amostra
        /// </summary>
        public decimal? DesvioPadrao { get; set; }
        
        /// <summary>
        /// Coeficiente de variação (uniformidade) em %
        /// </summary>
        public decimal? CoeficienteVariacao { get; set; }
        
        /// <summary>
        /// Ganho de peso semanal em gramas
        /// </summary>
        public decimal? GanhoSemanal { get; set; }
        
        /// <summary>
        /// Ganho médio diário (GMD) em gramas
        /// </summary>
        public decimal GanhoMedioDiario => GanhoSemanal.HasValue ? GanhoSemanal.Value / 7 : 0;
        
        /// <summary>
        /// Observações da pesagem
        /// </summary>
        [StringLength(500)]
        public string? Observacoes { get; set; }
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
