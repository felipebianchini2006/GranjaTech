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
        Task AddAsync(CreateLoteDto loteDto);
        Task<bool> UpdateAsync(int id, UpdateLoteDto loteDto); // Assinatura alterada
        Task DeleteAsync(int id);

        // NOVO: mortalidade
        Task<RegistroMortalidade> RegistrarMortalidadeAsync(CreateRegistroMortalidadeDto dto, int loteIdFromRoute);
        Task<IEnumerable<RegistroMortalidade>> ListarMortalidadesAsync(int loteId);
    }
}