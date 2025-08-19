namespace GranjaTech.Application.DTOs
{
    public class CreateGranjaDto
    {
        public string Nome { get; set; } = string.Empty;
        public string? Localizacao { get; set; }
        public int? UsuarioId { get; set; } // O ID do dono, opcional para Produtor, obrigatório para Admin
    }
}