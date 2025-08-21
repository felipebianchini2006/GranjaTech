namespace GranjaTech.Application.DTOs
{
    public class CreateSensorDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string IdentificadorUnico { get; set; } = string.Empty;
        public int GranjaId { get; set; }
    }
}
