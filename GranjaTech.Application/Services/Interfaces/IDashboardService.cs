using GranjaTech.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Application.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardKpiDto> GetKpisAsync();
        Task<IEnumerable<MonthlySummaryDto>> GetMonthlySummaryAsync();
    }
}