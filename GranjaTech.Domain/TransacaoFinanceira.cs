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

        // Chave estrangeira opcional para Lote
        // A interrogação '?' torna o campo anulável
        public int? LoteId { get; set; }
        public Lote? Lote { get; set; }
    }
}