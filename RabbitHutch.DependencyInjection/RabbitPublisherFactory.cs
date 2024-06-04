using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Routing;
using RabbitHutch.Core.Serialization;
using RabbitHutch.Publishers;
using RabbitHutch.Publishers.Interfaces;

namespace RabbitHutch.DependencyInjection
{
    /// <summary>
    /// The <see cref="RabbitPublisherFactory"/> extensions.
    /// </summary>
    public static class RabbitPublisherFactoryExtensions
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
    /// The <see cref="IRabbitPublisherFactory"/> interface.
    /// </summary>
    public interface IRabbitPublisherFactory
    {
        IRabbitPublisher<T> GetPublisher<T>();
        IRabbitPublisher<T> GetPublisher<T>(string name);

        IRabbitPublisher<T> CreateDummyPublisher<T>(IRabbitPublisherSettings settings, IConnectionLifecycleProfile lifecycleProfile, ILogger? logger = null, string? name = null);
        IRabbitPublisher<T> CreateDummyPublisher<T>(IRabbitPublisherSettings settings, IConnectionLifecycleProfile lifecycleProfile, MessageSerializerDelegate<T> serializer, ILogger? logger = null, string? name = null);
        IRabbitPublisher<T> CreateDummyPublisher<T>(IRabbitPublisherSettings settings, IConnectionLifecycleProfile lifecycleProfile, RoutingKeyGeneratorDelegate<T> routingKeyGenerator, ILogger? logger = null, string? name = null);
        IRabbitPublisher<T> CreateDummyPublisher<T>(IRabbitPublisherSettings settings, IConnectionLifecycleProfile lifecycleProfile, MessageSerializerDelegate<T> serializer, RoutingKeyGeneratorDelegate<T> routingKeyGenerator, ILogger? logger = null, string? name = null);

        IRabbitPublisher<T> CreateQueueingPublisher<T>(IQueueingRabbitPublisherSettings settings, IConnectionLifecycleProfile lifecycleProfile, ILogger? logger = null, string? name = null);
        IRabbitPublisher<T> CreateQueueingPublisher<T>(IQueueingRabbitPublisherSettings settings, IConnectionLifecycleProfile lifecycleProfile, MessageSerializerDelegate<T> serializer, ILogger? logger = null, string? name = null);
        IRabbitPublisher<T> CreateQueueingPublisher<T>(IQueueingRabbitPublisherSettings settings, IConnectionLifecycleProfile lifecycleProfile, RoutingKeyGeneratorDelegate<T> routingKeyGenerator, ILogger? logger = null, string? name = null);
        IRabbitPublisher<T> CreateQueueingPublisher<T>(IQueueingRabbitPublisherSettings settings, IConnectionLifecycleProfile lifecycleProfile, MessageSerializerDelegate<T> serializer, RoutingKeyGeneratorDelegate<T> routingKeyGenerator, ILogger? logger = null, string? name = null);

        IRabbitPublisher<T> CreatePublisher<T>(IRabbitPublisherSettings settings, IConnectionLifecycleProfile lifecycleProfile, ILogger? logger = null, string? name = null);
        IRabbitPublisher<T> CreatePublisher<T>(IRabbitPublisherSettings settings, IConnectionLifecycleProfile lifecycleProfile, MessageSerializerDelegate<T> serializer, ILogger? logger = null, string? name = null);
        IRabbitPublisher<T> CreatePublisher<T>(IRabbitPublisherSettings settings, IConnectionLifecycleProfile lifecycleProfile, RoutingKeyGeneratorDelegate<T> routingKeyGenerator, ILogger? logger = null, string? name = null);
        IRabbitPublisher<T> CreatePublisher<T>(IRabbitPublisherSettings settings, IConnectionLifecycleProfile lifecycleProfile, MessageSerializerDelegate<T> serializer, RoutingKeyGeneratorDelegate<T> routingKeyGenerator, ILogger? logger = null, string? name = null);
    }

    /// <summary>
    /// The <see cref="RabbitPublisherFactory"/>.
    /// </summary>
    public class RabbitPublisherFactory(ILoggerFactory loggerFactory, ILogger<RabbitPublisherFactory> publisherLogger) : IRabbitPublisherFactory
    {
        private static readonly Dictionary<string, object> _instances = [];

        /// <summary>
        /// Gets a <see cref="IRabbitPublisher{T}"/> by <see cref="type"/> key <see cref="{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IRabbitPublisher<T> GetPublisher<T>() => (IRabbitPublisher<T>)_instances[typeof(T).Name];

        /// <summary>
        /// Gets a <see cref="IRabbitPublisher{T}"/> by <see cref="name"/> key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public IRabbitPublisher<T> GetPublisher<T>(string name) => (IRabbitPublisher<T>)_instances[name];

        /// <summary>
        /// Creates a new <see cref="DummyRabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lifecycleProfile"></param>
        /// <param name="publisherSettings"></param>
        /// <returns></returns>
        public IRabbitPublisher<T> CreateDummyPublisher<T>(IRabbitPublisherSettings publisherSettings,
                                                           IConnectionLifecycleProfile lifecycleProfile,
                                                           ILogger? logger,
                                                           string? name)
            => CreateDummyPublisher(publisherSettings,
                                    lifecycleProfile,
                                    MessageSerializers.DefaultMessageSerializer<T>(),
                                    RoutingKeyGenerators.DefaultRoutingKeyGenerator<T>(),
                                    logger,
                                    name);

        /// <summary>
        /// Creates a new <see cref="DummyRabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lifecycleProfile"></param>
        /// <param name="publisherSettings"></param>
        /// <param name="routingKeyGenerator"></param>
        /// <returns></returns>
        public IRabbitPublisher<T> CreateDummyPublisher<T>(IRabbitPublisherSettings publisherSettings,
                                                           IConnectionLifecycleProfile lifecycleProfile,
                                                           RoutingKeyGeneratorDelegate<T> routingKeyGenerator,
                                                           ILogger? logger,
                                                           string? name)
            => CreateDummyPublisher(publisherSettings,
                                    lifecycleProfile,
                                    MessageSerializers.DefaultMessageSerializer<T>(),
                                    routingKeyGenerator,
                                    logger,
                                    name);

        /// <summary>
        /// Creates a new <see cref="DummyRabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lifecycleProfile"></param>
        /// <param name="publisherSettings"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public IRabbitPublisher<T> CreateDummyPublisher<T>(IRabbitPublisherSettings publisherSettings,
                                                           IConnectionLifecycleProfile lifecycleProfile,
                                                           MessageSerializerDelegate<T> serializer,
                                                           ILogger? logger,
                                                           string? name)
            => CreateDummyPublisher(publisherSettings,
                                    lifecycleProfile,
                                    serializer,
                                    RoutingKeyGenerators.DefaultRoutingKeyGenerator<T>(),
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
        public IRabbitPublisher<T> CreateDummyPublisher<T>(IRabbitPublisherSettings publisherSettings,
                                                           IConnectionLifecycleProfile lifecycleProfile,
                                                           MessageSerializerDelegate<T> serializer,
                                                           RoutingKeyGeneratorDelegate<T> routingKeyGenerator,
                                                           ILogger? logger,
                                                           string? name)
        {
            name ??= typeof(T).Name;
            if (_instances.TryGetValue(name, out object? value) is false)
            {
                logger ??= loggerFactory.CreateLogger<QueueingRabbitPublisher<T>>();
                DummyRabbitPublisher<T> publisher = new(publisherSettings, lifecycleProfile, serializer, routingKeyGenerator, logger);
                value = publisher;
                _instances.Add(name, publisher);

                logger.LogDebug("Created {publisherType} with name {name}", typeof(DummyRabbitPublisher<T>).Name, name);
            }
            return (IRabbitPublisher<T>)value;
        }

        /// <summary>
        /// Creates a new <see cref="QueueingRabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <returns></returns>
        public IRabbitPublisher<T> CreateQueueingPublisher<T>(IQueueingRabbitPublisherSettings publisherSettings,
                                                              IConnectionLifecycleProfile lifecycleProfile,
                                                              ILogger? logger,
                                                              string? name)
            => CreateQueueingPublisher<T>(publisherSettings,
                                          lifecycleProfile,
                                          MessageSerializers.DefaultMessageSerializer<T>(),
                                          RoutingKeyGenerators.DefaultRoutingKeyGenerator<T>(),
                                          logger,
                                          name);

        /// <summary>
        /// Creates a new <see cref="QueueingRabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public IRabbitPublisher<T> CreateQueueingPublisher<T>(IQueueingRabbitPublisherSettings publisherSettings,
                                                              IConnectionLifecycleProfile lifecycleProfile,
                                                              MessageSerializerDelegate<T> serializer,
                                                              ILogger? logger,
                                                              string? name)
            => CreateQueueingPublisher<T>(publisherSettings,
                                          lifecycleProfile,
                                          serializer,
                                          RoutingKeyGenerators.DefaultRoutingKeyGenerator<T>(),
                                          logger,
                                          name);

        /// <summary>
        /// Creates a new <see cref="QueueingRabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <param name="routingKeyGenerator"></param>
        /// <returns></returns>
        public IRabbitPublisher<T> CreateQueueingPublisher<T>(IQueueingRabbitPublisherSettings publisherSettings,
                                                              IConnectionLifecycleProfile lifecycleProfile,
                                                              RoutingKeyGeneratorDelegate<T> routingKeyGenerator,
                                                              ILogger? logger,
                                                              string? name)
            => CreateQueueingPublisher<T>(publisherSettings,
                                          lifecycleProfile,
                                          MessageSerializers.DefaultMessageSerializer<T>(),
                                          routingKeyGenerator,
                                          logger,
                                          name);

        /// <summary>
        /// Creates a new <see cref="QueueingRabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="publisherSettings"></param>
        /// <param name="serializer"></param>
        /// <param name="routingKeyGenerator"></param>
        /// <returns></returns>
        public IRabbitPublisher<T> CreateQueueingPublisher<T>(IQueueingRabbitPublisherSettings publisherSettings,
                                                              IConnectionLifecycleProfile lifecycleProfile,
                                                              MessageSerializerDelegate<T> serializer,
                                                              RoutingKeyGeneratorDelegate<T> routingKeyGenerator,
                                                              ILogger? logger,
                                                              string? name)
        {
            name ??= typeof(T).Name;
            if (_instances.TryGetValue(name, out object? value) is false)
            {
                logger ??= loggerFactory.CreateLogger<QueueingRabbitPublisher<T>>();
                QueueingRabbitPublisher<T> publisher = new(publisherSettings, lifecycleProfile, serializer, routingKeyGenerator, logger);
                publisher.Start();
                value = publisher;
                _instances.Add(name, value);

                logger.LogDebug("Created {publisherType} with name {name}", typeof(QueueingRabbitPublisher<T>).Name, name);
            }
            return (IRabbitPublisher<T>)value;
        }

        /// <summary>
        /// Creates a new <see cref="RabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <returns></returns>
        public IRabbitPublisher<T> CreatePublisher<T>(IRabbitPublisherSettings publisherSettings,
                                                      IConnectionLifecycleProfile lifecycleProfile,
                                                      ILogger? logger,
                                                      string? name)
            => CreatePublisher(publisherSettings,
                               lifecycleProfile,
                               MessageSerializers.DefaultMessageSerializer<T>(),
                               RoutingKeyGenerators.DefaultRoutingKeyGenerator<T>(),
                               logger,
                               name);

        /// <summary>
        /// Creates a new <see cref="RabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public IRabbitPublisher<T> CreatePublisher<T>(IRabbitPublisherSettings publisherSettings,
                                                      IConnectionLifecycleProfile lifecycleProfile,
                                                      MessageSerializerDelegate<T> serializer,
                                                      ILogger? logger,
                                                      string? name)
            => CreatePublisher(publisherSettings,
                               lifecycleProfile,
                               serializer,
                               RoutingKeyGenerators.DefaultRoutingKeyGenerator<T>(),
                               logger,
                               name);

        /// <summary>
        /// Creates a new <see cref="RabbitPublisher{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settings"></param>
        /// <param name="routingKeyGenerator"></param>
        /// <returns></returns>
        public IRabbitPublisher<T> CreatePublisher<T>(IRabbitPublisherSettings settings,
                                                      IConnectionLifecycleProfile lifecycleProfile,
                                                      RoutingKeyGeneratorDelegate<T> routingKeyGenerator,
                                                      ILogger? logger,
                                                      string? name)
            => CreatePublisher(settings,
                               lifecycleProfile,
                               MessageSerializers.DefaultMessageSerializer<T>(),
                               routingKeyGenerator,
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
        public IRabbitPublisher<T> CreatePublisher<T>(IRabbitPublisherSettings settings,
                                                      IConnectionLifecycleProfile lifecycleProfile,
                                                      MessageSerializerDelegate<T> serializer,
                                                      RoutingKeyGeneratorDelegate<T> routingKeyGenerator,
                                                      ILogger? logger,
                                                      string? name)
        {
            name ??= typeof(T).Name;
            if (_instances.TryGetValue(name, out object? value) is false)
            {
                logger ??= loggerFactory.CreateLogger<QueueingRabbitPublisher<T>>();
                RabbitPublisher<T> publisher = new(settings, lifecycleProfile, serializer, routingKeyGenerator, logger);
                value = publisher;
                _instances.Add(name, value);

                logger.LogDebug("Created {publisherType} with name {name}", typeof(RabbitPublisher<T>).Name, name);
            }
            return (IRabbitPublisher<T>)value;
        }
    }
}
