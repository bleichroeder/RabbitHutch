using RabbitHutch.Core.Settings.Interfaces;

namespace RabbitHutch.Core.Settings
{
    /// <summary>
    /// The queue declaration settings.
    /// </summary>
    public class QueueDeclarationSettings : IQueueDeclarationSettings
    {
        /// <summary>
        /// Gets or sets the value indicating whether the queue declaration is passive.
        /// </summary>
        public bool Passive { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the queue declaration is passive.
        /// </summary>
        public bool Exclusive { get; set; } = true;

        /// <summary>
        /// Gets or sets the queue type.
        /// Defaults to Classic.
        /// RabbitMQ does not provide constants for the queue types.
        /// Valid values are: classic, quorum, and lazy.
        /// </summary>
        public string QueueType { get; set; } = "classic";

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
