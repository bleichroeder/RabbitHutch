using RabbitHutch.Consumers.Interfaces;

namespace RabbitHutch.Consumers
{
    public class RabbitConsumerSettings : IRabbitConsumerSettings
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public Uri? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the exchange name.
        /// </summary>
        public string? ExchangeName { get; set; }

        /// <summary>
        /// Gets or sets the queue name.
        /// </summary>
        public string? QueueName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether automatic recovery is enabled.
        /// </summary>
        public bool AutomaticRecovery { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to auto ack.
        /// </summary>
        public bool AutoAck { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to nack on false.
        /// </summary>
        public bool NackOnFalse { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to requeue on nack.
        /// </summary>
        public bool RequeueOnNack { get; set; }

        /// <summary>
        /// Gets or sets the keys path.
        /// </summary>
        public string? KeysPath { get; set; }

        /// <summary>
        /// Gets or sets the prefetch count.
        /// </summary>
        public ushort PrefetchCount { get; set; }

        /// <summary>
        /// Gets or sets the routing keys.
        /// Defaults to match all routing keys.
        /// </summary>
        public string[] RoutingKeys { get; set; } = ["#"];

        /// <summary>
        /// Gets or sets the management base URI.
        /// </summary>
        public Uri? ManagementBaseUri { get; set; }

        /// <summary>
        /// Gets the <see cref="ManagementBaseUri"/> user info.
        /// </summary>
        public string? ManagementUserInfo => ManagementBaseUri?.UserInfo;

        /// <summary>
        /// Gets the <see cref="ManagementBaseUri"/> user.
        /// </summary>
        public string? ManagementUser => ManagementUserInfo?.Split(':')[0];

        /// <summary>
        /// Gets the <see cref="ManagementBaseUri"/> password.
        /// </summary>
        public string ManagementPassword => ManagementUserInfo?.Split(":").Length > 1 ? ManagementUserInfo.Split(":")[1] : string.Empty;

        /// <summary>
        /// Gets the <see cref="ManagementBaseUri"/> virtual host.
        /// </summary>
        public string ManagementVirtualHost => Uri.UnescapeDataString(ConnectionString?.AbsolutePath ?? string.Empty);
    }
}
