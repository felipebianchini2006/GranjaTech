using System;
using System.ComponentModel.DataAnnotations;

namespace GranjaTech.Application.DTOs
{
    public class CreateEventoSanitarioDto
    {
        [Required(ErrorMessage = "LoteId é obrigatório")]
        public int LoteId { get; set; }
        
        [Required(ErrorMessage = "Data é obrigatória")]
        public DateTime Data { get; set; }
        
        [Required(ErrorMessage = "Tipo do evento é obrigatório")]
        [StringLength(50, ErrorMessage = "Tipo do evento deve ter no máximo 50 caracteres")]
        public string TipoEvento { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Produto é obrigatório")]
        [StringLength(200, ErrorMessage = "Produto deve ter no máximo 200 caracteres")]
        public string Produto { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "Lote do produto deve ter no máximo 100 caracteres")]
        public string? LoteProduto { get; set; }
        
        [StringLength(100, ErrorMessage = "Dosagem deve ter no máximo 100 caracteres")]
        public string? Dosagem { get; set; }
        
        [StringLength(50, ErrorMessage = "Via de administração deve ter no máximo 50 caracteres")]
        public string? ViaAdministracao { get; set; }
        
        [Range(1, 100000, ErrorMessage = "Aves tratadas deve estar entre 1 e 100.000")]
        public int? AvesTratadas { get; set; }
        
        [Range(1, 365, ErrorMessage = "Duração do tratamento deve estar entre 1 e 365 dias")]
        public int? DuracaoTratamentoDias { get; set; }
        
        [Range(0, 365, ErrorMessage = "Período de carência deve estar entre 0 e 365 dias")]
        public int? PeriodoCarenciaDias { get; set; }
        
        [StringLength(200, ErrorMessage = "Responsável deve ter no máximo 200 caracteres")]
        public string? ResponsavelAplicacao { get; set; }
        
        [StringLength(1000, ErrorMessage = "Sintomas devem ter no máximo 1000 caracteres")]
        public string? Sintomas { get; set; }
        
        [StringLength(1000, ErrorMessage = "Observações devem ter no máximo 1000 caracteres")]
        public string? Observacoes { get; set; }
        
        [Range(0, 1000000, ErrorMessage = "Custo deve estar entre 0 e 1.000.000")]
        public decimal? Custo { get; set; }
    }
}
