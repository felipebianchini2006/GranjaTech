using System.Collections.Generic;

namespace GranjaTech.Application.DTOs
{
    public class RelatorioFinanceiroSimplificadoDto
    {
        public decimal TotalEntradas { get; set; }
        public decimal TotalSaidas { get; set; }
        public decimal Saldo { get; set; }
        public List<TransacaoSimplificadaDto> Transacoes { get; set; } = new List<TransacaoSimplificadaDto>();
    }
}
