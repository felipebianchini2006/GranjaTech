using System;
using System.ComponentModel.DataAnnotations;

namespace GranjaTech.Domain
{
    public class QualidadeAr
    {
        public int Id { get; set; }
        
        [Required]
        public int LoteId { get; set; }
        public Lote Lote { get; set; } = null!;
        
        [Required]
        public DateTime DataHora { get; set; }
        
        /// <summary>
        /// Concentração de Amônia (NH3) em ppm
        /// </summary>
        [Range(0, 100, ErrorMessage = "NH3 deve estar entre 0 e 100 ppm")]
        public decimal? NH3_ppm { get; set; }
        
        /// <summary>
        /// Concentração de Dióxido de Carbono (CO2) em ppm
        /// </summary>
        [Range(0, 10000, ErrorMessage = "CO2 deve estar entre 0 e 10000 ppm")]
        public decimal? CO2_ppm { get; set; }
        
        /// <summary>
        /// Concentração de Oxigênio (O2) em %
        /// </summary>
        [Range(0, 25, ErrorMessage = "O2 deve estar entre 0 e 25%")]
        public decimal? O2_percentual { get; set; }
        
        /// <summary>
        /// Velocidade do ar em m/s
        /// </summary>
        [Range(0, 10, ErrorMessage = "Velocidade do ar deve estar entre 0 e 10 m/s")]
        public decimal? VelocidadeAr_ms { get; set; }
        
        /// <summary>
        /// Luminosidade em lux
        /// </summary>
        [Range(0, 1000, ErrorMessage = "Luminosidade deve estar entre 0 e 1000 lux")]
        public decimal? Luminosidade_lux { get; set; }
        
        /// <summary>
        /// Temperatura do ar em °C
        /// </summary>
        [Range(-10, 50, ErrorMessage = "Temperatura deve estar entre -10 e 50°C")]
        public decimal? TemperaturaAr { get; set; }
        
        /// <summary>
        /// Umidade relativa do ar em %
        /// </summary>
        [Range(0, 100, ErrorMessage = "Umidade deve estar entre 0 e 100%")]
        public decimal? UmidadeRelativa { get; set; }
        
        /// <summary>
        /// Local da medição no galpão
        /// </summary>
        [StringLength(100)]
        public string? LocalMedicao { get; set; }
        
        /// <summary>
        /// Equipamento utilizado para medição
        /// </summary>
        [StringLength(200)]
        public string? EquipamentoMedicao { get; set; }
        
        /// <summary>
        /// Observações sobre as condições
        /// </summary>
        [StringLength(500)]
        public string? Observacoes { get; set; }
        
        /// <summary>
        /// Indica se os valores estão dentro dos parâmetros aceitáveis
        /// </summary>
        public bool ParametrosAceitaveis => 
            (NH3_ppm == null || NH3_ppm <= 25) &&
            (CO2_ppm == null || CO2_ppm <= 3000) &&
            (O2_percentual == null || O2_percentual >= 19.5m) &&
            (TemperaturaAr == null || (TemperaturaAr >= 18 && TemperaturaAr <= 33)) &&
            (UmidadeRelativa == null || (UmidadeRelativa >= 50 && UmidadeRelativa <= 70));
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
