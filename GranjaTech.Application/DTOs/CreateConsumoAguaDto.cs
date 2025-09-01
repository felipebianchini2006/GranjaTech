using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GranjaTech.Application.DTOs
{
    public class CreateConsumoAguaDto
    {
        [Required(ErrorMessage = "LoteId é obrigatório")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int LoteId { get; set; }

        [Required(ErrorMessage = "Data é obrigatória")]
        public DateTimeOffset Data { get; set; }

        [Required(ErrorMessage = "Quantidade em litros é obrigatória")]
        [Range(0.001, 10000, ErrorMessage = "Quantidade deve estar entre 0.001 e 10.000 litros")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal QuantidadeLitros { get; set; }

        [Required(ErrorMessage = "Número de aves vivas é obrigatório")]
        [Range(1, 100000, ErrorMessage = "Aves vivas deve estar entre 1 e 100.000")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int AvesVivas { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal? TemperaturaAmbiente { get; set; }

        [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres")]
        public string? Observacoes { get; set; }
    }
}