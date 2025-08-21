using GranjaTech.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace GranjaTech.Application.Services.Interfaces
{
    public interface IRelatorioService
    {
        Task<RelatorioFinanceiroDto> GetRelatorioFinanceiroAsync(DateTime dataInicio, DateTime dataFim, int? granjaId);
        Task<RelatorioProducaoDto> GetRelatorioProducaoAsync(DateTime dataInicio, DateTime dataFim, int? granjaId);
    }
}
