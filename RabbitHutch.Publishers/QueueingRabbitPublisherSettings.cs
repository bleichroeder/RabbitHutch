using RabbitHutch.Publishers.Interfaces;

namespace RabbitHutch.Publishers
{
    /// <summary>
    /// Placeholder for RabbitMQ publisher settings.
    /// </summary>
    public class QueueingRabbitPublisherSettings : RabbitPublisherSettings, IQueueingRabbitPublisherSettings
    {
        /// <summary>
        /// Gets or sets the maximum queue depth.
        /// Defaults to 1000.
        /// </summary>
        public int MaxQueueDepth { get; set; } = -1;
    }
}