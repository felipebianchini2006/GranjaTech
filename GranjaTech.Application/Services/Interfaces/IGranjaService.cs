using GranjaTech.Domain; // Para ter acesso à classe Granja
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Application.Services.Interfaces
{
    public interface IGranjaService
    {
        Task<IEnumerable<Granja>> GetAllAsync();
        Task<Granja?> GetByIdAsync(int id);
        Task AddAsync(Granja granja);
        Task UpdateAsync(Granja granja);
        Task DeleteAsync(int id);
    }
}