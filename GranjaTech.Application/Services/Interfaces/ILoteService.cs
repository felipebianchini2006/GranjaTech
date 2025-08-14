using GranjaTech.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Application.Services.Interfaces
{
    public interface ILoteService
    {
        Task<IEnumerable<Lote>> GetAllAsync();
        Task<Lote?> GetByIdAsync(int id);
        Task AddAsync(Lote lote);
        Task UpdateAsync(Lote lote);
        Task DeleteAsync(int id);
    }
}