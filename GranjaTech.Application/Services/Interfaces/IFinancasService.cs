using GranjaTech.Application.DTOs;
using GranjaTech.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Application.Services.Interfaces
{
    public interface IFinancasService
    {
        Task<IEnumerable<TransacaoFinanceira>> GetAllAsync();
        Task AddAsync(CreateTransacaoDto transacaoDto); // Assinatura alterada
        Task<bool> UpdateAsync(int id, UpdateTransacaoDto transacaoDto);
        Task<bool> DeleteAsync(int id);
    }
}
