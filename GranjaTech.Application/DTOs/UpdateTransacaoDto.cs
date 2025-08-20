using System;

namespace GranjaTech.Application.DTOs
{
    public class UpdateTransacaoDto
    {
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public int? LoteId { get; set; }
    }
}