namespace RabbitHutch.Publishers.Interfaces
{
    /// <summary>
    /// The rabbit publisher settings interface.
    /// </summary>
    public interface IRabbitPublisherSettings
    {
        /// <summary>
        /// Gets or sets the exchange name.
        /// </summary>
        string? ExchangeName { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether to declare exchange.
        /// </summary>
        bool DeclareExchange { get; set; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        Uri? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the keys path.
        /// </summary>
        string? KeysPath { get; set; }

        /// <summary>
        /// Gets or sets the enable acks value.
        /// </summary>
        bool EnableAcks { get; set; }

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the automatic recovery value.
        /// </summary>
        bool AutomaticRecovery { get; set; }
    }
}
