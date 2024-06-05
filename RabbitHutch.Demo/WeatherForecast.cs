using System.Text.Json;

namespace RabbitHutch.Demo
{
    public class WeatherForecast
    {
        private static readonly string[] Summaries =
        [
            "Freezing",
            "Bracing",
            "Chilly",
            "Cool",
            "Mild",
            "Warm",
            "Balmy",
            "Hot",
            "Sweltering",
            "Scorching"
        ];

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Gets or sets the temperature in Celsius.
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// Gets the temperature in Fahrenheit.
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Serializes the weather forecast.
        /// </summary>
        /// <returns></returns>
        public string Serialize() => JsonSerializer.Serialize(this);

        /// <summary>
        /// Deserializes the weather forecast from a byte array.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static WeatherForecast Deserialize(byte[] bytes) => JsonSerializer.Deserialize<WeatherForecast>(bytes)!;

        /// <summary>
        /// Creates a random weather forecast.
        /// </summary>
        public WeatherForecast()
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(Random.Shared.Next(1, 7)));
            TemperatureC = Random.Shared.Next(-20, 55);
            Summary = Summaries[Random.Shared.Next(Summaries.Length)];
        }
    }
}