using RabbitHutch.Core.Settings.Interfaces;
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
        /// True if the exchange should be declared.
        /// </summary>
        public bool DeclareExchange => ExchangeDeclarationSettings is not null;

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
        /// Gets or sets the exchange declaration settings.
        /// </summary>
        public IExchangeDeclarationSettings? ExchangeDeclarationSettings { get; set; }

        /// <summary>
        /// Creates <see cref="RabbitPublisherSettings"/>.
        /// </summary>
        public RabbitPublisherSettings() { }

        /// <summary>
        /// Creates <see cref="RabbitPublisherSettings"/>.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="exchangeName"></param>
        public RabbitPublisherSettings(string connectionString, string exchangeName)
            : this(new Uri(connectionString), exchangeName, null, null, false)
        { }

        /// <summary>
        /// Creates <see cref="RabbitPublisherSettings"/>.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="exchangeName"></param>
        public RabbitPublisherSettings(Uri connectionString, string exchangeName)
            : this(connectionString, exchangeName, null, null, false)
        { }

        /// <summary>
        /// Creates <see cref="RabbitPublisherSettings"/>.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="exchangeName"></param>
        public RabbitPublisherSettings(string connectionString,
                                       string exchangeName,
                                       string contentType)
            : this(new Uri(connectionString), exchangeName, null, contentType, false)
        { }

        /// <summary>
        /// Creates <see cref="RabbitPublisherSettings"/>.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="exchangeName"></param>
        public RabbitPublisherSettings(Uri connectionString,
                                       string exchangeName,
                                       string contentType)
            : this(connectionString, exchangeName, null, contentType, false)
        { }

        /// <summary>
        /// Creates <see cref="RabbitPublisherSettings"/>.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="exchangeName"></param>
        public RabbitPublisherSettings(string connectionString,
                                       string exchangeName,
                                       string keysPath,
                                       string contentType)
            : this(new Uri(connectionString), exchangeName, keysPath, contentType, false)
        { }

        /// <summary>
        /// Creates <see cref="RabbitPublisherSettings"/>.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="exchangeName"></param>
        public RabbitPublisherSettings(Uri connectionString,
                                       string exchangeName,
                                       string keysPath,
                                       string contentType)
            : this(connectionString, exchangeName, keysPath, contentType, false)
        { }

        /// <summary>
        /// Creates <see cref="RabbitPublisherSettings"/>.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="exchangeName"></param>
        /// <param name="keysPath"></param>
        /// <param name="contentType"></param>
        /// <param name="enableAcks"></param>
        public RabbitPublisherSettings(string connectionString,
                                       string exchangeName,
                                       string keysPath,
                                       string contentType,
                                       bool enableAcks)
            : this(new Uri(connectionString), exchangeName, keysPath, contentType, enableAcks)
        { }

        /// <summary>
        /// Creates <see cref="RabbitPublisherSettings"/> settings.
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="connectionString"></param>
        /// <param name="keysPath"></param>
        /// <param name="enableAcks"></param>
        /// <param name="contentType"></param>
        public RabbitPublisherSettings(string connectionString,
                                       string exchangeName,
                                       string? keysPath,
                                       string? contentType,
                                       bool? enableAcks)
            : this(new Uri(connectionString), exchangeName, keysPath, contentType, enableAcks)
        { }

        /// <summary>
        /// Creates <see cref="RabbitPublisherSettings"/> settings.
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="connectionString"></param>
        /// <param name="keysPath"></param>
        /// <param name="enableAcks"></param>
        /// <param name="contentType"></param>
        public RabbitPublisherSettings(Uri connectionString,
                                       string exchangeName,
                                       string? keysPath,
                                       string? contentType,
                                       bool? enableAcks)
        {
            ExchangeName = exchangeName;
            ConnectionString = connectionString;
            KeysPath = keysPath;
            EnableAcks = enableAcks ?? false;
            ContentType = contentType ?? MediaTypeNames.Application.Json;
        }
    }
}