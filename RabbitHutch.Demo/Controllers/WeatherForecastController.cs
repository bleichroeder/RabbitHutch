using Microsoft.AspNetCore.Mvc;
using RabbitHutch.DependencyInjection;
using RabbitHutch.Publishers.Interfaces;

namespace RabbitHutch.Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(IRabbitPublisherFactory rabbitPublisherFactory) : ControllerBase
    {
        private readonly IRabbitPublisher<WeatherForecast> _rabbitPublisher = rabbitPublisherFactory.GetPublisher<WeatherForecast>();

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<WeatherForecast> Get()
        {
            WeatherForecast forecast = new();

            await _rabbitPublisher.PublishAsync(forecast);

            return forecast;
        }
    }
}