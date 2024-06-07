using RabbitHutch.Core.Settings.Interfaces;

namespace RabbitHutch.Core.Settings
{
    /// <summary>
    /// The exchange declaration settings.
    /// </summary>
    public class ExchangeDeclarationSettings : IExchangeDeclarationSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the exchange declaration is passive.
        /// </summary>
        public bool Passive { get; set; } = true;

        /// <summary>
        /// Gets or sets the queue type.
        /// Defaults to Topic.
        /// Use the <see cref="RabbitMQ.Client.ExchangeType"/> class for valid values.
        /// </summary>
        public string ExchangeType { get; set; } = RabbitMQ.Client.ExchangeType.Topic;

        /// <summary>
        /// Gets or sets the value indicating whether a queue is durable.
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// Gets or sets the value inidcating whether a queue should be auto deleted.
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        public IDictionary<string, object>? Arguments { get; set; }
    }
}
