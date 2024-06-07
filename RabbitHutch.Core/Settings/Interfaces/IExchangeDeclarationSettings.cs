namespace RabbitHutch.Core.Settings.Interfaces
{
    /// <summary>
    /// The exchange declaration settings interface.
    /// </summary>
    public interface IExchangeDeclarationSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the exchange declaration is passive.
        /// </summary>
        bool Passive { get; set; }

        /// <summary>
        /// Gets or sets the exchange name.
        /// </summary>
        string ExchangeType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the exchange is durable.
        /// </summary>
        bool Durable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the exchange should be auto-deleted.
        /// </summary>
        bool AutoDelete { get; set; }

        /// <summary>
        /// Gets or sets the exchange arguments.
        /// </summary>
        IDictionary<string, object>? Arguments { get; set; }
    }
}
