namespace RabbitHutch.Core.Settings.Interfaces
{
    /// <summary>
    /// The queue declaration settings interface.
    /// </summary>
    public interface IQueueDeclarationSettings
    {
        /// <summary>
        /// Gets or sets the value indicating whether the queue declaration is passive.
        /// </summary>
        bool Passive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the queue declaration is passive.
        /// </summary>
        bool Exclusive { get; set; }

        /// <summary>
        /// Gets or sets the queue name.
        /// </summary>
        string QueueType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the queue is durable.
        /// </summary>
        bool Durable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the queue should be auto-deleted.
        /// </summary>
        bool AutoDelete { get; set; }

        /// <summary>
        /// Gets or sets the queue arguments.
        /// </summary>
        IDictionary<string, object>? Arguments { get; set; }
    }
}
