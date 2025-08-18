using GranjaTech.Application.DTOs;
using GranjaTech.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Application.Services.Interfaces
{
    public interface ILoteService
    {
        Task<IEnumerable<Lote>> GetAllAsync();
        Task<Lote?> GetByIdAsync(int id);
        Task AddAsync(CreateLoteDto loteDto); // Assinatura do método alterada
        Task UpdateAsync(Lote lote);
        Task DeleteAsync(int id);
    }
}