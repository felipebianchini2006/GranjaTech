using GranjaTech.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Application.Services.Interfaces
{
    public interface IFinancasService
    {
        Task<IEnumerable<TransacaoFinanceira>> GetAllAsync();
        Task AddAsync(TransacaoFinanceira transacao);
        // Por enquanto, não vamos implementar Update e Delete para manter simples,
        // mas a estrutura está pronta se precisarmos no futuro.
    }
}