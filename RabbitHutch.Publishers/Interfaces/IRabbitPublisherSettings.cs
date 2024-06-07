using RabbitHutch.Core.Settings.Interfaces;

namespace RabbitHutch.Publishers.Interfaces
{
    /// <summary>
    /// The rabbit publisher settings interface.
    /// </summary>
    public interface IRabbitPublisherSettings : IRabbitSettings
    {
        /// <summary>
        /// Gets or sets the enable acks value.
        /// </summary>
        bool EnableAcks { get; set; }

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        string ContentType { get; set; }
    }
}
