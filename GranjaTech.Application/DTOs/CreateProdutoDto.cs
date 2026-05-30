using System.ComponentModel.DataAnnotations;

namespace GranjaTech.Application.DTOs
{
    public class CreateProdutoDto
    {
        [Required(ErrorMessage = "Nome do produto é obrigatório")]
        [StringLength(200, ErrorMessage = "Nome do produto deve ter no máximo 200 caracteres")]
        public string Nome { get; set; } = string.Empty;
        [Required(ErrorMessage = "Tipo do produto é obrigatório")]
        [StringLength(100, ErrorMessage = "Tipo do produto deve ter no máximo 100 caracteres")]
        public string Tipo { get; set; } = string.Empty;
        [Range(0, double.MaxValue, ErrorMessage = "Quantidade não pode ser negativa")]
        public decimal Quantidade { get; set; }
        [Required(ErrorMessage = "Unidade de medida é obrigatória")]
        [StringLength(50, ErrorMessage = "Unidade de medida deve ter no máximo 50 caracteres")]
        public string UnidadeDeMedida { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "GranjaId é obrigatório")]
        public int GranjaId { get; set; }
    }
}
