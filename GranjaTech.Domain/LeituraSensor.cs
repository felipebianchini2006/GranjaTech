using System;
//leitura dos sensores do granjatech

namespace GranjaTech.Domain
{ 
    public class LeituraSensor
    {
        public int Id { get; set; }
        public decimal Valor { get; set; }
        public DateTime Timestamp { get; set; } // Data e hora da leitura

        // Cada leitura pertence a um sensor
        public int SensorId { get; set; }
        public Sensor Sensor { get; set; } = null!;
    }
}
