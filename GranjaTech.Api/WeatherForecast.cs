namespace GranjaTech.Api
{
    public class WeatherForecast
    {
        // Guarda a data da previsão, apenas o dia, mês e ano.
        public DateOnly Date { get; set; }

        // Guarda a temperatura em graus Celsius.
        public int TemperatureC { get; set; }

        // Calcula e retorna a temperatura em Fahrenheit.
        // Não é armazenado, é sempre calculado na hora a partir do valor em Celsius.
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        // Um breve resumo do tempo, como "Ensolarado" ou "Chuvoso".
        // O "?" indica que este texto pode ser nulo (não preenchido).
        public string? Summary { get; set; }
    }
}