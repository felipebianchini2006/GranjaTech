using GranjaTech.Domain;
using Microsoft.EntityFrameworkCore;

namespace GranjaTech.Infrastructure
{
    public class GranjaTechDbContext : DbContext
    {
        // Este construtor é essencial para a injeção de dependência que configuraremos na API.
        public GranjaTechDbContext(DbContextOptions<GranjaTechDbContext> options) : base(options)
        {
        }

        // Cada DbSet<T> representa uma tabela no banco de dados.
        // O nome da propriedade (ex: Granjas) será, por padrão, o nome da tabela.
        public DbSet<Granja> Granjas { get; set; }
        public DbSet<Lote> Lotes { get; set; }

        // Futuramente, adicionaremos aqui os outros DbSets (Usuarios, Sensores, etc.)
    }
}