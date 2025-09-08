using System;
using System.ComponentModel.DataAnnotations;

namespace GranjaTech.Application.DTOs
{
    public class CreateRegistroMortalidadeDto
    {
        public int? LoteId { get; set; } // pode vir da rota

        [Required]
        public DateTime Data { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantidade { get; set; }

        public string? Motivo { get; set; }
        public string? Setor { get; set; }
        public string? Observacoes { get; set; }
    }
}
