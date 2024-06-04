using RabbitHutch.Core.ConnectionLifecycle;

namespace RabbitHutch.DependencyInjection
{
    /// <summary>
    /// Interface for Rabbit publisher builder configuration context.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRabbitPublisherBuilderConfigurationContext<T>
    {
        /// <summary>
        /// True if the publisher is registered as a hosted service.
        /// </summary>
        bool AsHostedService { get; }

        /// <summary>
        /// Gets the serializer delegate.
        /// </summary>
        Func<T, string>? SerializerDelegate { get; }

        /// <summary>
        /// Gets the routing key delegate.
        /// </summary>
        Func<T, string>? RoutingKeyDelegate { get; }

        /// <summary>
        /// Sets the serializer.
        /// </summary>
        /// <param name="serializerDelegate"></param>
        void WithSerializer(Func<T, string> serializerDelegate);

        /// <summary>
        /// Sets the routing key formatter.
        /// </summary>
        /// <param name="routingKeyGenerator"></param>
        void WithRoutingKeyFormatter(Func<T, string> routingKeyGenerator);

        /// <summary>
        /// Registers the publisher as a hosted service.
        /// </summary>
        void RegisterAsHostedService();

        /// <summary>
        /// Sets the connection lifecycle profile.
        /// </summary>
        /// <param name="connectionLifecycleProfile"></param>
        void WithConnectionLifecycleProfile(IConnectionLifecycleProfile connectionLifecycleProfile);
    }
}
