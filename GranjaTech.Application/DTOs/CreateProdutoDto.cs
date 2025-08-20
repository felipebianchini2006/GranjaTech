namespace GranjaTech.Application.DTOs
{
    public class CreateProdutoDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public string UnidadeDeMedida { get; set; } = string.Empty;
        public int GranjaId { get; set; }
    }
}
