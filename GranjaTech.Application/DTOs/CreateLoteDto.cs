using System;

namespace GranjaTech.Application.DTOs
{
    public class CreateLoteDto
    {
        public string Identificador { get; set; }
        public int QuantidadeAvesInicial { get; set; }
        public DateTime DataEntrada { get; set; }
        public int GranjaId { get; set; }
    }
}