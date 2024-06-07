using Microsoft.Extensions.Logging;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Delegates;
using RabbitHutch.Core.Serialization;
using RabbitHutch.Core.Settings.Interfaces;
using RabbitHutch.DependencyInjection.Interfaces;

namespace RabbitHutch.DependencyInjection
{
    /// <summary>
    /// The RabbitPublisherBuilderConfigurationContext.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RabbitConsumerBuilderConfigurationContext<T> : IRabbitConsumerBuilderConfigurationContext<T>
    {
        /// <summary>
        /// Gets the publisher Name.
        /// </summary>
        public string Name { get; private set; } = typeof(T).Name;

        /// <summary>
        /// Gets the custom logger.
        /// </summary>
        public ILogger? Logger { get; private set; }

        /// <summary>
        /// Gets the serializer delegate.
        /// </summary>
        public MessageDeserializerFromBytesDelegate<T> DeserializationDelegate { get; private set; } = MessageDeserializers.DefaultMessageDeserializerFromBytes<T>();

        /// <summary>
        /// Gets the routing key delegate.
        /// </summary>
        public AsyncNewMessageCallbackDelegate<T> MessageCallbackDelegate { get; private set; } = MessageReceivedDelegates.DefaultNewMessageCallback<T>();

        /// <summary>
        /// Gets the connection lifecycle profile.
        /// </summary>
        public IConnectionLifecycleProfile ConnectionLifecycleProfile { get; private set; } = ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile();

        /// <summary>
        /// Gets or sets a value indicating whether the publisher is registered as a hosted service.
        /// </summary>
        public bool AsHostedService { get; private set; }

        /// <summary>
        /// Gets the exchange declaration settings.
        /// </summary>
        public IExchangeDeclarationSettings? ExchangeDeclarationSettings { get; private set; }

        /// <summary>
        /// Gets the exchange declaration settings.
        /// </summary>
        public IQueueDeclarationSettings? QueueDeclarationSettings { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitPublisherBuilderConfigurationContext{T}"/> class.
        /// </summary>
        /// <param name="serializerDelegate"></param>
        /// <returns></returns>
        public RabbitConsumerBuilderConfigurationContext<T> WithDeserializer(Func<byte[], T?> serializerDelegate)
        {
            DeserializationDelegate = new MessageDeserializerFromBytesDelegate<T>(serializerDelegate);
            return this;
        }

        /// <summary>
        /// Sets the routing key formatter.
        /// </summary>
        /// <param name="routingKeyGenerator"></param>
        /// <returns></returns>
        public RabbitConsumerBuilderConfigurationContext<T> WithNewMessageDelegate(Func<T, Task<bool>> messageRecievedDelegate)
        {
            MessageCallbackDelegate = new AsyncNewMessageCallbackDelegate<T>(messageRecievedDelegate);
            return this;
        }

        /// <summary>
        /// Sets the connection lifecycle profile.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public RabbitConsumerBuilderConfigurationContext<T> WithConnectionLifecycleProfile(IConnectionLifecycleProfile profile)
        {
            ConnectionLifecycleProfile = profile;
            return this;
        }

        /// <summary>
        /// Registers the publisher as a hosted service.
        /// </summary>
        /// <returns></returns>
        public RabbitConsumerBuilderConfigurationContext<T> RegisterAsHostedService()
        {
            AsHostedService = true;
            return this;
        }

        /// <summary>
        /// Sets the publisher name. The default name is the type name.
        /// Using a custom name allows for multiple publishers of the same type.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public RabbitConsumerBuilderConfigurationContext<T> WithName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Specifies a custom logger for the publisher.
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public RabbitConsumerBuilderConfigurationContext<T> WithLogger(ILogger logger)
        {
            Logger = logger;
            return this;
        }

        /// <summary>
        /// Sets the exchange declaration settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public RabbitConsumerBuilderConfigurationContext<T> WithExchangeDeclarationSettings(IExchangeDeclarationSettings settings)
        {
            ExchangeDeclarationSettings = settings;
            return this;
        }

        /// <summary>
        /// Sets the queue declaration settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public RabbitConsumerBuilderConfigurationContext<T> WithQueueDeclarationSettings(IQueueDeclarationSettings settings)
        {
            QueueDeclarationSettings = settings;
            return this;
        }
    }
}
