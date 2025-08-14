using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using GranjaTech.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Infrastructure.Services.Implementations
{
    public class GranjaService : IGranjaService
    {
        private readonly GranjaTechDbContext _context;

        public GranjaService(GranjaTechDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Granja>> GetAllAsync()
        {
            return await _context.Granjas.ToListAsync();
        }

        public async Task<Granja?> GetByIdAsync(int id)
        {
            return await _context.Granjas.FindAsync(id);
        }

        public async Task AddAsync(Granja granja)
        {
            await _context.Granjas.AddAsync(granja);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Granja granja)
        {
            var entityToUpdate = await _context.Granjas.FindAsync(granja.Id);

            if (entityToUpdate != null)
            {
                entityToUpdate.Nome = granja.Nome;
                entityToUpdate.Localizacao = granja.Localizacao;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var granjaToDelete = await _context.Granjas.FindAsync(id);

            if (granjaToDelete != null)
            {
                _context.Granjas.Remove(granjaToDelete);
                await _context.SaveChangesAsync();
            }
        }
    }
}