namespace GranjaTech.Domain
{
    public class FinanceiroProdutor
    {
        public int FinanceiroId { get; set; }
        public Usuario Financeiro { get; set; } = null!;

        public int ProdutorId { get; set; }
        public Usuario Produtor { get; set; } = null!;
    }
}