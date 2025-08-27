using System;
using System.ComponentModel.DataAnnotations;

namespace GranjaTech.Application.DTOs
{
    public class CreatePesagemSemanalDto
    {
        [Required(ErrorMessage = "LoteId é obrigatório")]
        public int LoteId { get; set; }
        
        [Required(ErrorMessage = "Data da pesagem é obrigatória")]
        public DateTime DataPesagem { get; set; }
        
        [Required(ErrorMessage = "Idade em dias é obrigatória")]
        [Range(1, 365, ErrorMessage = "Idade deve estar entre 1 e 365 dias")]
        public int IdadeDias { get; set; }
        
        [Required(ErrorMessage = "Semana de vida é obrigatória")]
        [Range(1, 52, ErrorMessage = "Semana de vida deve estar entre 1 e 52")]
        public int SemanaVida { get; set; }
        
        [Required(ErrorMessage = "Peso médio é obrigatório")]
        [Range(10, 10000, ErrorMessage = "Peso médio deve estar entre 10 e 10.000 gramas")]
        public decimal PesoMedioGramas { get; set; }
        
        [Required(ErrorMessage = "Quantidade amostrada é obrigatória")]
        [Range(10, 1000, ErrorMessage = "Quantidade amostrada deve estar entre 10 e 1.000 aves")]
        public int QuantidadeAmostrada { get; set; }
        
        [Range(5, 15000, ErrorMessage = "Peso mínimo deve estar entre 5 e 15.000 gramas")]
        public decimal? PesoMinimo { get; set; }
        
        [Range(5, 15000, ErrorMessage = "Peso máximo deve estar entre 5 e 15.000 gramas")]
        public decimal? PesoMaximo { get; set; }
        
        [Range(0, 1000, ErrorMessage = "Desvio padrão deve estar entre 0 e 1.000")]
        public decimal? DesvioPadrao { get; set; }
        
        [Range(0, 100, ErrorMessage = "Coeficiente de variação deve estar entre 0 e 100%")]
        public decimal? CoeficienteVariacao { get; set; }
        
        [Range(-500, 2000, ErrorMessage = "Ganho semanal deve estar entre -500 e 2.000 gramas")]
        public decimal? GanhoSemanal { get; set; }
        
        [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres")]
        public string? Observacoes { get; set; }
    }
}
