using System;
using System.ComponentModel.DataAnnotations;

namespace GranjaTech.Domain
{
    public class RegistroMortalidade
    {
        public int Id { get; set; }
        
        [Required]
        public int LoteId { get; set; }
        public Lote Lote { get; set; } = null!;
        
        [Required]
        public DateTime Data { get; set; }
        
        /// <summary>
        /// Número de aves mortas no dia
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Mortalidade não pode ser negativa")]
        public int QuantidadeMortas { get; set; }
        
        /// <summary>
        /// Número de aves vivas antes da mortalidade
        /// </summary>
        public int AvesVivas { get; set; }
        
        /// <summary>
        /// Percentual de mortalidade do dia
        /// </summary>
        public decimal PercentualMortalidadeDia => AvesVivas > 0 ? (decimal)QuantidadeMortas / AvesVivas * 100 : 0;
        
        /// <summary>
        /// Principal causa da mortalidade
        /// </summary>
        [StringLength(200)]
        public string? CausaPrincipal { get; set; }
        
        /// <summary>
        /// Idade das aves em dias
        /// </summary>
        public int IdadeDias { get; set; }
        
        /// <summary>
        /// Peso médio das aves mortas (se pesadas)
        /// </summary>
        public decimal? PesoMedioMortas { get; set; }
        
        /// <summary>
        /// Observações sobre as condições das aves mortas
        /// </summary>
        [StringLength(1000)]
        public string? Observacoes { get; set; }
        
        /// <summary>
        /// Ação tomada em relação à mortalidade
        /// </summary>
        [StringLength(500)]
        public string? AcaoTomada { get; set; }
        
        /// <summary>
        /// Responsável pelo registro
        /// </summary>
        [StringLength(200)]
        public string? ResponsavelRegistro { get; set; }
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
