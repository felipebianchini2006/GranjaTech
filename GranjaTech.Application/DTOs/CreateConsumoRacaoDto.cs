using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GranjaTech.Application.DTOs
{
    public class CreateConsumoRacaoDto
    {
        [Required(ErrorMessage = "LoteId é obrigatório")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int LoteId { get; set; }
        
        [Required(ErrorMessage = "Data é obrigatória")]
        public DateTime Data { get; set; }
        
        [Required(ErrorMessage = "Quantidade em kg é obrigatória")]
        [Range(0.001, 10000, ErrorMessage = "Quantidade deve estar entre 0.001 e 10.000 kg")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal QuantidadeKg { get; set; }
        
        [Required(ErrorMessage = "Tipo de ração é obrigatório")]
        [StringLength(50, ErrorMessage = "Tipo de ração deve ter no máximo 50 caracteres")]
        public string TipoRacao { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Número de aves vivas é obrigatório")]
        [Range(1, 100000, ErrorMessage = "Aves vivas deve estar entre 1 e 100.000")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int AvesVivas { get; set; }
        
        [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres")]
        public string? Observacoes { get; set; }
    }
}
