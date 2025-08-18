using GranjaTech.Domain;
using Microsoft.EntityFrameworkCore;

namespace GranjaTech.Infrastructure
{
    public class GranjaTechDbContext : DbContext
    {
        public GranjaTechDbContext(DbContextOptions<GranjaTechDbContext> options) : base(options)
        {
        }

        // ADICIONE OS DOIS DBSETS ABAIXO
        public DbSet<Perfil> Perfis { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Granja> Granjas { get; set; }
        public DbSet<Lote> Lotes { get; set; }

        // ADICIONE ESTA LINHA
        public DbSet<TransacaoFinanceira> TransacoesFinanceiras { get; set; }        // ADICIONE O MÉTODO ABAIXO
        // Este método é chamado pelo EF Core para configurar o modelo
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // "Seed" de dados: Popula a tabela de Perfis com dados iniciais
            modelBuilder.Entity<Perfil>().HasData(
                new Perfil { Id = 1, Nome = "Administrador" },
                new Perfil { Id = 2, Nome = "Produtor" },
                new Perfil { Id = 3, Nome = "Financeiro" }
            );
        }
    }
}