using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Infrastructure.Services.Implementations
{
    public class FinancasService : IFinancasService
    {
        private readonly GranjaTechDbContext _context;

        public FinancasService(GranjaTechDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TransacaoFinanceira>> GetAllAsync()
        {
            // Inclui o Lote relacionado para podermos mostrar o identificador
            return await _context.TransacoesFinanceiras
                                 .Include(t => t.Lote)
                                 .ToListAsync();
        }

        public async Task AddAsync(TransacaoFinanceira transacao)
        {
            // Se LoteId for 0 ou null, garante que seja null no banco
            if (transacao.LoteId == 0)
            {
                transacao.LoteId = null;
            }
            await _context.TransacoesFinanceiras.AddAsync(transacao);
            await _context.SaveChangesAsync();
        }
    }
}