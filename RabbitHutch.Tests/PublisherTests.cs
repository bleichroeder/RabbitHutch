using Microsoft.Extensions.Logging;
using RabbitHutch.Publishers;
using RabbitHutch.Publishers.Interfaces;
using Xunit.Abstractions;

namespace RabbitHutch.Tests
{
    /// <summary>
    /// Tests for the various <see cref="IRabbitPublisher{T}"/> types.
    /// </summary>
    /// <param name="testOutputHelper"></param>
    public class PublisherTests(ITestOutputHelper testOutputHelper)
    {
        private static readonly ILogger Logger = new LoggerFactory().CreateLogger<PublisherTests>();

        private const string CONNECTION_STRING = "amqp://guest:guest@localhost:5672/";
        private const string EXCHANGE_NAME = "TestExchange";

        /// <summary>
        /// Create a dummy publisher and publish a message.
        /// </summary>
        [Fact]
        public async void SimulateMessagePublication_DummyRabbitPublisher()
        {
            RabbitPublisherSettings publisherSettings = new(CONNECTION_STRING, EXCHANGE_NAME);

            DummyRabbitPublisher<TestClass> publisher = new(publisherSettings, Logger);

            testOutputHelper.WriteLine("Publishing new {0} to {1}.", nameof(TestClass), EXCHANGE_NAME);

            Assert.True(await publisher.PublishAsync(new TestClass()));
        }

        /// <summary>
        /// Publish a single message with a <see cref="RabbitPublisher{T}"/>.
        /// </summary>
        [Fact]
        public async void PublishSingleMessage_RabbitPublisher()
        {
            RabbitPublisherSettings publisherSettings = new(CONNECTION_STRING, EXCHANGE_NAME);

            RabbitPublisher<TestClass> publisher = new(publisherSettings, Logger);

            testOutputHelper.WriteLine("Publishing new {0} to {1}.", nameof(TestClass), EXCHANGE_NAME);

            Assert.True(await publisher.PublishAsync(new TestClass()));
        }
    }
}