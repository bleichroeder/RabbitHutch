using RabbitHutch.Consumers;
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

            // Register a CancellationTokenSource with the DI container.
            // The factories will pass the source token to the consumers and publishers.
            // This is not required.
            CancellationTokenSource cancellationTokenSource = new();
            builder.Services.AddSingleton(cancellationTokenSource);

            // Here we configure our Hutch.
            // We'll add a single dummy publisher and register it with the DI container.
            // Next we'll add a single dummy consumer and register it with the DI container.
            builder.Services.BuildRabbitHutch(hutchBuilder =>
            {
                // Add a RabbitPublisher.
                hutchBuilder.AddDummyPublisher<WeatherForecast>(new RabbitPublisherSettings()
                {
                    ConnectionString = new Uri(CONNECTION_STRING),
                    ExchangeName = EXCHANGE_NAME,
                    ContentType = MediaTypeNames.Application.Json,
                    AutomaticRecovery = true,
                    EnableAcks = true

                }, publisherConfigurator =>
                {
                    publisherConfigurator.WithConnectionLifecycleProfile(ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile());
                    publisherConfigurator.WithRoutingKeyFormatter(forecast => nameof(forecast));
                    publisherConfigurator.WithSerializer(forecast => forecast.Serialize());
                    publisherConfigurator.WithName("MyCustomDummyPublisher");
                    publisherConfigurator.RegisterAsHostedService();
                });

                // Add a RabbitConsumer.
                hutchBuilder.AddDummyConsumer<WeatherForecast>(new RabbitConsumerSettings()
                {
                    ConnectionString = new Uri(CONNECTION_STRING),
                    ExchangeName = EXCHANGE_NAME,
                    QueueName = "WeatherForecastQueue",
                    NackOnFalse = true,
                    RequeueOnNack = false,
                    AutomaticRecovery = true,
                    RoutingKeys = ["#"],
                    PrefetchCount = 1

                }, consumerConfigurator =>
                {
                    consumerConfigurator.WithConnectionLifecycleProfile(ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile());
                    consumerConfigurator.WithDeserializer(bytes => WeatherForecast.Deserialize(bytes));
                    consumerConfigurator.WithNewMessageDelegate(LogIfFreezing);
                    consumerConfigurator.WithName("MyCustomDummyConsumer");
                    consumerConfigurator.RegisterAsHostedService();
                });
            });

            WebApplication? app = builder.Build();

            // Register a callback to cancel the token source when the application is stopping.
            app.Lifetime.ApplicationStopping.Register(() =>
            {
                cancellationTokenSource.Cancel();
            });
            Console.CancelKeyPress += (s, e) =>
            {
                cancellationTokenSource.Cancel();

                e.Cancel = true;
            };

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

        /// <summary>
        /// A method to call when a message is consumed.
        /// </summary>
        /// <param name="forecast"></param>
        /// <returns></returns>
        private static async Task<bool> LogIfFreezing(WeatherForecast forecast)
        {
            bool isFreezing = forecast.TemperatureC <= 0;

            if (isFreezing)
            {
                Console.WriteLine($"It sure is cold out there! [{forecast.TemperatureF}f]");
            }

            return await Task.FromResult(true);
        }
    }
}