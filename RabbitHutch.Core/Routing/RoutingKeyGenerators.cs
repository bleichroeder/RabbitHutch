namespace RabbitHutch.Core.Routing
{
    /// <summary>
    /// The RoutingKeyGeneratorDelegate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    public delegate string RoutingKeyGeneratorDelegate<T>(T input);

    /// <summary>
    /// Provides a set of static methods for <see cref="RoutingKeyGeneratorDelegate{T}"/>.
    /// </summary>
    public static class RoutingKeyGenerators
    {
        private const string DEFAULT_ROUTING_KEY = "#";

        /// <summary>
        /// Creates a default routing key generator with a routing key of "#".
        /// The "#" routing key is a special routing key that will match any routing key, and is used for topic exchanges.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static RoutingKeyGeneratorDelegate<T> DefaultRoutingKeyGenerator<T>() => item => DEFAULT_ROUTING_KEY;

        /// <summary>
        /// Creates a default routing key generator with a speciic static routing key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="routingKey"></param>
        /// <returns></returns>
        public static RoutingKeyGeneratorDelegate<T> DefaultRoutingKeyGenerator<T>(string routingKey) => item => routingKey;

        /// <summary>
        /// Creates a custom routing key generator with a specific function.
        /// Routing key can be generated based on the properties of the message.
        /// <br/>
        /// <br/>
        /// Example: HeadersExchange RoutingKey<br/>
        ///          (message) => $"format={message.Format}"
        /// <br/>
        /// <br/>
        /// Example: DirectExchange<br/>
        ///          (message) => message.LogLevel.ToLowerInvariant()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        /// <returns></returns>
        public static RoutingKeyGeneratorDelegate<T> CustomRoutingKeyGenerator<T>(Func<T, string> function) => new(function.Invoke);
    }
}
