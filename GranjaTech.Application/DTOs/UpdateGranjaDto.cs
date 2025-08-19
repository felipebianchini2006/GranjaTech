namespace GranjaTech.Application.DTOs
{
    public class UpdateGranjaDto
    {
        public string Nome { get; set; } = string.Empty;
        public string? Localizacao { get; set; }
        // Admin poderá alterar o dono, então incluímos o UsuarioId
        public int UsuarioId { get; set; }
    }
}