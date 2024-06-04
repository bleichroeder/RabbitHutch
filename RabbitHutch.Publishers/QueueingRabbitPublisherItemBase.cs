using System.Text;

namespace RabbitHutch.Publishers
{
    /// <summary>
    /// Represents a queueing rabbit publisher item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueueingRabbitPublisherItemBase<T> : IQueueingRabbitPublisherItem<T>
    {
        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        public T Item { get; set; } = default!;

        /// <summary>
        /// Gets or sets the routing key.
        /// </summary>
        public string RoutingKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the content encoding.
        /// </summary>
        public string ContentEncoding { get; set; } = Encoding.UTF8.WebName;

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        public Dictionary<string, object>? Headers { get; set; }
    }
}
