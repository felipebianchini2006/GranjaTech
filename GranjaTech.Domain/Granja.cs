namespace GranjaTech.Domain
{
    public class Granja
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty; // ADICIONE ESTA LINHA
        public string Nome { get; set; } = string.Empty;
        public string? Localizacao { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
    }
}