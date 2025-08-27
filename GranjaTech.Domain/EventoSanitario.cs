using System;
using System.ComponentModel.DataAnnotations;

namespace GranjaTech.Domain
{
    public class EventoSanitario
    {
        public int Id { get; set; }
        
        [Required]
        public int LoteId { get; set; }
        public Lote Lote { get; set; } = null!;
        
        [Required]
        public DateTime Data { get; set; }
        
        /// <summary>
        /// Tipo do evento: Vacinacao, Medicacao, Doenca, Preventivo
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TipoEvento { get; set; } = string.Empty;
        
        /// <summary>
        /// Nome do produto/vacina/medicamento aplicado
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Produto { get; set; } = string.Empty;
        
        /// <summary>
        /// Lote/número do produto
        /// </summary>
        [StringLength(100)]
        public string? LoteProduto { get; set; }
        
        /// <summary>
        /// Dosagem aplicada
        /// </summary>
        [StringLength(100)]
        public string? Dosagem { get; set; }
        
        /// <summary>
        /// Via de administração: Oral, Intramuscular, Subcutanea, Spray, Água, Ração
        /// </summary>
        [StringLength(50)]
        public string? ViaAdministracao { get; set; }
        
        /// <summary>
        /// Número de aves tratadas
        /// </summary>
        public int? AvesTratadas { get; set; }
        
        /// <summary>
        /// Duração do tratamento em dias
        /// </summary>
        public int? DuracaoTratamentoDias { get; set; }
        
        /// <summary>
        /// Período de carência em dias
        /// </summary>
        public int? PeriodoCarenciaDias { get; set; }
        
        /// <summary>
        /// Responsável pela aplicação
        /// </summary>
        [StringLength(200)]
        public string? ResponsavelAplicacao { get; set; }
        
        /// <summary>
        /// Sintomas observados (para casos de doença)
        /// </summary>
        [StringLength(1000)]
        public string? Sintomas { get; set; }
        
        /// <summary>
        /// Observações adicionais
        /// </summary>
        [StringLength(1000)]
        public string? Observacoes { get; set; }
        
        /// <summary>
        /// Custo do tratamento/vacina
        /// </summary>
        public decimal? Custo { get; set; }
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
