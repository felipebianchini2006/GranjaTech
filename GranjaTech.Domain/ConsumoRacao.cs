using System;
using System.ComponentModel.DataAnnotations;

namespace GranjaTech.Domain
{
    public class ConsumoRacao
    {
        public int Id { get; set; }
        
        [Required]
        public int LoteId { get; set; }
        public Lote Lote { get; set; } = null!;
        
        [Required]
        public DateTime Data { get; set; }
        
        /// <summary>
        /// Quantidade de ração consumida em kg
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Consumo não pode ser negativo")]
        public decimal QuantidadeKg { get; set; }
        
        /// <summary>
        /// Tipo/Fase da ração: Inicial, Crescimento, Terminação
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TipoRacao { get; set; } = string.Empty;
        
        /// <summary>
        /// Número de aves vivas no momento do consumo
        /// </summary>
        public int AvesVivas { get; set; }
        
        /// <summary>
        /// Consumo por ave em gramas (calculado automaticamente)
        /// </summary>
        public decimal ConsumoPorAveGramas => AvesVivas > 0 ? (QuantidadeKg * 1000) / AvesVivas : 0;
        
        /// <summary>
        /// Observações sobre o consumo
        /// </summary>
        [StringLength(500)]
        public string? Observacoes { get; set; }
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
