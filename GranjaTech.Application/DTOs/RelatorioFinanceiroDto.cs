using GranjaTech.Domain;
using System.Collections.Generic;

namespace GranjaTech.Application.DTOs
{
    public class RelatorioFinanceiroDto
    {
        public decimal TotalEntradas { get; set; }
        public decimal TotalSaidas { get; set; }
        public decimal Saldo { get; set; }
        public IEnumerable<TransacaoFinanceira> Transacoes { get; set; } = new List<TransacaoFinanceira>();
    }
}
