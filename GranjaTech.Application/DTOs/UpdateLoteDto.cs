using System;

namespace GranjaTech.Application.DTOs
{
    public class UpdateLoteDto
    {
        // Note que não temos o objeto 'Granja' aqui, apenas o 'GranjaId'
        public string Identificador { get; set; } = string.Empty;
        public int QuantidadeAvesInicial { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime? DataSaida { get; set; }
        public int GranjaId { get; set; }
    }
}