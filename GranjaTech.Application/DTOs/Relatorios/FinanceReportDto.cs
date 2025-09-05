namespace GranjaTech.Application.DTOs.Relatorios;

public sealed class FinanceReportItemDto
{
    public DateTime Data { get; set; }
    public string Categoria { get; set; } = ""; // "entrada" | "saida"
    public string Descricao { get; set; } = "";
    public decimal Valor { get; set; }
}

public sealed class FinanceReportDto
{
    public int GranjaId { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public decimal TotalEntradas { get; set; }
    public decimal TotalSaidas { get; set; }
    public decimal Saldo { get; set; }
    public List<FinanceReportItemDto> Itens { get; set; } = new();
}
