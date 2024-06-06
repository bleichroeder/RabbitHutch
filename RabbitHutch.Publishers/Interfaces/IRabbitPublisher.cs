using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Routing;
using RabbitHutch.Core.Serialization;

namespace RabbitHutch.Publishers.Interfaces
{
    /// <summary>
    /// The IRabbitPublisher interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRabbitPublisher<T> : IDisposable, IHostedService
    {
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        ILogger? Logger { get; set; }

        /// <summary>
        /// Gets or sets the lifecycle profile.
        /// </summary>
        IConnectionLifecycleProfile LifecycleProfile { get; set; }

        /// <summary>
        /// Gets or sets the serializer.
        /// </summary>
        MessageSerializerDelegate<T> Serializer { get; set; }

        /// <summary>
        /// Gets or sets the routing key generator.
        /// </summary>
        RoutingKeyGeneratorDelegate<T> RoutingKeyGenerator { get; set; }

        /// <summary>
        /// True if the underlying connection is active.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Initializes the RabbitMQ connection.
        /// </summary>
        /// <returns></returns>
        Task<bool> InitializeRabbitAsync();

        /// <summary>
        /// Initializes the RabbitMQ connection.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> InitializeRabbitAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Publishes a message to the RabbitMQ exchange.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        bool Publish(T message, Dictionary<string, object>? headers = null);

        /// <summary>
        /// Publishes a message to the RabbitMQ exchange.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        Task<bool> PublishAsync(T message, Dictionary<string, object>? headers = null);

        /// <summary>
        /// Publishes a message to the RabbitMQ exchange.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="routingKey"></param>
        /// <param name="contentType"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        bool Publish(T item, string routingKey, string contentType, Dictionary<string, object>? headers = null);

        /// <summary>
        /// Publishes a message to the RabbitMQ exchange.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="routingKey"></param>
        /// <param name="contentType"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        Task<bool> PublishAsync(T item, string routingKey, string contentType, Dictionary<string, object>? headers = null);
    }
}
