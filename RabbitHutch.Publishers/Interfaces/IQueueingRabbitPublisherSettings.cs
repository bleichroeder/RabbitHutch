namespace RabbitHutch.Publishers.Interfaces
{
    /// <summary>
    /// The rabbit publisher settings interface.
    /// </summary>
    public interface IQueueingRabbitPublisherSettings : IRabbitPublisherSettings
    {
        /// <summary>
        /// Gets or sets the maximum queue depth.
        /// </summary>
        int MaxQueueDepth { get; set; }
    }
}
