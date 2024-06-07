using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using RabbitHutch.Consumers;
using RabbitHutch.Consumers.Interfaces;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.DependencyInjection.Interfaces;
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
        private readonly IRabbitConsumerFactory _consumerFactory;

        /// <summary>
        /// Provides methods for configuring a RabbitHutch.
        /// </summary>
        /// <param name="services"></param>
        public RabbitHutchBuilder(IServiceCollection services)
        {
            _services = services;

            _services.TryAddSingleton<IRabbitPublisherFactory, RabbitPublisherFactory>();
            _publisherFactory = _services.BuildServiceProvider().GetRequiredService<IRabbitPublisherFactory>();

            _services.TryAddSingleton<IRabbitConsumerFactory, RabbitConsumerFactory>();
            _consumerFactory = _services.BuildServiceProvider().GetRequiredService<IRabbitConsumerFactory>();
        }

        /// <summary>
        /// Executes the <see cref="RabbitPublisherBuilderConfigurationContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        private static RabbitPublisherBuilderConfigurationContext<T> ExecutePublisherConfigurationContext<T>(Action<IRabbitPublisherBuilderConfigurationContext<T>> builderContext) where T : notnull
        {
            var context = new RabbitPublisherBuilderConfigurationContext<T>();
            builderContext(context);
            return context;
        }

        /// <summary>
        /// Executes the <see cref="RabbitConsumerBuilderConfigurationContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        private static RabbitConsumerBuilderConfigurationContext<T> ExecuteConsumerConfigurationContext<T>(Action<IRabbitConsumerBuilderConfigurationContext<T>> builderContext) where T : notnull
        {
            var context = new RabbitConsumerBuilderConfigurationContext<T>();
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
        public RabbitHutchBuilder AddPublisher<T>(IRabbitPublisherSettings publisherSettings, Action<IRabbitPublisherBuilderConfigurationContext<T>> builderContext) where T : notnull
            => AddPublisher(publisherSettings, ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile(), builderContext);

        /// <summary>
        /// Builds and registers a <see cref="RabbitPublisher{T}"/> for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <param name="lifecycleProfile"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddPublisher<T>(IRabbitPublisherSettings publisherSettings, IConnectionLifecycleProfile lifecycleProfile, Action<IRabbitPublisherBuilderConfigurationContext<T>> builderContext) where T : notnull
        {
            RabbitPublisherBuilderConfigurationContext<T> publisherConfigurationContext = ExecutePublisherConfigurationContext(builderContext);

            RegisterTypeIfNotExists<T>();

            publisherSettings.ExchangeDeclarationSettings = publisherConfigurationContext.ExchangeDeclarationSettings ?? publisherSettings.ExchangeDeclarationSettings;

            IRabbitPublisher<T> publisher = _publisherFactory.CreatePublisher<T>(publisherSettings,
                                                                                 lifecycleProfile,
                                                                                 publisherConfigurationContext.SerializerDelegate,
                                                                                 publisherConfigurationContext.RoutingKeyDelegate,
                                                                                 publisherConfigurationContext.Logger,
                                                                                 publisherConfigurationContext.Name);

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
        public RabbitHutchBuilder AddQueueingPublisher<T>(IQueueingRabbitPublisherSettings publisherSettings, Action<IRabbitPublisherBuilderConfigurationContext<T>> builderContext) where T : notnull
            => AddQueueingPublisher(publisherSettings, ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile(), builderContext);

        /// <summary>
        /// Builds and registers a <see cref="QueueingRabbitPublisher{T}"/> for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddQueueingPublisher<T>(IQueueingRabbitPublisherSettings publisherSettings, IConnectionLifecycleProfile lifecycleProfile, Action<IRabbitPublisherBuilderConfigurationContext<T>> builderContext) where T : notnull
        {
            RabbitPublisherBuilderConfigurationContext<T> publisherConfigurationContext = ExecutePublisherConfigurationContext(builderContext);

            RegisterTypeIfNotExists<T>();

            publisherSettings.ExchangeDeclarationSettings = publisherConfigurationContext.ExchangeDeclarationSettings ?? publisherSettings.ExchangeDeclarationSettings;

            IRabbitPublisher<T> publisher = _publisherFactory.CreateQueueingPublisher<T>(publisherSettings,
                                                                                         lifecycleProfile,
                                                                                         publisherConfigurationContext.SerializerDelegate,
                                                                                         publisherConfigurationContext.RoutingKeyDelegate,
                                                                                         name: publisherConfigurationContext.Name);

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
        public RabbitHutchBuilder AddDummyPublisher<T>(IRabbitPublisherSettings publisherSettings, Action<IRabbitPublisherBuilderConfigurationContext<T>> builderContext) where T : notnull
            => AddDummyPublisher(publisherSettings, ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile(), builderContext);

        /// <summary>
        /// Builds and registers a <see cref="DummyRabbitPublisher{T}"/> for the specified type.
        /// Dummy publishers do not attempt to connect to RabbitMQ and are used for testing purposes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSettings"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddDummyPublisher<T>(IRabbitPublisherSettings publisherSettings, IConnectionLifecycleProfile lifecycleProfile, Action<IRabbitPublisherBuilderConfigurationContext<T>> builderContext) where T : notnull
        {
            IRabbitPublisherBuilderConfigurationContext<T> publisherConfigurationContext = ExecutePublisherConfigurationContext(builderContext);

            RegisterTypeIfNotExists<T>();

            publisherSettings.ExchangeDeclarationSettings = publisherConfigurationContext.ExchangeDeclarationSettings ?? publisherSettings.ExchangeDeclarationSettings;

            IRabbitPublisher<T> publisher = _publisherFactory.CreateDummyPublisher<T>(publisherSettings,
                                                                                      lifecycleProfile,
                                                                                      publisherConfigurationContext.SerializerDelegate,
                                                                                      publisherConfigurationContext.RoutingKeyDelegate,
                                                                                      name: publisherConfigurationContext.Name);

            AddAsHostedServiceIfRequired(publisher, publisherConfigurationContext.AsHostedService);

            return this;
        }

        /// <summary>
        /// Builds and registers a <see cref="DummyRabbitConsumer{T}"/> for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="consumerSettings"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddDummyConsumer<T>(IRabbitConsumerSettings consumerSettings, Action<IRabbitConsumerBuilderConfigurationContext<T>> builderContext) where T : notnull, new()
            => AddDummyConsumer(consumerSettings, ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile(), builderContext);

        /// <summary>
        /// Builds and registers a <see cref="DummyRabbitConsumer{T}"/> for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="consumerSettings"></param>
        /// <param name="lifecycleProfile"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddDummyConsumer<T>(IRabbitConsumerSettings consumerSettings, IConnectionLifecycleProfile lifecycleProfile, Action<IRabbitConsumerBuilderConfigurationContext<T>> builderContext) where T : notnull, new()
        {
            IRabbitConsumerBuilderConfigurationContext<T> consumerConfigurationContext = ExecuteConsumerConfigurationContext(builderContext);

            RegisterTypeIfNotExists<T>();

            consumerSettings.ExchangeDeclarationSettings = consumerConfigurationContext.ExchangeDeclarationSettings ?? consumerSettings.ExchangeDeclarationSettings;
            consumerSettings.QueueDeclarationSettings = consumerConfigurationContext.QueueDeclarationSettings ?? consumerSettings.QueueDeclarationSettings;

            IRabbitConsumer<T> consumer = _consumerFactory.CreateDummyConsumer<T>(consumerSettings,
                                                                                  lifecycleProfile,
                                                                                  consumerConfigurationContext.MessageCallbackDelegate,
                                                                                  consumerConfigurationContext.DeserializationDelegate,
                                                                                  name: consumerConfigurationContext.Name);

            AddAsHostedServiceIfRequired(consumer, consumerConfigurationContext.AsHostedService);

            return this;
        }

        /// <summary>
        /// Builds and registers a <see cref="RabbitConsumer{T}"/> for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="consumerSettings"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddConsumer<T>(IRabbitConsumerSettings consumerSettings, Action<IRabbitConsumerBuilderConfigurationContext<T>> builderContext) where T : notnull
            => AddConsumer<T>(consumerSettings, ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile(), builderContext);

        /// <summary>
        /// Builds and registers a <see cref="RabbitConsumer{T}"/> for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="consumerSettings"></param>
        /// <param name="lifecycleProfile"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddConsumer<T>(IRabbitConsumerSettings consumerSettings, IConnectionLifecycleProfile lifecycleProfile, Action<IRabbitConsumerBuilderConfigurationContext<T>> builderContext) where T : notnull
        {
            IRabbitConsumerBuilderConfigurationContext<T> consumerConfigurationContext = ExecuteConsumerConfigurationContext(builderContext);

            RegisterTypeIfNotExists<T>();

            consumerSettings.ExchangeDeclarationSettings = consumerConfigurationContext.ExchangeDeclarationSettings ?? consumerSettings.ExchangeDeclarationSettings;
            consumerSettings.QueueDeclarationSettings = consumerConfigurationContext.QueueDeclarationSettings ?? consumerSettings.QueueDeclarationSettings;

            IRabbitConsumer<T> consumer = _consumerFactory.CreateConsumer<T>(consumerSettings,
                                                                             lifecycleProfile,
                                                                             consumerConfigurationContext.MessageCallbackDelegate,
                                                                             consumerConfigurationContext.DeserializationDelegate,
                                                                             name: consumerConfigurationContext.Name);

            AddAsHostedServiceIfRequired(consumer, consumerConfigurationContext.AsHostedService);

            return this;
        }

        /// <summary>
        /// Builds and registers a <see cref="SingleFetchRabbitConsumer{T}"/> for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="consumerSettings"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddSingleFetchConsumer<T>(IRabbitConsumerSettings consumerSettings, Action<IRabbitConsumerBuilderConfigurationContext<T>> builderContext) where T : notnull
            => AddSingleFetchConsumer<T>(consumerSettings, ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile(), builderContext);

        /// <summary>
        /// Builds and registers a <see cref="SingleFetchRabbitConsumer{T}"/> for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="consumerSettings"></param>
        /// <param name="lifecycleProfile"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public RabbitHutchBuilder AddSingleFetchConsumer<T>(IRabbitConsumerSettings consumerSettings, IConnectionLifecycleProfile lifecycleProfile, Action<IRabbitConsumerBuilderConfigurationContext<T>> builderContext) where T : notnull
        {
            IRabbitConsumerBuilderConfigurationContext<T> consumerConfigurationContext = ExecuteConsumerConfigurationContext(builderContext);

            RegisterTypeIfNotExists<T>();

            consumerSettings.ExchangeDeclarationSettings = consumerConfigurationContext.ExchangeDeclarationSettings ?? consumerSettings.ExchangeDeclarationSettings;
            consumerSettings.QueueDeclarationSettings = consumerConfigurationContext.QueueDeclarationSettings ?? consumerSettings.QueueDeclarationSettings;

            IRabbitConsumer<T> consumer = _consumerFactory.CreateSingleFetchConsumer<T>(consumerSettings,
                                                                                        lifecycleProfile,
                                                                                        consumerConfigurationContext.MessageCallbackDelegate,
                                                                                        consumerConfigurationContext.DeserializationDelegate,
                                                                                        name: consumerConfigurationContext.Name);

            AddAsHostedServiceIfRequired(consumer, consumerConfigurationContext.AsHostedService);

            return this;
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
                _services.AddSingleton<IHostedService>(sp => new HostedRabbitWrapper<IRabbitPublisher<T>>(publisher));
            }
        }

        /// <summary>
        /// If the publisher is configured to be registered as a hosted service, then it is added to the service collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisher"></param>
        /// <param name="asHostedService"></param>
        private void AddAsHostedServiceIfRequired<T>(IRabbitConsumer<T> consumer, bool asHostedService)
        {
            if (asHostedService)
            {
                _services.AddSingleton<IHostedService>(sp => new HostedRabbitWrapper<IRabbitConsumer<T>>(consumer));
            }
        }
    }
}
