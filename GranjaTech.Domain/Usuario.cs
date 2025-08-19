using System.Collections.Generic;

namespace GranjaTech.Domain
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty; // ADICIONE ESTA LINHA
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;

        public int PerfilId { get; set; }
        public Perfil Perfil { get; set; } = null!;

        public ICollection<FinanceiroProdutor> FinanceirosAssociados { get; set; } = new List<FinanceiroProdutor>();
        public ICollection<FinanceiroProdutor> ProdutoresGerenciados { get; set; } = new List<FinanceiroProdutor>();
    }
}