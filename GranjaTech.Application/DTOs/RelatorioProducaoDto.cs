using GranjaTech.Domain;
using System.Collections.Generic;

namespace GranjaTech.Application.DTOs
{
    public class RelatorioProducaoDto
    {
        public int TotalLotes { get; set; }
        public int TotalAvesInicial { get; set; }
        public IEnumerable<Lote> Lotes { get; set; } = new List<Lote>();
    }
}