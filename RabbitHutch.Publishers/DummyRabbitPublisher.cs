using Microsoft.Extensions.Logging;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Routing;
using RabbitHutch.Core.Serialization;
using RabbitHutch.Publishers.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RabbitHutch.Publishers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitPublisher" /> class.
    /// </summary>
    /// <param name="connectionString">The host.</param>
    /// <param name="exchangeName">Name of the exchange.</param>
    public class DummyRabbitPublisher<T> : RabbitPublisherBase<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DummyRabbitPublisher{T}" /> class.
        /// </summary>
        /// <param name="rabbitConfiguration"></param>
        [SetsRequiredMembers]
        public DummyRabbitPublisher(IRabbitPublisherSettings rabbitConfiguration,
                                    ILogger? logger)
            : this(rabbitConfiguration,
                   ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile(),
                   MessageSerializers.DefaultMessageSerializer<T>(),
                   logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyRabbitPublisher{T}" /> class.
        /// </summary>
        /// <param name="rabbitConfiguration"></param>
        /// <param name="serializer"></param>
        [SetsRequiredMembers]
        public DummyRabbitPublisher(IRabbitPublisherSettings rabbitConfiguration,
                                    IConnectionLifecycleProfile lifecycleProfile,
                                    MessageSerializerDelegate<T> serializer,
                                    ILogger? logger)
            : this(rabbitConfiguration,
                   lifecycleProfile,
                   serializer,
                   RoutingKeyGenerators.DefaultRoutingKeyGenerator<T>(),
                   logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyRabbitPublisher{T}" /> class.
        /// </summary>
        /// <param name="rabbitConfiguration"></param>
        /// <param name="routingKeyGenerator"></param>
        [SetsRequiredMembers]
        public DummyRabbitPublisher(IRabbitPublisherSettings rabbitConfiguration,
                                    RoutingKeyGeneratorDelegate<T> routingKeyGenerator,
                                    ILogger? logger)
            : this(rabbitConfiguration,
                   ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile(),
                   MessageSerializers.DefaultMessageSerializer<T>(),
                   routingKeyGenerator,
                   logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyRabbitPublisher{T}" /> class.
        /// </summary>
        /// <param name="rabbitConfiguration"></param>
        /// <param name="lifecycleProfile"></param>
        /// <param name="serializer"></param>
        /// <param name="routingKeyGenerator"></param>
        [SetsRequiredMembers]
        public DummyRabbitPublisher(IRabbitPublisherSettings rabbitConfiguration,
                                    IConnectionLifecycleProfile lifecycleProfile,
                                    MessageSerializerDelegate<T> serializer,
                                    RoutingKeyGeneratorDelegate<T> routingKeyGenerator,
                                    ILogger? logger)
        {
            RabbitConfiguration = rabbitConfiguration;
            LifecycleProfile = lifecycleProfile;
            Serializer = serializer ?? MessageSerializers.DefaultMessageSerializer<T>();
            RoutingKeyGenerator = routingKeyGenerator ?? RoutingKeyGenerators.DefaultRoutingKeyGenerator<T>();
            Logger = logger;
        }

        /// <summary>
        /// Enqueues the message for publication.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public override bool Publish(T message,
                                     Dictionary<string, object>? headers = null)
            => Publish(message,
                       RoutingKeyGenerator(message),
                       RabbitConfiguration.ContentType,
                       headers);

        /// <summary>
        /// Enqueues the message for publication.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public override async Task<bool> PublishAsync(T message,
                                                      Dictionary<string, object>? headers = null)
            => await PublishAsync(message,
                                  RoutingKeyGenerator(message),
                                  RabbitConfiguration.ContentType,
                                  headers);

        /// <summary>
        /// Enqueues the message for publication.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="routingKey"></param>
        /// <param name="contentType"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public override async Task<bool> PublishAsync(T message,
                                                      string routingKey,
                                                      string contentType,
                                                      Dictionary<string, object>? headers = null)
            => await Task.Run(() => Publish(message,
                                            routingKey,
                                            contentType,
                                            headers));

        /// <summary>
        /// Enqueues the message for publication.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="routingKey"></param>
        /// <param name="contentType"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public override bool Publish(T message,
                                     string routingKey,
                                     string contentType,
                                     Dictionary<string, object>? headers = null)
        {
            string serialized = Serializer(message);
            byte[] bytes = Encoding.UTF8.GetBytes(serialized);
            return Publish(bytes, routingKey, contentType, "UTF-8", headers);
        }

        /// <summary>
        /// Publishes the specified byte array content.
        /// </summary>
        /// <param name="messageBytes">Content of the message.</param>
        /// <param name="routingKey">The routing key.</param>
        private bool Publish(byte[] messageBytes,
                             string routingKey,
                             string contentType,
                             string contentEncoding,
                             Dictionary<string, object>? headers = null)
        {
            Logger?.LogInformation("Simulated publication of {Type} [{RoutingKey}] to [{ExchangeName}]\n" +
                                   "Size: {Size} bytes\n" +
                                   "ContentType: {ContentType}\n" +
                                   "Encoding: {Encoding}\n" +
                                   "{Headers}",
                                   typeof(T).Name,
                                   routingKey,
                                   RabbitConfiguration.ExchangeName,
                                   messageBytes.Length,
                                   contentType,
                                   contentEncoding,
                                   headers is not null ? $"Headers: {string.Join(", ", headers.Select(i => $"{i.Key}: {i.Value}"))}" : string.Empty);

            return true;
        }

        /// <summary>
        /// Hosted service start.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Hosted service stop.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StopAsync(CancellationToken cancellationToken) => Task.Run(Dispose, cancellationToken);
    }
}
