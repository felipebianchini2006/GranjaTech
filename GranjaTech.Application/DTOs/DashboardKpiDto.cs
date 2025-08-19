namespace GranjaTech.Application.DTOs
{
    public class DashboardKpiDto
    {
        public decimal TotalEntradas { get; set; }
        public decimal TotalSaidas { get; set; }
        public decimal LucroTotal { get; set; }
        public int LotesAtivos { get; set; }
    }
}