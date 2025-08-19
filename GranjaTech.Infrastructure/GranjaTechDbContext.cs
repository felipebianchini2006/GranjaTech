using GranjaTech.Domain;
using Microsoft.EntityFrameworkCore;

namespace GranjaTech.Infrastructure
{
    public class GranjaTechDbContext : DbContext
    {
        public GranjaTechDbContext(DbContextOptions<GranjaTechDbContext> options) : base(options)
        {
        }

        public DbSet<Perfil> Perfis { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<FinanceiroProdutor> FinanceiroProdutor { get; set; }
        public DbSet<Granja> Granjas { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<TransacaoFinanceira> TransacoesFinanceiras { get; set; }
        public DbSet<LogAuditoria> LogsAuditoria { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da relação muitos-para-muitos
            modelBuilder.Entity<FinanceiroProdutor>(entity =>
            {
                entity.HasKey(fp => new { fp.FinanceiroId, fp.ProdutorId });
                entity.HasOne(fp => fp.Financeiro).WithMany(u => u.ProdutoresGerenciados).HasForeignKey(fp => fp.FinanceiroId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(fp => fp.Produtor).WithMany(u => u.FinanceirosAssociados).HasForeignKey(fp => fp.ProdutorId).OnDelete(DeleteBehavior.Restrict);
            });

            // --- INÍCIO DAS NOVAS CONFIGURAÇÕES DE EXCLUSÃO EM CASCATA ---

            // Regra: Quando um Usuario (Produtor) for deletado, suas Granjas serão deletadas.
            modelBuilder.Entity<Granja>()
                .HasOne(g => g.Usuario)
                .WithMany() // Como não temos uma lista de Granjas em Usuario, deixamos o WithMany vazio
                .HasForeignKey(g => g.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Regra: Quando uma Granja for deletada, seus Lotes serão deletados.
            modelBuilder.Entity<Lote>()
                .HasOne(l => l.Granja)
                .WithMany() // O mesmo aqui, não temos a lista na entidade Granja
                .HasForeignKey(l => l.GranjaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Regra: Quando um Lote for deletado, suas TransacoesFinanceiras serão deletadas.
            modelBuilder.Entity<TransacaoFinanceira>()
                .HasOne(t => t.Lote)
                .WithMany() // O mesmo aqui
                .HasForeignKey(t => t.LoteId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- FIM DAS NOVAS CONFIGURAÇÕES ---

            // Seed de dados dos Perfis
            modelBuilder.Entity<Perfil>().HasData(
                new Perfil { Id = 1, Nome = "Administrador" },
                new Perfil { Id = 2, Nome = "Produtor" },
                new Perfil { Id = 3, Nome = "Financeiro" }
            );
        }
    }
}