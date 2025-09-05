using GranjaTech.Application.DTOs.Relatorios;

namespace GranjaTech.Application.Services.Interfaces;

public interface IRelatorioAvancadoService
{
    Task<FinanceReportDto> FinanceiroAsync(int granjaId, DateTime inicioUtc, DateTime fimUtc, CancellationToken ct);
    Task<GeralReportDto> GeralAsync(int granjaId, DateTime inicioUtc, DateTime fimUtc, CancellationToken ct);

    Task<SetorReportDto<ConsumoResumoDto>> ConsumoAsync(int granjaId, DateTime inicioUtc, DateTime fimUtc, CancellationToken ct);
    Task<SetorReportDto<PesagemResumoDto>> PesagemAsync(int granjaId, DateTime inicioUtc, DateTime fimUtc, CancellationToken ct);
    Task<SetorReportDto<SanitarioResumoDto>> SanitarioAsync(int granjaId, DateTime inicioUtc, DateTime fimUtc, CancellationToken ct);
    Task<SetorReportDto<SensorResumoDto>> SensoresAsync(int granjaId, DateTime inicioUtc, DateTime fimUtc, CancellationToken ct);
}
