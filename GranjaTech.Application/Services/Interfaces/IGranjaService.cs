using GranjaTech.Application.DTOs;
using GranjaTech.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Application.Services.Interfaces
{
    public interface IGranjaService
    {
        Task<IEnumerable<Granja>> GetAllAsync();
        Task<Granja?> GetByIdAsync(int id);
        Task AddAsync(CreateGranjaDto granjaDto);
        Task<bool> UpdateAsync(int id, UpdateGranjaDto granjaDto); // Assinatura alterada
        Task DeleteAsync(int id);
    }
}