using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.DependencyInjection;
using RabbitHutch.Publishers;
using System.Net.Mime;

namespace RabbitHutch.Demo
{
    public class Program
    {
        // These are essentially useless because we're using a dummy publisher.
        private const string CONNECTION_STRING = "amqp://admin:admin@localhost:5672/%2F";
        private const string EXCHANGE_NAME = "WeatherForecastExchange";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddLogging();

            // Here we configure our Hutch.
            // We'll add a single dummy publisher and register it with the DI container.
            builder.Services.BuildRabbitHutch(hutchBuilder =>
            {
                hutchBuilder.AddDummyPublisher<WeatherForecast>(new RabbitPublisherSettings()
                {
                    ConnectionString = new Uri(CONNECTION_STRING),
                    ExchangeName = EXCHANGE_NAME,
                    ContentType = MediaTypeNames.Application.Json

                }, rabbitConfigurator =>
                {
                    rabbitConfigurator.WithConnectionLifecycleProfile(ConnectionLifecycleProfiles.DefaultPublisherConnectionLifecycleProfile());
                    rabbitConfigurator.WithRoutingKeyFormatter(forecast => nameof(forecast));
                    rabbitConfigurator.WithSerializer(forecast => forecast.Serialize());
                    rabbitConfigurator.WithName("MyCustomDummyPublisher");
                });
            });

            WebApplication? app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}