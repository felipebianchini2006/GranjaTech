using System;

namespace GranjaTech.Application.DTOs
{
    public class RegistroMortalidadeDto
    {
        public int Id { get; set; }
        public int LoteId { get; set; }
        public DateTime Data { get; set; }
        public int Quantidade { get; set; }
        public string? Motivo { get; set; }
        public string? Setor { get; set; }
        public string? Observacoes { get; set; }
        public string? ResponsavelRegistro { get; set; }
        public decimal PercentualMortalidadeDia { get; set; }
        public int IdadeDias { get; set; }
    }
}
