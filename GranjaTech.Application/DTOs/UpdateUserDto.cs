namespace GranjaTech.Application.DTOs
{
    public class UpdateUserDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int PerfilId { get; set; }
    }
}