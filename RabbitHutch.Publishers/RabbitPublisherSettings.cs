using RabbitHutch.Publishers.Interfaces;
using System.Net.Mime;

namespace RabbitHutch.Publishers
{
    /// <summary>
    /// Placeholder for RabbitMQ publisher settings.
    /// </summary>
    public class RabbitPublisherSettings : IRabbitPublisherSettings
    {
        /// <summary>
        /// Gets or sets the ExchangeName.
        /// </summary>
        public string? ExchangeName { get; set; }

        /// <summary>
        /// Gets or sets the ConnectionString.
        /// </summary>
        public Uri? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the KeysPath.
        /// If configured, the publisher will load all keys from the specified path.
        /// </summary>
        public string? KeysPath { get; set; }

        /// <summary>
        /// Gets or sets EnableAcks.
        /// </summary>
        public bool EnableAcks { get; set; }

        /// <summary>
        /// Gets or sets the ContentType.
        /// Defaults to <see cref="MediaTypeNames.Application.Json"/>.
        /// </summary>
        public string ContentType { get; set; } = MediaTypeNames.Application.Json;

        /// <summary>
        /// Gets or sets the AutomaticRecovery.
        /// Enabled by default.
        /// </summary>
        public bool AutomaticRecovery { get; set; } = true;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RabbitPublisherSettings() { }

        /// <summary>
        /// Creates <see cref="IRabbitPublisher{T}"/> settings.
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="connectionString"></param>
        /// <param name="keysPath"></param>
        /// <param name="enableAcks"></param>
        /// <param name="contentType"></param>
        public RabbitPublisherSettings(string? exchangeName,
                                       Uri? connectionString,
                                       string? keysPath,
                                       bool enableAcks,
                                       string contentType)
        {
            ExchangeName = exchangeName;
            ConnectionString = connectionString;
            KeysPath = keysPath;
            EnableAcks = enableAcks;
            ContentType = contentType;
        }
    }
}