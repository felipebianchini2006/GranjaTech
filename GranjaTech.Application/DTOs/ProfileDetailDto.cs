using System.Collections.Generic;

namespace GranjaTech.Application.DTOs
{
    public class ProfileDetailDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PerfilNome { get; set; } = string.Empty;
        public List<string> Associados { get; set; } = new List<string>();
    }
}
