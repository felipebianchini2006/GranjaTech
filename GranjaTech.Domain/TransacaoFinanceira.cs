using System;

namespace GranjaTech.Domain
{
    public class TransacaoFinanceira
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Entrada" ou "Saida"
        public DateTime Data { get; set; }

        // ADICIONE AS PROPRIEDADES ABAIXO
        public DateTime TimestampCriacao { get; set; } // Guarda a data/hora exata da criação
        public int UsuarioId { get; set; } // Guarda quem criou
        public Usuario Usuario { get; set; } = null!;

        public int? LoteId { get; set; }
        public Lote? Lote { get; set; }
    }
}