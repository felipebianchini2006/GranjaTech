using GranjaTech.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Application.Services.Interfaces
{
    public interface IAuditoriaService
    {
        Task<IEnumerable<LogAuditoria>> GetAllAsync();
        Task RegistrarLog(string acao, string detalhes);
    }
}