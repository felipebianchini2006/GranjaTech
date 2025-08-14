using System;
using System.Collections.Generic;

namespace GranjaTech.Domain
{
    public class Lote
    {
        public int Id { get; set; }
        public string Identificador { get; set; } = string.Empty;
        public DateTime DataEntrada { get; set; }
        public DateTime? DataSaida { get; set; }
        public int QuantidadeAvesInicial { get; set; }

        // Propriedades de relacionamento com a Granja
        // Esta é a chave estrangeira para a tabela Granja.
        public int GranjaId { get; set; }

        // Esta é a "propriedade de navegação", que permite ao EF Core
        // carregar os dados da Granja relacionada a este Lote.
        public Granja Granja { get; set; } = null!;
    }
}