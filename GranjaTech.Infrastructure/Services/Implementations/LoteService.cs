using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using GranjaTech.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Infrastructure.Services.Implementations
{
    public class LoteService : ILoteService
    {
        private readonly GranjaTechDbContext _context;

        public LoteService(GranjaTechDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Lote>> GetAllAsync()
        {
            // .Include(l => l.Granja) diz ao EF para também carregar os dados da Granja relacionada.
            return await _context.Lotes
                                 .Include(l => l.Granja)
                                 .ToListAsync();
        }

        public async Task<Lote?> GetByIdAsync(int id)
        {
            // Também usamos o .Include para buscar um lote específico com os dados da sua granja.
            return await _context.Lotes
                                 .Include(l => l.Granja)
                                 .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task AddAsync(Lote lote)
        {
            // Para criar um lote, o objeto 'lote' já deve vir com um 'GranjaId' válido.
            await _context.Lotes.AddAsync(lote);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Lote lote)
        {
            var entityToUpdate = await _context.Lotes.FindAsync(lote.Id);
            if (entityToUpdate != null)
            {
                entityToUpdate.Identificador = lote.Identificador;
                entityToUpdate.DataEntrada = lote.DataEntrada;
                entityToUpdate.DataSaida = lote.DataSaida;
                entityToUpdate.QuantidadeAvesInicial = lote.QuantidadeAvesInicial;
                entityToUpdate.GranjaId = lote.GranjaId; // Permite mover um lote para outra granja

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var loteToDelete = await _context.Lotes.FindAsync(id);
            if (loteToDelete != null)
            {
                _context.Lotes.Remove(loteToDelete);
                await _context.SaveChangesAsync();
            }
        }
    }
}