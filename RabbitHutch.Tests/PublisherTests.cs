using Microsoft.Extensions.Logging;
using RabbitHutch.Publishers;

namespace RabbitHutch.Tests
{
    public class PublisherTests
    {
        private static readonly ILogger Logger = new LoggerFactory().CreateLogger<PublisherTests>();

        /// <summary>
        /// Create a dummy publisher and publish a message.
        /// </summary>
        [Fact]
        public async void CreateDummyPublisher()
        {
            DummyRabbitPublisher<string> publisher = new(new RabbitPublisherSettings()
            {
                ConnectionString = new Uri("amqp://guest:guest@localhost:5672/"),
                ExchangeName = "TestExchange"
            }, Logger);

            Assert.True(await publisher.PublishAsync("Test Message"));
        }
    }
}