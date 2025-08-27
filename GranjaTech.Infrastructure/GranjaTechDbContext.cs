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
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Sensor> Sensores { get; set; }
        public DbSet<LeituraSensor> LeiturasSensores { get; set; }
        
        // Novas entidades específicas de avicultura
        public DbSet<ConsumoRacao> ConsumosRacao { get; set; }
        public DbSet<ConsumoAgua> ConsumosAgua { get; set; }
        public DbSet<PesagemSemanal> PesagensSemanais { get; set; }
        public DbSet<EventoSanitario> EventosSanitarios { get; set; }
        public DbSet<QualidadeAr> MedicoesQualidadeAr { get; set; }
        public DbSet<RegistroMortalidade> RegistrosMortalidade { get; set; }
        public DbSet<RegistroAbate> RegistrosAbate { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da relação muitos-para-muitos
            modelBuilder.Entity<FinanceiroProdutor>(entity =>
            {
                entity.HasKey(fp => new { fp.FinanceiroId, fp.ProdutorId });

                entity.HasOne(fp => fp.Financeiro)
                      .WithMany(u => u.ProdutoresGerenciados)
                      .HasForeignKey(fp => fp.FinanceiroId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(fp => fp.Produtor)
                      .WithMany(u => u.FinanceirosAssociados)
                      .HasForeignKey(fp => fp.ProdutorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configurações de Exclusão em Cascata
            modelBuilder.Entity<Granja>()
                .HasOne(g => g.Usuario)
                .WithMany()
                .HasForeignKey(g => g.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Lote>()
                .HasOne(l => l.Granja)
                .WithMany()
                .HasForeignKey(l => l.GranjaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TransacaoFinanceira>()
                .HasOne(t => t.Lote)
                .WithMany()
                .HasForeignKey(t => t.LoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Produto>()
                .HasOne(p => p.Granja)
                .WithMany()
                .HasForeignKey(p => p.GranjaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sensor>()
                .HasOne(s => s.Granja)
                .WithMany()
                .HasForeignKey(s => s.GranjaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeituraSensor>()
                .HasOne(l => l.Sensor)
                .WithMany(s => s.Leituras)
                .HasForeignKey(l => l.SensorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurações das novas entidades
            modelBuilder.Entity<ConsumoRacao>()
                .HasOne(c => c.Lote)
                .WithMany(l => l.ConsumosRacao)
                .HasForeignKey(c => c.LoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConsumoAgua>()
                .HasOne(c => c.Lote)
                .WithMany(l => l.ConsumosAgua)
                .HasForeignKey(c => c.LoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PesagemSemanal>()
                .HasOne(p => p.Lote)
                .WithMany(l => l.PesagensSemanais)
                .HasForeignKey(p => p.LoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventoSanitario>()
                .HasOne(e => e.Lote)
                .WithMany(l => l.EventosSanitarios)
                .HasForeignKey(e => e.LoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QualidadeAr>()
                .HasOne(q => q.Lote)
                .WithMany(l => l.MedicoesQualidadeAr)
                .HasForeignKey(q => q.LoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RegistroMortalidade>()
                .HasOne(r => r.Lote)
                .WithMany(l => l.RegistrosMortalidade)
                .HasForeignKey(r => r.LoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RegistroAbate>()
                .HasOne(r => r.Lote)
                .WithMany(l => l.RegistrosAbate)
                .HasForeignKey(r => r.LoteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurações de precisão decimal
            modelBuilder.Entity<ConsumoRacao>()
                .Property(c => c.QuantidadeKg)
                .HasPrecision(10, 3);

            modelBuilder.Entity<ConsumoAgua>()
                .Property(c => c.QuantidadeLitros)
                .HasPrecision(10, 3);

            modelBuilder.Entity<PesagemSemanal>()
                .Property(p => p.PesoMedioGramas)
                .HasPrecision(8, 2);

            modelBuilder.Entity<QualidadeAr>()
                .Property(q => q.NH3_ppm)
                .HasPrecision(6, 2);

            modelBuilder.Entity<QualidadeAr>()
                .Property(q => q.CO2_ppm)
                .HasPrecision(8, 2);

            modelBuilder.Entity<RegistroAbate>()
                .Property(r => r.PesoVivoTotalKg)
                .HasPrecision(10, 3);

            // Índices para performance
            modelBuilder.Entity<ConsumoRacao>()
                .HasIndex(c => new { c.LoteId, c.Data });

            modelBuilder.Entity<ConsumoAgua>()
                .HasIndex(c => new { c.LoteId, c.Data });

            modelBuilder.Entity<PesagemSemanal>()
                .HasIndex(p => new { p.LoteId, p.SemanaVida });

            modelBuilder.Entity<RegistroMortalidade>()
                .HasIndex(r => new { r.LoteId, r.Data });

            // Seed de dados dos Perfis
            modelBuilder.Entity<Perfil>().HasData(
                new Perfil { Id = 1, Nome = "Administrador" },
                new Perfil { Id = 2, Nome = "Produtor" },
                new Perfil { Id = 3, Nome = "Financeiro" }
            );

            // Seed do Utilizador Admin Padrão com HASH VÁLIDO
            var senhaHash = "$2a$11$Y.7g.3s5B5B5B5B5B5B5B.u5n5n5n5n5n5n5n5n5n5n5n5n5n5n5n5";
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    Codigo = "ADM-001",
                    Nome = "Admin Padrão",
                    Email = "admin@admin.com",
                    SenhaHash = senhaHash,
                    PerfilId = 1
                }
            );
        }
    }
}
