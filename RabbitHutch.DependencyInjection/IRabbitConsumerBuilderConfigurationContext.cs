using Microsoft.Extensions.Logging;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Delegates;
using RabbitHutch.Core.Serialization;

namespace RabbitHutch.DependencyInjection
{
    /// <summary>
    /// Interface for Rabbit publisher builder configuration context.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRabbitConsumerBuilderConfigurationContext<T>
    {
        /// <summary>
        /// Gets the publisher Name.
        /// </summary>
        string Name { get; }

        ILogger? Logger { get; }

        /// <summary>
        /// True if the publisher is registered as a hosted service.
        /// </summary>
        bool AsHostedService { get; }

        /// <summary>
        /// Gets the serializer delegate.
        /// </summary>
        MessageDeserializerFromBytesDelegate<T> DeserializationDelegate { get; }

        /// <summary>
        /// Gets the message recieved delegate.
        /// </summary>
        AsyncNewMessageCallbackDelegate<T> MessageCallbackDelegate { get; }

        /// <summary>
        /// Sets the serializer.
        /// </summary>
        /// <param name="serializerDelegate"></param>
        RabbitConsumerBuilderConfigurationContext<T> WithDeserializer(Func<byte[], T?> deserializerFunction);

        /// <summary>
        /// Sets the routing key formatter.
        /// </summary>
        /// <param name="routingKeyGenerator"></param>
        RabbitConsumerBuilderConfigurationContext<T> WithNewMessageDelegate(Func<T, Task<bool>> messageRecievedDelegate);

        /// <summary>
        /// Registers the publisher as a hosted service.
        /// </summary>
        RabbitConsumerBuilderConfigurationContext<T> RegisterAsHostedService();

        /// <summary>
        /// Sets the connection lifecycle profile.
        /// </summary>
        /// <param name="connectionLifecycleProfile"></param>
        RabbitConsumerBuilderConfigurationContext<T> WithConnectionLifecycleProfile(IConnectionLifecycleProfile connectionLifecycleProfile);

        /// <summary>
        /// Sets the publisher name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        RabbitConsumerBuilderConfigurationContext<T> WithName(string name);

        /// <summary>
        /// Sets the logger.
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        RabbitConsumerBuilderConfigurationContext<T> WithLogger(ILogger logger);
    }
}
