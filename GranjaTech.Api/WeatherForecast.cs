namespace GranjaTech.Api
{
    public class WeatherForecast
    {
        // Guarda a data da previs�o, apenas o dia, m�s e ano.
        public DateOnly Date { get; set; }

        // Guarda a temperatura em graus Celsius.
        public int TemperatureC { get; set; }

        // Calcula e retorna a temperatura em Fahrenheit.
        // N�o � armazenado, � sempre calculado na hora a partir do valor em Celsius.
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        // Um breve resumo do tempo, como "Ensolarado" ou "Chuvoso".
        // O "?" indica que este texto pode ser nulo (n�o preenchido).
        public string? Summary { get; set; }
    }
}