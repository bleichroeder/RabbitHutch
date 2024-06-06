using Microsoft.Extensions.Logging;
using RabbitHutch.Consumers;

namespace RabbitHutch.Tests
{
    public partial class ConsumerTests
    {
        private static readonly ILogger Logger = new LoggerFactory().CreateLogger<PublisherTests>();

        /// <summary>
        /// Create a dummy consumer and consume a message.
        /// </summary>
        [Fact]
        public async void CreateDummyConsumer()
        {
            bool messageConsumed = false;

            DummyRabbitConsumer<TestClass> consumer = new(new RabbitConsumerSettings()
            {
                ConnectionString = new Uri("amqp://guest:guest@localhost:5672/"),
                ExchangeName = "TestExchange"
            },
            async (testClass) =>
            {
                messageConsumed = true;

                return await Task.FromResult(messageConsumed);

            }, Logger);

            consumer.Start();

            while(messageConsumed is false)
            {
                await Task.Delay(100);
            }

            Assert.True(messageConsumed);
        }
    }
}