using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RabbitHutch.Consumers;
using RabbitHutch.Consumers.Interfaces;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Delegates;
using RabbitHutch.Core.Serialization;
using RabbitHutch.Publishers;
using RabbitHutch.Publishers.Interfaces;

namespace RabbitHutch.DependencyInjection
{
    /// <summary>
    /// The <see cref="RabbitPublisherFactory"/> extensions.
    /// </summary>
    public static class RabbitConsumerFactoryExtensions
    {
        /// <summary>
        /// Adds a <see cref="IRabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        /// <param name="lifecycleProfile"></param>
        /// <param name="logger"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitPublisher<T>(this IServiceCollection services, IRabbitPublisherSettings settings, IConnectionLifecycleProfile lifecycleProfile, ILogger? logger, string? name)
        {
            name ??= typeof(T).Name;
            services.TryAddSingleton<IRabbitPublisherFactory, RabbitPublisherFactory>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            IRabbitPublisherFactory factory = serviceProvider.GetRequiredService<IRabbitPublisherFactory>();
            factory.CreatePublisher<T>(settings, lifecycleProfile, logger, name);
            return services;
        }
    }

    /// <summary>
    /// The <see cref="IRabbitConsumerFactory"/> interface.
    /// </summary>
    public interface IRabbitConsumerFactory
    {
        IRabbitConsumer<T> GetConsumer<T>();
        IRabbitConsumer<T> GetConsumer<T>(string name);

        IRabbitConsumer<T> CreateDummyConsumer<T>(IRabbitConsumerSettings settings, IConnectionLifecycleProfile lifecycleProfile, AsyncNewMessageCallbackDelegate<T> newMessageCallback, ILogger? logger = null, string? name = null) where T : new();
        IRabbitConsumer<T> CreateDummyConsumer<T>(IRabbitConsumerSettings settings, IConnectionLifecycleProfile lifecycleProfile, AsyncNewMessageCallbackDelegate<T> newMessageCallback, MessageDeserializerFromBytesDelegate<T> deserializer, ILogger? logger = null, string? name = null) where T : new();

        IRabbitConsumer<T> CreateConsumer<T>(IRabbitConsumerSettings settings, IConnectionLifecycleProfile lifecycleProfile, AsyncNewMessageCallbackDelegate<T> newMessageCallback, ILogger? logger = null, string? name = null);
        IRabbitConsumer<T> CreateConsumer<T>(IRabbitConsumerSettings settings, IConnectionLifecycleProfile lifecycleProfile, AsyncNewMessageCallbackDelegate<T> newMessageCallback, MessageDeserializerFromBytesDelegate<T> deserializer, ILogger? logger = null, string? name = null);

        IRabbitConsumer<T> CreateSingleFetchConsumer<T>(IRabbitConsumerSettings settings, IConnectionLifecycleProfile lifecycleProfile, AsyncNewMessageCallbackDelegate<T> newMessageCallback, ILogger? logger = null, string? name = null);
        IRabbitConsumer<T> CreateSingleFetchConsumer<T>(IRabbitConsumerSettings settings, IConnectionLifecycleProfile lifecycleProfile, AsyncNewMessageCallbackDelegate<T> newMessageCallback, MessageDeserializerFromBytesDelegate<T> deserializer, ILogger? logger = null, string? name = null);
    }

    /// <summary>
    /// The <see cref="RabbitConsumerFactory"/>.
    /// </summary>
    public class RabbitConsumerFactory(ILoggerFactory loggerFactory, ILogger<RabbitConsumerFactory> publisherLogger, CancellationTokenSource? cancellationTokenSource) : IRabbitConsumerFactory
    {
        private static readonly Dictionary<string, object> _instances = [];

        private readonly CancellationToken _cancellationToken = cancellationTokenSource?.Token ?? CancellationToken.None;

        /// <summary>
        /// Gets a <see cref="IRabbitPublisher{T}"/> by <see cref="type"/> key <see cref="{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IRabbitConsumer<T> GetConsumer<T>() => (IRabbitConsumer<T>)_instances[typeof(T).Name];

        /// <summary>
        /// Gets a <see cref="IRabbitPublisher{T}"/> by <see cref="name"/> key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public IRabbitConsumer<T> GetConsumer<T>(string name) => (IRabbitConsumer<T>)_instances[name];

        /// <summary>
        /// Creates a new <see cref="DummyRabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lifecycleProfile"></param>
        /// <param name="publisherSettings"></param>
        /// <returns></returns>
        public IRabbitConsumer<T> CreateDummyConsumer<T>(IRabbitConsumerSettings publisherSettings,
                                                         IConnectionLifecycleProfile lifecycleProfile,
                                                         AsyncNewMessageCallbackDelegate<T> newMessageCallback,
                                                         ILogger? logger,
                                                         string? name) where T : new()
            => CreateDummyConsumer(publisherSettings,
                                   lifecycleProfile,
                                   newMessageCallback,
                                   MessageDeserializers.DefaultMessageDeserializerFromBytes<T>(),
                                   logger,
                                   name);

        /// <summary>
        /// Creates a new <see cref="DummyRabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lifecycleProfile"></param>
        /// <param name="publisherSettings"></param>
        /// <param name="serializer"></param>
        /// <param name="routingKeyGenerator"></param>
        /// <returns></returns>
        public IRabbitConsumer<T> CreateDummyConsumer<T>(IRabbitConsumerSettings publisherSettings,
                                                         IConnectionLifecycleProfile lifecycleProfile,
                                                         AsyncNewMessageCallbackDelegate<T> newMessageCallback,
                                                         MessageDeserializerFromBytesDelegate<T> deserializer,
                                                         ILogger? logger,
                                                         string? name) where T : new()
        {
            name ??= typeof(T).Name;
            if (_instances.TryGetValue(name, out object? value) is false)
            {
                logger ??= loggerFactory.CreateLogger<DummyRabbitConsumer<T>>();
                DummyRabbitConsumer<T> consumer = new(publisherSettings, lifecycleProfile, deserializer, newMessageCallback, logger)
                {
                    Name = name,
                    CancellationToken = _cancellationToken
                };
                consumer.Start();

                value = consumer;
                _instances.Add(name, consumer);

                publisherLogger.LogDebug("Created {consumerType} with name {name}", typeof(DummyRabbitConsumer<T>).Name, name);
            }
            return (IRabbitConsumer<T>)value;
        }

        /// <summary>
        /// Creates a new <see cref="RabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public IRabbitConsumer<T> CreateConsumer<T>(IRabbitConsumerSettings publisherSettings,
                                                         IConnectionLifecycleProfile lifecycleProfile,
                                                         AsyncNewMessageCallbackDelegate<T> newMessageCallback,
                                                         ILogger? logger,
                                                         string? name)
            => CreateConsumer(publisherSettings,
                                   lifecycleProfile,
                                   newMessageCallback,
                                   MessageDeserializers.DefaultMessageDeserializerFromBytes<T>(),
                                   logger,
                                   name);

        /// <summary>
        /// Creates a new <see cref="RabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="settings"></param>
        /// <param name="serializer"></param>
        /// <param name="routingKeyGenerator"></param>
        /// <returns></returns>
        public IRabbitConsumer<T> CreateConsumer<T>(IRabbitConsumerSettings settings,
                                                         IConnectionLifecycleProfile lifecycleProfile,
                                                         AsyncNewMessageCallbackDelegate<T> newMessageCallback,
                                                         MessageDeserializerFromBytesDelegate<T> deserializer,
                                                         ILogger? logger,
                                                         string? name)
        {
            name ??= typeof(T).Name;
            if (_instances.TryGetValue(name, out object? value) is false)
            {
                logger ??= loggerFactory.CreateLogger<QueueingRabbitPublisher<T>>();
                RabbitConsumer<T> consumer = new(settings, lifecycleProfile, deserializer, newMessageCallback, logger)
                {
                    Name = name,
                    CancellationToken = _cancellationToken
                };
                consumer.Start();

                value = consumer;
                _instances.Add(name, value);

                publisherLogger.LogDebug("Created {consumerType} with name {name}", typeof(RabbitConsumer<T>).Name, name);
            }
            return (IRabbitConsumer<T>)value;
        }

        /// <summary>
        /// Creates a new <see cref="SingleFetchRabbitConsumer{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settings"></param>
        /// <param name="lifecycleProfile"></param>
        /// <param name="newMessageCallback"></param>
        /// <param name="logger"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IRabbitConsumer<T> CreateSingleFetchConsumer<T>(IRabbitConsumerSettings settings,
                                                               IConnectionLifecycleProfile lifecycleProfile,
                                                               AsyncNewMessageCallbackDelegate<T> newMessageCallback,
                                                               ILogger? logger,
                                                                string? name)
            => CreateSingleFetchConsumer(settings,
                                         lifecycleProfile,
                                         newMessageCallback,
                                         MessageDeserializers.DefaultMessageDeserializerFromBytes<T>(),
                                         logger,
                                         name);

        /// <summary>
        /// Creates a new <see cref="SingleFetchRabbitConsumer{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settings"></param>
        /// <param name="lifecycleProfile"></param>
        /// <param name="newMessageCallback"></param>
        /// <param name="deserializer"></param>
        /// <param name="logger"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IRabbitConsumer<T> CreateSingleFetchConsumer<T>(IRabbitConsumerSettings settings,
                                                               IConnectionLifecycleProfile lifecycleProfile,
                                                               AsyncNewMessageCallbackDelegate<T> newMessageCallback,
                                                               MessageDeserializerFromBytesDelegate<T> deserializer,
                                                               ILogger? logger,
                                                               string? name)
        {
            name ??= typeof(T).Name;
            if (_instances.TryGetValue(name, out object? value) is false)
            {
                logger ??= loggerFactory.CreateLogger<SingleFetchRabbitConsumer<T>>();
                SingleFetchRabbitConsumer<T> consumer = new(settings, lifecycleProfile, deserializer, newMessageCallback, logger)
                {
                    Name = name,
                    CancellationToken = _cancellationToken
                };
                value = consumer;
                _instances.Add(name, value);
                publisherLogger.LogDebug("Created {consumerType} with name {name}", typeof(SingleFetchRabbitConsumer<T>).Name, name);
            }
            return (IRabbitConsumer<T>)value;
        }
    }
}
