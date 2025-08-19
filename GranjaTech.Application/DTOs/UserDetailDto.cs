using System.Collections.Generic;

namespace GranjaTech.Application.DTOs
{
    public class UserDetailDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int PerfilId { get; set; }
        public List<int> ProdutorIds { get; set; } = new List<int>();
    }
}