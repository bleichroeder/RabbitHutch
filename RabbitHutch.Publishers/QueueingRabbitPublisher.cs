using Microsoft.Extensions.Logging;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Routing;
using RabbitHutch.Core.Serialization;
using RabbitHutch.Publishers.Interfaces;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RabbitHutch.Publishers
{
    /// <summary>
    /// A queueing rabbit publisher.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueueingRabbitPublisher<T> : RabbitPublisherBase<T>, IDisposable
    {
        private bool _isRunning;
        private bool _stop = true;
        private bool _stopNow = false;

        private readonly Thread _queueManagerThread;
        private readonly static ConcurrentQueue<IQueueingRabbitPublisherItem<T>> _queue = new();
        private readonly object _queueLock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitPublisher" /> class.
        /// </summary>
        /// <param name="rabbitPublisherSettings"></param>
        [SetsRequiredMembers]
        public QueueingRabbitPublisher(IQueueingRabbitPublisherSettings rabbitPublisherSettings,
                                       ILogger? logger)
            : this(rabbitPublisherSettings,
                   ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile(),
                   MessageSerializers.DefaultMessageSerializer<T>(),
                   RoutingKeyGenerators.DefaultRoutingKeyGenerator<T>(),
                   logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitPublisher" /> class.
        /// </summary>
        /// <param name="rabbitPublisherSettings"></param>
        /// <param name="lifecycleProfile"></param>
        [SetsRequiredMembers]
        public QueueingRabbitPublisher(IQueueingRabbitPublisherSettings rabbitPublisherSettings,
                                       IConnectionLifecycleProfile lifecycleProfile,
                                       ILogger? logger)
            : this(rabbitPublisherSettings,
                   lifecycleProfile,
                   MessageSerializers.DefaultMessageSerializer<T>(),
                   RoutingKeyGenerators.DefaultRoutingKeyGenerator<T>(),
                   logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitPublisher" /> class.
        /// </summary>
        /// <param name="rabbitPublisherSettings"></param>
        /// <param name="lifecycleProfile"></param>
        /// <param name="messageSerializer"></param>
        [SetsRequiredMembers]
        public QueueingRabbitPublisher(IQueueingRabbitPublisherSettings rabbitPublisherSettings,
                                       IConnectionLifecycleProfile lifecycleProfile,
                                       MessageSerializerDelegate<T> messageSerializer,
                                       ILogger? logger)
            : this(rabbitPublisherSettings,
                   lifecycleProfile,
                   messageSerializer,
                   RoutingKeyGenerators.DefaultRoutingKeyGenerator<T>(),
                   logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitPublisher" /> class.
        /// </summary>
        /// <param name="rabbitPublisherSettings"></param>
        /// <param name="lifecycleProfile"></param>
        /// <param name="routingKeyGenerator"></param>
        [SetsRequiredMembers]
        public QueueingRabbitPublisher(IQueueingRabbitPublisherSettings rabbitPublisherSettings,
                                       IConnectionLifecycleProfile lifecycleProfile,
                                       RoutingKeyGeneratorDelegate<T> routingKeyGenerator,
                                       ILogger? logger)
            : this(rabbitPublisherSettings,
                   lifecycleProfile,
                   MessageSerializers.DefaultMessageSerializer<T>(),
                   routingKeyGenerator,
                   logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitPublisher" /> class.
        /// </summary>
        /// <param name="connectionString">The host.</param>
        /// <param name="exchangeName">Name of the exchange.</param>
        [SetsRequiredMembers]
        public QueueingRabbitPublisher(IQueueingRabbitPublisherSettings rabbitConfiguration,
                                       IConnectionLifecycleProfile lifecycleProfile,
                                       MessageSerializerDelegate<T> serializer,
                                       RoutingKeyGeneratorDelegate<T> routingKeyGenerator,
                                       ILogger? logger)
        {
            LifecycleProfile = lifecycleProfile;
            Serializer = serializer ?? MessageSerializers.DefaultMessageSerializer<T>();
            RoutingKeyGenerator = routingKeyGenerator ?? RoutingKeyGenerators.DefaultRoutingKeyGenerator<T>();
            RabbitConfiguration = rabbitConfiguration;
            Logger = logger;

            _queueManagerThread = new Thread(async () => await RunAsync())
            {
                Name = $"{GetType().Name}-{typeof(T).Name}"
            };
        }

        /// <summary>
        /// HostedService StartAsync.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// HostedService StopAsync.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StopAsync(CancellationToken cancellationToken) => Task.Run(ShutdownAsync, cancellationToken);

        /// <summary>
        /// Starts the <see cref="QueueingRabbitPublisher{T}"/>
        /// </summary>
        public void Start()
        {
            Logger?.LogInformation("The {typeof(T).Name} {GetType().Name} is starting.", typeof(T).Name, GetType().Name);

            _stop = false;
            _isRunning = true;

            _queueManagerThread.Start();
        }

        /// <summary>
        /// Stops the <see cref="QueueingRabbitPublisher{T}"/>
        /// </summary>
        public async Task ShutdownAsync()
        {
            Logger?.LogInformation("The {typeof(T).Name} {GetType().Name} is shutting down.", typeof(T).Name, GetType().Name);

            _stop = true;

            DateTime timeout = DateTime.Now.AddSeconds(30);

            while (_isRunning)
            {
                if (DateTime.Now > timeout)
                {
                    Logger?.LogWarning("WARNING: Timed out shutting down {GetType().Name}. (Possible loss of {typeof(T).Name} messages)", GetType().Name, typeof(T).Name);

                    _stopNow = true;

                    await Task.Delay(1000);

                    break;
                }

                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Manages the publication queue.
        /// </summary>
        private async Task RunAsync()
        {
            Logger?.LogDebug("{typeof(T).Name} {nameof(QueueManager)} is starting.", typeof(T).Name, nameof(RunAsync));

            if (await InitializeRabbitAsync() is false)
            {
                Logger?.LogWarning("WARNING: The connection to {_connection?.Endpoint.HostName} is not active.", _connection?.Endpoint.HostName);
            }

            while (!(_stop && _queue.IsEmpty))
            {
                try
                {
                    if (_queue.TryDequeue(out IQueueingRabbitPublisherItem<T>? item))
                    {
                        if (item is not null)
                        {
                            while (await PublishMessageAsync(item.Item, item.RoutingKey, item.ContentType, item.Headers) is false)
                            {
                                await Task.Delay(500);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger?.LogWarning("QUEUEING ERROR: {ex}", ex);
                }

                if (_stopNow) break;
            }

            _isRunning = false;

            Logger?.LogWarning("The {typeof(T).Name} {nameof(QueueManager)} has stopped.", typeof(T).Name, nameof(RunAsync));
        }

        /// <summary>
        /// Enqueues the message for publication.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="routingKey"></param>
        /// <param name="contentType"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public bool EnqueueForPublication(T message,
                                          string routingKey,
                                          string contentType,
                                          Dictionary<string, object>? headers = null)
            => EnqueueForPublicationAsync(message, routingKey, contentType, headers).Result;

        /// <summary>
        /// Enqueues the message for publication.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="routingKey"></param>
        /// <param name="contentType"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public async Task<bool> EnqueueForPublicationAsync(T item,
                                                           string routingKey,
                                                           string contentType,
                                                           Dictionary<string, object>? headers = null)
            => await EnqueueForPublicationAsync(new QueueingRabbitPublisherItemBase<T>()
            {
                Item = item,
                RoutingKey = routingKey,
                ContentType = contentType,
                Headers = headers,
            });

        /// <summary>
        /// Enqueues the <see cref="IQueueingRabbitPublisherItem<typeparamref name="T"/>"/> for publication. 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<bool> EnqueueForPublicationAsync(IQueueingRabbitPublisherItem<T> item)
        {
            int maxQueueDepth = (RabbitConfiguration as QueueingRabbitPublisherSettings)?.MaxQueueDepth ?? -1;

            // Wait until the ProductEventQueue depth is below our max depth threshold.
            if (maxQueueDepth >= 0 && _queue.Count > maxQueueDepth)
            {
                Logger?.LogWarning("WARNING: {typeof(T).Name} publication {nameof(maxQueueDepth)} exceeded [{maxQueueDepth}]. The item has been discarded!", typeof(T).Name, nameof(maxQueueDepth), maxQueueDepth);

                return await Task.FromResult(false);
            }

            lock (_queueLock)
            {
                _queue.Enqueue(item);

                Logger?.LogInformation("Enqueued {typeof(IQueueingRabbitPublisherItem<T>).Name}[{item.RoutingKey}] in {typeof(T).Name} queue.", typeof(IQueueingRabbitPublisherItem<T>).Name, item.RoutingKey, typeof(T).Name);
                Logger?.LogTrace("{GetType().Name}[{typeof(IQueueingRabbitPublisherItem<T>).Name}] current queue depth is {_queue.Count}", GetType().Name, typeof(IQueueingRabbitPublisherItem<T>).Name, _queue.Count);
            }

            return await Task.FromResult(true);
        }

        /// <summary>
        /// In QueueingRabbitPublishers, rather than directly publish,
        /// this method instead enqueues the provided method as a <see cref="IQueueingRabbitPublisherItem{T}"/>
        /// to be published by the <see cref="RunAsync"/>.
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
        /// In QueueingRabbitPublishers, rather than directly publish,
        /// this method instead enqueues the provided method as a <see cref="IQueueingRabbitPublisherItem{T}"/>
        /// to be published by the <see cref="RunAsync"/>.
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
        /// In QueueingRabbitPublishers, rather than directly publish,
        /// this method instead enqueues the provided method as a <see cref="IQueueingRabbitPublisherItem{T}"/>
        /// to be published by the <see cref="RunAsync"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="routingKey"></param>
        /// <param name="contentType"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public override bool Publish(T message, string routingKey, string contentType, Dictionary<string, object>? headers = null)
            => EnqueueForPublication(message, routingKey, contentType, headers);

        /// <summary>
        /// In QueueingRabbitPublishers, rather than directly publish,
        /// this method instead enqueues the provided method as a <see cref="IQueueingRabbitPublisherItem{T}"/>
        /// to be published by the <see cref="RunAsync"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="routingKey"></param>
        /// <param name="contentType"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public override async Task<bool> PublishAsync(T message, string routingKey, string contentType, Dictionary<string, object>? headers = null)
            => await EnqueueForPublicationAsync(message, routingKey, contentType, headers);

        /// <summary>
        /// Forces an immediate publication of the message bypassing the publication queue.
        /// Not recommended for use in QueueingRabbitPublishers.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="routingKey"></param>
        /// <param name="contentType"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public async Task<bool> ForcePublishAsync(T message, string routingKey, string contentType, Dictionary<string, object>? headers = null)
            => await PublishMessageAsync(message, routingKey, contentType, headers);

        /// <summary>
        /// Serializes the provided message using the configured serializer
        /// then publishes the message to the configured exchange using the provided routing key and <see cref="Encoding.UTF8"/> content type.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="routingKey"></param>
        /// <param name="contentType"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        private async Task<bool> PublishMessageAsync(T message, string routingKey, string contentType, Dictionary<string, object>? headers = null)
            => await PublishMessageAsync(Encoding.UTF8.GetBytes(Serializer(message)),
                                   routingKey,
                                   contentType,
                                   Encoding.UTF8.WebName,
                                   headers);

        /// <summary>
        /// Publishes the specified byte array content
        /// to the configured exchange.
        /// </summary>
        /// <param name="messageBytes">Content of the message.</param>
        /// <param name="routingKey">The routing key.</param>
        private async Task<bool> PublishMessageAsync(byte[] messageBytes,
                                                     string routingKey,
                                                     string contentType,
                                                     string contentEncoding,
                                                     Dictionary<string, object>? headers = null)
        {
            _publishSuccess = false;

            if (await InitializeRabbitAsync() is false)
            {
                Logger?.LogWarning("WARNING: Connection to {_connection?.Endpoint.HostName} is not active.", _connection?.Endpoint.HostName);
                return false;
            }

            Logger?.LogDebug("Connection to {_connection?.Endpoint.HostName} is active.", _connection?.Endpoint.HostName);

            try
            {
                IBasicProperties? properties = _channel?.CreateBasicProperties();

                if (properties is not null)
                {
                    properties.ContentEncoding = contentEncoding;
                    properties.ContentType = contentType;

                    if (headers is not null)
                        properties.Headers = headers;
                }

                _channel.BasicPublish(RabbitConfiguration.ExchangeName,
                                      routingKey,
                                      properties,
                                      messageBytes);

                if (RabbitConfiguration.EnableAcks)
                {
                    _channel?.WaitForConfirmsOrDie();
                }

                _publishSuccess = true;

                Logger?.LogDebug("Published {typeof(T).Name} [{routingKey}] to [{RabbitConfiguration.ExchangeName}]", typeof(T).Name, routingKey, RabbitConfiguration.ExchangeName);
            }
            catch (Exception ex)
            {
                Logger?.LogError("PUBLISHING ERROR: {ex}", ex);
            }

            return _publishSuccess;
        }
    }
}
