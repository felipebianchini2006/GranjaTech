using System.Collections.Generic;

namespace GranjaTech.Application.DTOs
{
    public class RegisterDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public int PerfilId { get; set; }
        public List<int>? ProdutorIds { get; set; } // Alterado de 'ProdutorOwnerId'
    }
}