namespace GranjaTech.Application.DTOs.Relatorios;

public sealed class ConsumoResumoDto
{
    public DateTime Data { get; set; }
    public double RacaoKg { get; set; }
    public double AguaLitros { get; set; }
    public int AvesVivas { get; set; }
}

public sealed class PesagemResumoDto
{
    public DateTime Data { get; set; }
    public double PesoMedioKg { get; set; }
    public int Amostra { get; set; }
}

public sealed class SanitarioResumoDto
{
    public DateTime Data { get; set; }
    public string TipoEvento { get; set; } = "";
    public string Produto { get; set; } = "";
    public string? Via { get; set; }
}

// ATENÇÃO: sensor é genérico no seu domínio (Valor decimal).
public sealed class SensorResumoDto
{
    public DateTime Data { get; set; }     // LeituraSensor.Timestamp
    public string Tipo { get; set; } = ""; // Sensor.Tipo: "Temperatura", "Humidade", etc.
    public double Valor { get; set; }      // LeituraSensor.Valor -> double
}

public sealed class GeralReportDto
{
    public int GranjaId { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }

    public List<ConsumoResumoDto> Consumo { get; set; } = new();
    public List<PesagemResumoDto> Pesagens { get; set; } = new();
    public List<SanitarioResumoDto> Sanitario { get; set; } = new();
    public List<SensorResumoDto> Sensores { get; set; } = new();
}
