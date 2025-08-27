using System;
using System.ComponentModel.DataAnnotations;

namespace GranjaTech.Domain
{
    public class ConsumoAgua
    {
        public int Id { get; set; }
        
        [Required]
        public int LoteId { get; set; }
        public Lote Lote { get; set; } = null!;
        
        [Required]
        public DateTime Data { get; set; }
        
        /// <summary>
        /// Quantidade de água consumida em litros
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Consumo não pode ser negativo")]
        public decimal QuantidadeLitros { get; set; }
        
        /// <summary>
        /// Número de aves vivas no momento do consumo
        /// </summary>
        public int AvesVivas { get; set; }
        
        /// <summary>
        /// Consumo por ave em ml (calculado automaticamente)
        /// </summary>
        public decimal ConsumoPorAveMl => AvesVivas > 0 ? (QuantidadeLitros * 1000) / AvesVivas : 0;
        
        /// <summary>
        /// Temperatura ambiente no momento da medição
        /// </summary>
        public decimal? TemperaturaAmbiente { get; set; }
        
        /// <summary>
        /// Observações sobre o consumo
        /// </summary>
        [StringLength(500)]
        public string? Observacoes { get; set; }
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
