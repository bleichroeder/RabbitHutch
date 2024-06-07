using Microsoft.Extensions.Logging;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Routing;
using RabbitHutch.Core.Serialization;
using RabbitHutch.Core.Settings.Interfaces;

namespace RabbitHutch.DependencyInjection.Interfaces
{
    /// <summary>
    /// Interface for Rabbit publisher builder configuration context.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRabbitPublisherBuilderConfigurationContext<T>
    {
        /// <summary>
        /// Gets the publisher Name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        ILogger? Logger { get; }

        /// <summary>
        /// True if the publisher is registered as a hosted service.
        /// </summary>
        bool AsHostedService { get; }

        /// <summary>
        /// Gets the serializer delegate.
        /// </summary>
        MessageSerializerDelegate<T> SerializerDelegate { get; }

        /// <summary>
        /// Gets the routing key delegate.
        /// </summary>
        RoutingKeyGeneratorDelegate<T> RoutingKeyDelegate { get; }

        /// <summary>
        /// Gets the exchange declaration settings.
        /// </summary>
        IExchangeDeclarationSettings? ExchangeDeclarationSettings { get; }

        /// <summary>
        /// Sets the serializer.
        /// </summary>
        /// <param name="serializerDelegate"></param>
        RabbitPublisherBuilderConfigurationContext<T> WithSerializer(Func<T, string> serializerDelegate);

        /// <summary>
        /// Sets the routing key formatter.
        /// </summary>
        /// <param name="routingKeyGenerator"></param>
        RabbitPublisherBuilderConfigurationContext<T> WithRoutingKeyFormatter(Func<T, string> routingKeyGenerator);

        /// <summary>
        /// Registers the publisher as a hosted service.
        /// </summary>
        RabbitPublisherBuilderConfigurationContext<T> RegisterAsHostedService();

        /// <summary>
        /// Sets the connection lifecycle profile.
        /// </summary>
        /// <param name="connectionLifecycleProfile"></param>
        RabbitPublisherBuilderConfigurationContext<T> WithConnectionLifecycleProfile(IConnectionLifecycleProfile connectionLifecycleProfile);

        /// <summary>
        /// Sets the publisher name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        RabbitPublisherBuilderConfigurationContext<T> WithName(string name);

        /// <summary>
        /// Specifies a custom logger for the publisher.
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public RabbitPublisherBuilderConfigurationContext<T> WithLogger(ILogger logger);

        /// <summary>
        /// Sets the exchange declaration settings.
        /// </summary>
        /// <param name="exchangeDeclarationSettings"></param>
        /// <returns></returns>
        public RabbitPublisherBuilderConfigurationContext<T> WithExchangeDeclarationSettings(IExchangeDeclarationSettings exchangeDeclarationSettings);
    }
}
