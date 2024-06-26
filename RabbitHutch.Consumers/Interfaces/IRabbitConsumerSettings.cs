﻿using RabbitHutch.Core.Settings.Interfaces;

namespace RabbitHutch.Consumers.Interfaces
{
    public interface IRabbitConsumerSettings : IRabbitSettings
    {
        /// <summary>
        /// Gets or sets the queue declaration settings.
        /// </summary>
        IQueueDeclarationSettings? QueueDeclarationSettings { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether to declare the queue.
        /// </summary>
        bool DeclareQueue { get; }

        /// <summary>
        /// Gets or sets the queue name.
        /// </summary>
        string? QueueName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to auto ack.
        /// </summary>
        bool AutoAck { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to nack on false.
        /// </summary>
        bool NackOnFalse { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to requeue on nack.
        /// </summary>
        bool RequeueOnNack { get; set; }

        /// <summary>
        /// Gets or sets the prefetch count.
        /// </summary>
        ushort PrefetchCount { get; set; }

        /// <summary>
        /// Gets or sets the routing keys.
        /// </summary>
        string[] RoutingKeys { get; set; }

        /// <summary>
        /// Gets or sets the management base URI.
        /// </summary>
        Uri? ManagementBaseUri { get; set; }

        /// <summary>
        /// Gets the <see cref="ManagementBaseUri"/> user info.
        /// </summary>
        string? ManagementUserInfo { get; }

        /// <summary>
        /// Gets the <see cref="ManagementBaseUri"/> user.
        /// </summary>
        string? ManagementUser { get; }

        /// <summary>
        /// Gets the <see cref="ManagementBaseUri"/> password.
        /// </summary>
        string ManagementPassword { get; }

        /// <summary>
        /// Gets the <see cref="ManagementBaseUri"/> virtual host.
        /// </summary>
        string ManagementVirtualHost { get; }
    }
}
