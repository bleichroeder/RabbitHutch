using RabbitHutch.Consumers.Interfaces;
using System.Net.Mime;

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
        /// Gets or sets the value indicating whether to declare exchange.
        /// </summary>
        public bool DeclareExchange { get; set; }

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

        /// <summary>
        /// Creates <see cref="RabbitConsumerSettings"/> settings.
        /// </summary>
        public RabbitConsumerSettings() { }

        /// <summary>
        /// Creates <see cref="RabbitConsumerSettings"/> settings.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        public RabbitConsumerSettings(Uri connectionString,
                                      string exchangeName,
                                      string queueName)
            : this(connectionString, exchangeName, queueName, null)
        { }

        /// <summary>
        /// Creates <see cref="RabbitConsumerSettings"/> settings.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        public RabbitConsumerSettings(string connectionString,
                                      string exchangeName,
                                      string queueName)
            : this(new Uri(connectionString), exchangeName, queueName, null)
        { }

        /// <summary>
        /// Creates <see cref="RabbitConsumerSettings"/> settings.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKeys"></param>
        public RabbitConsumerSettings(Uri connectionString,
                                      string exchangeName,
                                      string queueName,
                                      string[]? routingKeys)
            : this(connectionString, exchangeName, queueName, routingKeys, null)
        { }

        /// <summary>
        /// Creates <see cref="RabbitConsumerSettings"/> settings.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKeys"></param>
        public RabbitConsumerSettings(string connectionString,
                                      string exchangeName,
                                      string queueName,
                                      string[]? routingKeys)
            : this(new Uri(connectionString), exchangeName, queueName, routingKeys, null)
        { }

        /// <summary>
        /// Creates <see cref="RabbitConsumerSettings"/> settings.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKeys"></param>
        /// <param name="prefetchCount"></param>
        public RabbitConsumerSettings(Uri connectionString,
                                      string exchangeName,
                                      string queueName,
                                      string[]? routingKeys,
                                      ushort? prefetchCount)
            : this(connectionString, exchangeName, queueName, routingKeys, prefetchCount, null, null, null, null, null)
        { }

        /// <summary>
        /// Creates <see cref="RabbitConsumerSettings"/> settings.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKeys"></param>
        /// <param name="prefetchCount"></param>
        public RabbitConsumerSettings(string connectionString,
                                      string exchangeName,
                                      string queueName,
                                      string[]? routingKeys,
                                      ushort? prefetchCount)
            : this(new Uri(connectionString), exchangeName, queueName, routingKeys, prefetchCount, null, null, null, null, null)
        { }

        /// <summary>
        /// Creates <see cref="RabbitConsumerSettings"/> settings.
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="connectionString"></param>
        /// <param name="keysPath"></param>
        /// <param name="enableAcks"></param>
        /// <param name="contentType"></param>
        public RabbitConsumerSettings(string connectionString,
                                      string exchangeName,
                                      string queueName,
                                      string[]? routingKeys,
                                      ushort? prefetchCount,
                                      string? keysPath,
                                      bool? autoAck,
                                      bool? nackOnFalse,
                                      bool? requeueOnNack,
                                      Uri? managementBaseUri)
            : this(new Uri(connectionString),
                   exchangeName,
                   queueName,
                   routingKeys,
                   prefetchCount,
                   keysPath,
                   autoAck,
                   nackOnFalse,
                   requeueOnNack,
                   managementBaseUri)
        { }

        /// <summary>
        /// Creates <see cref="RabbitConsumerSettings"/> settings.
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="connectionString"></param>
        /// <param name="keysPath"></param>
        /// <param name="enableAcks"></param>
        /// <param name="contentType"></param>
        public RabbitConsumerSettings(Uri connectionString,
                                      string exchangeName,
                                      string queueName,
                                      string[]? routingKeys,
                                      ushort? prefetchCount,
                                      string? keysPath,
                                      bool? autoAck,
                                      bool? nackOnFalse,
                                      bool? requeueOnNack,
                                      Uri? managementBaseUri)
        {
            ExchangeName = exchangeName;
            ConnectionString = connectionString;
            QueueName = queueName;
            RoutingKeys = routingKeys ?? ["#"];
            PrefetchCount = prefetchCount ?? 20;
            KeysPath = keysPath;
            AutoAck = autoAck ?? false;
            NackOnFalse = nackOnFalse ?? false;
            RequeueOnNack = requeueOnNack ?? false;
            ManagementBaseUri = managementBaseUri;
        }
    }
}
