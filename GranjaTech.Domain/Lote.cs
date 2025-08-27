using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GranjaTech.Domain
{
    public class Lote
    {
        public int Id { get; set; }
        
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Identificador { get; set; } = string.Empty;
        
        /// <summary>
        /// Data de alojamento das aves
        /// </summary>
        [Required]
        public DateTime DataEntrada { get; set; }
        
        /// <summary>
        /// Data prevista para o abate
        /// </summary>
        public DateTime? DataAbatePrevista { get; set; }
        
        /// <summary>
        /// Data real de saída/abate
        /// </summary>
        public DateTime? DataSaida { get; set; }
        
        /// <summary>
        /// Quantidade inicial de aves alojadas
        /// </summary>
        [Range(1, 100000, ErrorMessage = "Quantidade deve estar entre 1 e 100.000 aves")]
        public int QuantidadeAvesInicial { get; set; }
        
        /// <summary>
        /// Quantidade atual de aves vivas
        /// </summary>
        public int QuantidadeAvesAtual { get; set; }
        
        /// <summary>
        /// Área do galpão em m²
        /// </summary>
        [Range(0.1, 10000, ErrorMessage = "Área deve estar entre 0.1 e 10.000 m²")]
        public decimal? AreaGalpao { get; set; }
        
        /// <summary>
        /// Densidade atual (aves/m²)
        /// </summary>
        public decimal DensidadeAtual => AreaGalpao.HasValue && AreaGalpao > 0 ? QuantidadeAvesAtual / AreaGalpao.Value : 0;
        
        /// <summary>
        /// Linhagem das aves
        /// </summary>
        [StringLength(100)]
        public string? Linhagem { get; set; }
        
        /// <summary>
        /// Origem dos pintinhos (granja matriz)
        /// </summary>
        [StringLength(200)]
        public string? OrigemPintinhos { get; set; }
        
        /// <summary>
        /// Status do lote: Ativo, Abatido, Descartado
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Ativo";
        
        /// <summary>
        /// Idade atual do lote em dias
        /// </summary>
        public int IdadeAtualDias => (int)(DateTime.Now.Date - DataEntrada.Date).TotalDays;
        
        /// <summary>
        /// Mortalidade total acumulada
        /// </summary>
        public int MortalidadeTotalAcumulada => QuantidadeAvesInicial - QuantidadeAvesAtual;
        
        /// <summary>
        /// Percentual de mortalidade acumulada
        /// </summary>
        public decimal PercentualMortalidadeAcumulada => QuantidadeAvesInicial > 0 
            ? (decimal)MortalidadeTotalAcumulada / QuantidadeAvesInicial * 100 : 0;
        
        /// <summary>
        /// Viabilidade (%) = 100 - Mortalidade
        /// </summary>
        public decimal Viabilidade => 100 - PercentualMortalidadeAcumulada;
        
        /// <summary>
        /// Observações gerais do lote
        /// </summary>
        [StringLength(1000)]
        public string? Observacoes { get; set; }

        [Required]
        public int GranjaId { get; set; }
        public Granja Granja { get; set; } = null!;
        
        // Relacionamentos com as novas entidades
        public ICollection<ConsumoRacao> ConsumosRacao { get; set; } = new List<ConsumoRacao>();
        public ICollection<ConsumoAgua> ConsumosAgua { get; set; } = new List<ConsumoAgua>();
        public ICollection<PesagemSemanal> PesagensSemanais { get; set; } = new List<PesagemSemanal>();
        public ICollection<EventoSanitario> EventosSanitarios { get; set; } = new List<EventoSanitario>();
        public ICollection<QualidadeAr> MedicoesQualidadeAr { get; set; } = new List<QualidadeAr>();
        public ICollection<RegistroMortalidade> RegistrosMortalidade { get; set; } = new List<RegistroMortalidade>();
        public ICollection<RegistroAbate> RegistrosAbate { get; set; } = new List<RegistroAbate>();
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataAtualizacao { get; set; }
        
        /// <summary>
        /// Cálculo da Conversão Alimentar Acumulada
        /// CA = Total de Ração Consumida (kg) / Ganho de Peso Total (kg)
        /// </summary>
        public decimal CalcularConversaoAlimentar()
        {
            var totalRacaoKg = ConsumosRacao.Sum(c => c.QuantidadeKg);
            var pesagemMaisRecente = PesagensSemanais.OrderByDescending(p => p.DataPesagem).FirstOrDefault();
            
            if (pesagemMaisRecente == null || totalRacaoKg == 0) return 0;
            
            var ganhoTotalPorAve = pesagemMaisRecente.PesoMedioGramas - 45; // 45g peso inicial médio
            var ganhoTotalLoteKg = (ganhoTotalPorAve * QuantidadeAvesAtual) / 1000;
            
            return ganhoTotalLoteKg > 0 ? totalRacaoKg / ganhoTotalLoteKg : 0;
        }
        
        /// <summary>
        /// Cálculo do Índice de Eficiência Produtiva (IEP)
        /// IEP = (Ganho de Peso x Viabilidade x 100) ÷ (CA x Idade)
        /// </summary>
        public decimal CalcularIEP()
        {
            var pesagemMaisRecente = PesagensSemanais.OrderByDescending(p => p.DataPesagem).FirstOrDefault();
            
            if (pesagemMaisRecente == null || IdadeAtualDias == 0) return 0;
            
            var ganhoPeso = (pesagemMaisRecente.PesoMedioGramas - 45) / 1000m; // em kg
            var conversaoAlimentar = CalcularConversaoAlimentar();
            
            if (conversaoAlimentar == 0) return 0;
            
            return (ganhoPeso * Viabilidade * 100) / (conversaoAlimentar * IdadeAtualDias);
        }
    }
}