using Microsoft.Extensions.Logging;
using RabbitHutch.Consumers;
using RabbitHutch.Consumers.Interfaces;
using RabbitHutch.Publishers;
using Xunit.Abstractions;

namespace RabbitHutch.Tests
{
    /// <summary>
    /// Tests for the various <see cref="IRabbitConsumer{T}"/> types.
    /// </summary>
    /// <param name="outputHelper"></param>
    public partial class ConsumerTests(ITestOutputHelper outputHelper)
    {
        private static readonly ILogger Logger = new LoggerFactory().CreateLogger<PublisherTests>();

        private const string CONNECTION_STRING = "amqp://guest:guest@localhost:5672/";
        private const string MANAGEMENT_BASE_URI = "http://guest:guest@localhost:15672/api/";
        private const string EXCHANGE_NAME = "TestExchange";
        private const string QUEUE_NAME = "TestQueue";

        /// <summary>
        /// Create a dummy consumer and consume a message.
        /// </summary>
        [Fact]
        public async void SimulateMessageConsumption_DummyRabbitPublisher()
        {
            bool messageConsumed = false;

            RabbitConsumerSettings consumerSettings = new(CONNECTION_STRING, EXCHANGE_NAME, QUEUE_NAME)
            {
                ManagementBaseUri = new Uri(MANAGEMENT_BASE_URI)
            };

            DummyRabbitConsumer<TestClass> consumer = new(consumerSettings, async (testClass) =>
            {
                messageConsumed = true;

                outputHelper.WriteLine("Consumed a single {0}; Name: {1} ID: {2}", nameof(TestClass), testClass.Name, testClass.Id);

                return await Task.FromResult(messageConsumed);

            }, Logger);

            consumer.Start();

            while (messageConsumed is false)
            {
                await Task.Delay(100);
            }

            Assert.True(messageConsumed);
        }

        /// <summary>
        /// Publish a message with a <see cref="RabbitPublisher{T}"/> and consume it with a <see cref="SingleFetchRabbitConsumer{T}"/>.
        /// </summary>
        [Fact]
        public async void PublishAndConsumeSingleMessage_RabbitPublisher_SingleFetchConsumer()
        {
            string name = "Porter Robinson";

            RabbitPublisherSettings publisherSettings = new(CONNECTION_STRING, EXCHANGE_NAME);

            // Create a publisher and publish a message.
            RabbitPublisher<TestClass> publisher = new(publisherSettings, Logger);

            bool messagePublished = await publisher.PublishAsync(new TestClass()
            {
                Name = name,
                Id = Guid.NewGuid()
            });

            Assert.True(messagePublished);

            RabbitConsumerSettings consumerSettings = new(CONNECTION_STRING, EXCHANGE_NAME, QUEUE_NAME)
            {
                ManagementBaseUri = new Uri(MANAGEMENT_BASE_URI)
            };

            // Create a single fetch consumer and consume the message.
            SingleFetchRabbitConsumer<TestClass> consumer = new(consumerSettings, async (testClass) =>
            {
                outputHelper.WriteLine("Consumed a single {0}; Name: {1} ID: {2}", nameof(TestClass), testClass.Name, testClass.Id);

                return await Task.FromResult(testClass.Name == name);

            }, Logger);

            bool messageConsumed = await consumer.FetchMessageAsync();

            Assert.True(messageConsumed);
        }
    }
}