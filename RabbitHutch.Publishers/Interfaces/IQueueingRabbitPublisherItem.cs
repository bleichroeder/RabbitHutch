namespace RabbitHutch.Publishers
{
    /// <summary>
    /// Represents a queueing rabbit publisher item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQueueingRabbitPublisherItem<T>
    {
        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        T Item { get; set; }

        /// <summary>
        /// Gets or sets the routing key.
        /// </summary>
        string RoutingKey { get; set; }

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content encoding.
        /// </summary>
        string ContentEncoding { get; set; }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        Dictionary<string, object>? Headers { get; set; }
    }
}
