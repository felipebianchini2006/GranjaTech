using System.Collections.Generic;

namespace GranjaTech.Domain
{
    public class Sensor
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty; // Ex: "Temperatura", "Humidade"
        public string IdentificadorUnico { get; set; } = string.Empty; // Ex: "TEMP-GRJ001-SALA1"

        // Cada sensor pertence a uma granja
        public int GranjaId { get; set; }
        public Granja Granja { get; set; } = null!;

        // Um sensor pode ter muitas leituras
        public ICollection<LeituraSensor> Leituras { get; set; } = new List<LeituraSensor>();
    }
}
