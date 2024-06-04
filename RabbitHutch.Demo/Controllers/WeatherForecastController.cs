using Microsoft.AspNetCore.Mvc;
using RabbitHutch.DependencyInjection;
using RabbitHutch.Publishers.Interfaces;

namespace RabbitHutch.Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(ILogger<WeatherForecastController> logger, IRabbitPublisherFactory rabbitPublisherFactory) : ControllerBase
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

        private readonly ILogger<WeatherForecastController> _logger = logger;

        private readonly IRabbitPublisher<WeatherForecast> _rabbitPublisher = rabbitPublisherFactory.GetPublisher<WeatherForecast>();

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            List<WeatherForecast> forecasts = [];

            foreach (int i in Enumerable.Range(1, 5))
            {
                WeatherForecast weatherForecast = new()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                };

                forecasts.Add(weatherForecast);

                await _rabbitPublisher.PublishAsync(weatherForecast);
            }

            return forecasts.ToArray();
        }
    }
}