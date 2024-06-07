namespace RabbitHutch.Core.Settings.Interfaces
{
    /// <summary>
    /// The rabbit settings interface.
    /// </summary>
    public interface IRabbitSettings
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        Uri? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the exchange name.
        /// </summary>
        string? ExchangeName { get; set; }

        /// <summary>
        /// Gets or sets the routing key.
        /// </summary>
        string? KeysPath { get; set; }

        /// <summary>
        /// Gets or sets the queue name.
        /// </summary>
        bool AutomaticRecovery { get; set; }

        /// <summary>
        /// Gets or sets the exchange type.
        /// </summary>
        bool DeclareExchange { get; }

        /// <summary>
        /// Gets or sets the exchange declaration settings.
        /// </summary>
        IExchangeDeclarationSettings? ExchangeDeclarationSettings { get; set; }
    }
}
