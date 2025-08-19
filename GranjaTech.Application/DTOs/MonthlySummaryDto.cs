namespace GranjaTech.Application.DTOs
{
    public class MonthlySummaryDto
    {
        public string Mes { get; set; } = string.Empty;
        public decimal Entradas { get; set; }
        public decimal Saidas { get; set; }
    }
}