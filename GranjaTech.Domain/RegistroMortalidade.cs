using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GranjaTech.Domain
{
    public class RegistroMortalidade
    {
        public int Id { get; set; }

        [Required]
        public int LoteId { get; set; }
        public Lote Lote { get; set; } = null!;

        [Required]
        public DateTime Data { get; set; }

        /// <summary>Quantidade de aves mortas no evento.</summary>
        [Range(1, int.MaxValue)]
        public int Quantidade { get; set; }

        /// <summary>Motivo resumido: mortalidade, doença, acidente, etc.</summary>
        [StringLength(200)]
        public string? Motivo { get; set; }

        /// <summary>Setor/Baia/Galpão, se aplicável.</summary>
        [StringLength(100)]
        public string? Setor { get; set; }

        /// <summary>Observações adicionais.</summary>
        [StringLength(1000)]
        public string? Observacoes { get; set; }

        /// <summary>Quem registrou.</summary>
        [StringLength(200)]
        public string? ResponsavelRegistro { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // ===================== Aliases de compatibilidade =====================
        // Permitem que seeds e relatórios antigos usem os nomes legados.

        /// <summary>Alias do legado: QuantidadeMortas ↔ Quantidade</summary>
        [NotMapped]
        public int QuantidadeMortas
        {
            get => Quantidade;
            set => Quantidade = value;
        }

        /// <summary>Alias do legado: CausaPrincipal ↔ Motivo</summary>
        [NotMapped]
        public string? CausaPrincipal
        {
            get => Motivo;
            set => Motivo = value;
        }

        // Valor opcional só para seed; se não for definido, calcula automaticamente.
        private int? _idadeDiasSeed;

        /// <summary>Alias do legado: IdadeDias (dias desde a entrada do lote até a Data do evento)</summary>
        [NotMapped]
        public int IdadeDias
        {
            get
            {
                if (_idadeDiasSeed.HasValue) return _idadeDiasSeed.Value;
                if (Lote == null) return 0;
                var dias = (Data.Date - Lote.DataEntrada.Date).TotalDays;
                return dias < 0 ? 0 : (int)Math.Round(dias, MidpointRounding.AwayFromZero);
            }
            set { _idadeDiasSeed = value; }
        }

        /// <summary>Alias do legado: AvesVivas (somente leitura; setter no-op para seeds)</summary>
        [NotMapped]
        public int AvesVivas
        {
            get => Lote?.QuantidadeAvesAtual ?? 0;
            set { /* no-op para compatibilidade com seeds antigos */ }
        }

        /// <summary>Alias do legado: PercentualMortalidadeDia (calculado; setter no-op)</summary>
        [NotMapped]
        public decimal PercentualMortalidadeDia
        {
            get
            {
                var totalIni = Lote?.QuantidadeAvesInicial ?? 0;
                if (totalIni <= 0) return 0m;
                return Math.Round((decimal)Quantidade / totalIni * 100m, 2);
            }
            set { /* no-op para compatibilidade com seeds antigos */ }
        }
    }
}
