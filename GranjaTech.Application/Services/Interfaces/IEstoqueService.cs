using GranjaTech.Application.DTOs; // Adicione este using
using GranjaTech.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Application.Services.Interfaces
{
    public interface IEstoqueService
    {
        Task<IEnumerable<Produto>> GetAllAsync();
        Task AddAsync(CreateProdutoDto produtoDto); // Assinatura alterada
        Task<bool> UpdateAsync(int id, Produto produto);
        Task<bool> DeleteAsync(int id);
    }
}
