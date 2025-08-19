using System;

namespace GranjaTech.Domain
{
    public class Lote
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty; // ADICIONE ESTA LINHA
        public string Identificador { get; set; } = string.Empty;
        public DateTime DataEntrada { get; set; }
        public DateTime? DataSaida { get; set; }
        public int QuantidadeAvesInicial { get; set; }

        public int GranjaId { get; set; }
        public Granja Granja { get; set; } = null!;
    }
}