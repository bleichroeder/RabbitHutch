using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Routing;
using RabbitHutch.Core.Serialization;
using RabbitHutch.Publishers;
using RabbitHutch.Publishers.Interfaces;

namespace RabbitHutch.DependencyInjection
{
    /// <summary>
    /// Provides methods for configuring a RabbitHutch.
    /// </summary>
    public class RabbitHutchBuilder
    {
        private readonly IServiceCollection _services;
        private readonly IRabbitPublisherFactory _publisherFactory;

        /// <summary>
        /// Provides methods for configuring a RabbitHutch.
        /// </summary>
        /// <param name="services"></param>
        public RabbitHutchBuilder(IServiceCollection services)
        {
            _services = services;
            _services.TryAddSingleton<IRabbitPublisherFactory, RabbitPublisherFactory>();
            _publisherFactory = _services.BuildServiceProvider().GetRequiredService<IRabbitPublisherFactory>();
        }

        /// <summary>
        /// Executes the <see cref="RabbitPublisherBuilderConfigurationContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        private static RabbitPublisherBuilderConfigurationContext<T> ExecutePublisherConfigurationContext<T>(Action<RabbitPublisherBuilderConfigurationContext<T>> builderContext) where T : notnull
        {
            var context = new RabbitPublisherBuilderConfigurationContext<T>();
            builderContext(context);
            return context;
        }

        /// <summary>
        /// Builds and registers a <see cref="RabbitPublisher{T}"/> for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddPublisher<T>(IRabbitPublisherSettings publisherSettings, Action<RabbitPublisherBuilderConfigurationContext<T>> builderContext) where T : notnull
            => AddPublisher(publisherSettings, ConnectionLifecycleProfiles.DefaultPublisherConnectionLifecycleProfile(), builderContext);

        /// <summary>
        /// Builds and registers a <see cref="RabbitPublisher{T}"/> for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <param name="lifecycleProfile"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddPublisher<T>(IRabbitPublisherSettings publisherSettings, IConnectionLifecycleProfile lifecycleProfile, Action<RabbitPublisherBuilderConfigurationContext<T>> builderContext) where T : notnull
        {
            RabbitPublisherBuilderConfigurationContext<T> publisherConfigurationContext = ExecutePublisherConfigurationContext(builderContext);

            RegisterTypeIfNotExists<T>();

            IRabbitPublisher<T> publisher = _publisherFactory.CreatePublisher<T>(publisherSettings, lifecycleProfile, publisherConfigurationContext.Logger, publisherConfigurationContext.Name);

            ConfigurePublisher(publisher, publisherConfigurationContext);

            AddAsHostedServiceIfRequired(publisher, publisherConfigurationContext.AsHostedService);

            return this;
        }

        /// <summary>
        /// Builds and registers a <see cref="QueueingRabbitPublisher{T}"/> for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddQueueingPublisher<T>(QueueingRabbitPublisherSettings publisherSettings, Action<RabbitPublisherBuilderConfigurationContext<T>> builderContext) where T : notnull
            => AddQueueingPublisher(publisherSettings, ConnectionLifecycleProfiles.DefaultPublisherConnectionLifecycleProfile(), builderContext);

        /// <summary>
        /// Builds and registers a <see cref="QueueingRabbitPublisher{T}"/> for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddQueueingPublisher<T>(QueueingRabbitPublisherSettings publisherSettings, IConnectionLifecycleProfile lifecycleProfile, Action<RabbitPublisherBuilderConfigurationContext<T>> builderContext) where T : notnull
        {
            RabbitPublisherBuilderConfigurationContext<T> publisherConfigurationContext = ExecutePublisherConfigurationContext(builderContext);

            RegisterTypeIfNotExists<T>();

            IRabbitPublisher<T> publisher = _publisherFactory.CreateQueueingPublisher<T>(publisherSettings, lifecycleProfile, name: publisherConfigurationContext.Name);

            ConfigurePublisher(publisher, publisherConfigurationContext);

            AddAsHostedServiceIfRequired(publisher, publisherConfigurationContext.AsHostedService);

            return this;
        }

        /// <summary>
        /// Builds and registers a <see cref="DummyRabbitPublisher{T}"/> for the specified type.
        /// Dummy publishers do not attempt to connect to RabbitMQ and are used for testing purposes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddDummyPublisher<T>(RabbitPublisherSettings publisherSettings, Action<RabbitPublisherBuilderConfigurationContext<T>> builderContext) where T : notnull
            => AddDummyPublisher(publisherSettings, ConnectionLifecycleProfiles.DefaultPublisherConnectionLifecycleProfile(), builderContext);

        /// <summary>
        /// Builds and registers a <see cref="DummyRabbitPublisher{T}"/> for the specified type.
        /// Dummy publishers do not attempt to connect to RabbitMQ and are used for testing purposes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddDummyPublisher<T>(RabbitPublisherSettings publisherSettings, IConnectionLifecycleProfile lifecycleProfile, Action<RabbitPublisherBuilderConfigurationContext<T>> builderContext) where T : notnull
        {
            RabbitPublisherBuilderConfigurationContext<T> publisherConfigurationContext = ExecutePublisherConfigurationContext(builderContext);

            RegisterTypeIfNotExists<T>();

            IRabbitPublisher<T> publisher = _publisherFactory.CreateDummyPublisher<T>(publisherSettings, lifecycleProfile, name: publisherConfigurationContext.Name);

            ConfigurePublisher(publisher, publisherConfigurationContext);

            AddAsHostedServiceIfRequired(publisher, publisherConfigurationContext.AsHostedService);

            return this;
        }

        /// <summary>
        /// Configures the publisher with the specified configuration context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisher"></param>
        /// <param name="config"></param>
        private static void ConfigurePublisher<T>(IRabbitPublisher<T> publisher, RabbitPublisherBuilderConfigurationContext<T> config)
        {
            if (config.RoutingKeyDelegate is not null)
                publisher.RoutingKeyGenerator = new RoutingKeyGeneratorDelegate<T>(config.RoutingKeyDelegate);

            if (config.SerializerDelegate is not null)
                publisher.Serializer = new MessageSerializerDelegate<T>(config.SerializerDelegate);
        }

        /// <summary>
        /// Attempts to register the type as a singleton if it does not already exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private void RegisterTypeIfNotExists<T>() where T : notnull => _services.TryAddSingleton(typeof(T));

        /// <summary>
        /// If the publisher is configured to be registered as a hosted service, then it is added to the service collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisher"></param>
        /// <param name="asHostedService"></param>
        private void AddAsHostedServiceIfRequired<T>(IRabbitPublisher<T> publisher, bool asHostedService)
        {
            if (asHostedService)
            {
                _services.AddSingleton<IHostedService>(sp => new HostedRabbitPublisherWrapper<IRabbitPublisher<T>>(publisher));
            }
        }
    }
}
