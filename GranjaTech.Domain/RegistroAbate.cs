using System;
using System.ComponentModel.DataAnnotations;

namespace GranjaTech.Domain
{
    public class RegistroAbate
    {
        public int Id { get; set; }
        
        [Required]
        public int LoteId { get; set; }
        public Lote Lote { get; set; } = null!;
        
        [Required]
        public DateTime DataAbate { get; set; }
        
        /// <summary>
        /// Data prevista para o abate
        /// </summary>
        public DateTime? DataAbatePrevista { get; set; }
        
        /// <summary>
        /// Idade das aves no abate em dias
        /// </summary>
        public int IdadeAbateDias { get; set; }
        
        /// <summary>
        /// Número de aves enviadas para abate
        /// </summary>
        public int QuantidadeEnviada { get; set; }
        
        /// <summary>
        /// Peso vivo total em kg
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Peso vivo não pode ser negativo")]
        public decimal PesoVivoTotalKg { get; set; }
        
        /// <summary>
        /// Peso médio por ave em gramas
        /// </summary>
        public decimal PesoMedioPorAve => QuantidadeEnviada > 0 ? (PesoVivoTotalKg * 1000) / QuantidadeEnviada : 0;
        
        /// <summary>
        /// Peso total da carcaça em kg
        /// </summary>
        public decimal? PesoCarcacaTotalKg { get; set; }
        
        /// <summary>
        /// Rendimento de carcaça em %
        /// </summary>
        public decimal RendimentoCarcaca => PesoVivoTotalKg > 0 && PesoCarcacaTotalKg.HasValue 
            ? (PesoCarcacaTotalKg.Value / PesoVivoTotalKg) * 100 : 0;
        
        /// <summary>
        /// Número de aves condenadas
        /// </summary>
        public int? AvesCondenadas { get; set; }
        
        /// <summary>
        /// Motivos das condenações
        /// </summary>
        [StringLength(1000)]
        public string? MotivoCondenacoes { get; set; }
        
        /// <summary>
        /// Peso das aves condenadas em kg
        /// </summary>
        public decimal? PesoCondenadoKg { get; set; }
        
        /// <summary>
        /// Frigorífico/abatedouro de destino
        /// </summary>
        [StringLength(200)]
        public string? FrigorificoDestino { get; set; }
        
        /// <summary>
        /// Transportadora responsável
        /// </summary>
        [StringLength(200)]
        public string? Transportadora { get; set; }
        
        /// <summary>
        /// Valor pago por kg de peso vivo
        /// </summary>
        public decimal? ValorPorKg { get; set; }
        
        /// <summary>
        /// Valor total recebido
        /// </summary>
        public decimal? ValorTotalRecebido { get; set; }
        
        /// <summary>
        /// Observações sobre o abate
        /// </summary>
        [StringLength(1000)]
        public string? Observacoes { get; set; }
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
