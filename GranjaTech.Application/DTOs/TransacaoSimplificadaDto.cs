using System;

namespace GranjaTech.Application.DTOs
{
    public class TransacaoSimplificadaDto
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public string? LoteIdentificador { get; set; }
        public string? UsuarioNome { get; set; }
        public string? GranjaNome { get; set; }
    }
}
