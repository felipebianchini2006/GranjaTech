
using System.Collections.Generic;


namespace GranjaTech.Domain
{
    // onde vai criar sensores
    public class Sensor
    {
        // indentificação dos sensores
        public int Id { get; set; }

        //medida temperatura e umidade
        public string Tipo { get; set; } = string.Empty;

        //  para identificar o sensor 
        public string IdentificadorUnico { get; set; } = string.Empty;

        // Guarda o número da granja onde o sensor está instalado
        public int GranjaId { get; set; }

        // Permite acessar diretamente as informações da granja a que este sensor pertence
        public Granja Granja { get; set; } = null!;

        // Uma lista para guardar todas as leituras
        public ICollection<LeituraSensor> Leituras { get; set; } = new List<LeituraSensor>();
    }
}