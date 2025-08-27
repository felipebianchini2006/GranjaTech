using System;
using System.Collections.Generic;

namespace GranjaTech.Application.DTOs
{
    public class CurvasCrescimentoDto
    {
        public int LoteId { get; set; }
        public string LoteIdentificador { get; set; } = string.Empty;
        public int IdadeAtualDias { get; set; }
        public List<PontoCurvaDto> CurvaPeso { get; set; } = new List<PontoCurvaDto>();
        public List<PontoCurvaDto> CurvaConsumoRacao { get; set; } = new List<PontoCurvaDto>();
        public List<PontoCurvaDto> CurvaConsumoAgua { get; set; } = new List<PontoCurvaDto>();
        public List<PontoCurvaDto> CurvaMortalidade { get; set; } = new List<PontoCurvaDto>();
        public List<PontoCurvaDto> CurvaGanhoMedioDiario { get; set; } = new List<PontoCurvaDto>();
    }

    public class PontoCurvaDto
    {
        public int Dia { get; set; }
        public int Semana { get; set; }
        public decimal Valor { get; set; }
        public decimal? ValorPadrao { get; set; } // Para comparação com padrões da indústria
        public DateTime Data { get; set; }
    }
}
