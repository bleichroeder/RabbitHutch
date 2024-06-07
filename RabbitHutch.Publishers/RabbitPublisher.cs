using Microsoft.Extensions.Logging;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Routing;
using RabbitHutch.Core.Serialization;
using RabbitHutch.Publishers.Interfaces;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RabbitHutch.Publishers
{
    public class RabbitPublisher<T> : RabbitPublisherBase<T>, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitPublisher" /> class.
        /// </summary>
        /// <param name="rabbitConfiguration"></param>
        [SetsRequiredMembers]
        public RabbitPublisher(IRabbitPublisherSettings rabbitConfiguration,
                               ILogger? logger)
            : this(rabbitConfiguration,
                   ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile(),
                   MessageSerializers.DefaultMessageSerializer<T>(),
                   logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitPublisher" /> class.
        /// </summary>
        /// <param name="rabbitConfiguration"></param>
        /// <param name="serializer"></param>
        [SetsRequiredMembers]
        public RabbitPublisher(IRabbitPublisherSettings rabbitConfiguration,
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
        /// Initializes a new instance of the <see cref="RabbitPublisher" /> class.
        /// </summary>
        /// <param name="rabbitConfiguration"></param>
        /// <param name="routingKeyGenerator"></param>
        [SetsRequiredMembers]
        public RabbitPublisher(IRabbitPublisherSettings rabbitConfiguration,
                               RoutingKeyGeneratorDelegate<T> routingKeyGenerator,
                               ILogger? logger)
            : this(rabbitConfiguration,
                   ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile(),
                   MessageSerializers.DefaultMessageSerializer<T>(),
                   routingKeyGenerator,
                   logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitPublisher" /> class.
        /// </summary>
        /// <param name="rabbitConfiguration"></param>
        /// <param name="lifecycleProfile"></param>
        /// <param name="serializer"></param>
        /// <param name="routingKeyGenerator"></param>
        [SetsRequiredMembers]
        public RabbitPublisher(IRabbitPublisherSettings rabbitConfiguration,
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
        public override bool Publish(T message, Dictionary<string, object>? headers = null)
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
        public override async Task<bool> PublishAsync(T message, Dictionary<string, object>? headers = null)
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
            => PublishAsync(Encoding.UTF8.GetBytes(Serializer(message)),
                            routingKey,
                            contentType,
                            Encoding.UTF8.WebName,
                            headers).GetAwaiter().GetResult();

        /// <summary>
        /// Publishes the specified byte array content.
        /// </summary>
        /// <param name="messageBytes">Content of the message.</param>
        /// <param name="routingKey">The routing key.</param>
        private async Task<bool> PublishAsync(byte[] messageBytes,
                                              string routingKey,
                                              string contentType,
                                              string contentEncoding,
                                              Dictionary<string, object>? headers = null)
        {
            _publishSuccess = false;

            // Check that the channel and connection are open.
            if (await InitializeRabbitAsync() is false)
            {
                Logger?.LogWarning("WARNING: Connection to {_connection?.Endpoint.HostName} is not active.", _connection?.Endpoint.HostName);
                return false;
            }

            Logger?.LogDebug("Connection to {_connection?.Endpoint.HostName} is open.", _connection?.Endpoint.HostName);

            try
            {
                IBasicProperties? properties = _model?.CreateBasicProperties();

                if (properties is not null)
                {
                    properties.ContentEncoding = contentEncoding;
                    properties.ContentType = contentType;

                    if (headers is not null)
                        properties.Headers = headers;
                }

                _model.BasicPublish(RabbitConfiguration.ExchangeName,
                                      routingKey,
                                      properties,
                                      messageBytes);

                // If ACKS are enabled...
                // Wait for confirms will throw an exception if not ack'd.
                if (RabbitConfiguration.EnableAcks)
                {
                    _model?.WaitForConfirmsOrDie();
                }

                // If we've made it here we published.
                _publishSuccess = true;

                Logger?.LogDebug("Published {typeof(T).Name} [{routingKey}] to [{RabbitConfiguration.ExchangeName}]", typeof(T).Name, routingKey, RabbitConfiguration.ExchangeName);
            }
            catch (Exception ex)
            {
                Logger?.LogError("PUBLISHING ERROR: {ex}", ex);
            }

            return _publishSuccess;
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
