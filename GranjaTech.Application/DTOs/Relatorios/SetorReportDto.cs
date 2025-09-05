namespace GranjaTech.Application.DTOs.Relatorios;

public sealed class SetorReportDto<TItem>
{
    public int GranjaId { get; set; }
    public string Setor { get; set; } = "";
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public List<TItem> Itens { get; set; } = new();
}
