using System.ComponentModel.DataAnnotations;

namespace GranjaTech.Application.DTOs
{
    public class UpdateProdutoDto : CreateProdutoDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Id do produto é obrigatório")]
        public int Id { get; set; }
    }
}
