using Microsoft.Extensions.Logging;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Routing;
using RabbitHutch.Core.Serialization;
using RabbitHutch.Publishers.Interfaces;
using RabbitMQ.Client;

namespace RabbitHutch.Publishers
{
    public abstract class RabbitPublisherBase<T> : IRabbitPublisher<T>
    {
        protected IModel? _channel;
        protected IConnection? _connection;
        protected bool _publishSuccess;
        protected bool _isDisposed;

        private string? SslCert
        {
            get
            {
                if (Directory.Exists(RabbitConfiguration.KeysPath))
                {
                    try
                    {
                        return Directory.EnumerateFiles(RabbitConfiguration.KeysPath).FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError("{ex}", ex);
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        public ILogger? Logger { get; set; }

        /// <summary>
        /// Gets or sets the name of the publisher.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets or sets the RabbitConfiguration.
        /// </summary>
        public required IRabbitPublisherSettings RabbitConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the connection lifecycle profile.
        /// </summary>
        public required IConnectionLifecycleProfile LifecycleProfile { get; set; }

        /// <summary>
        /// Gets or sets the message serializer delegate.
        /// </summary>
        public required MessageSerializerDelegate<T> Serializer { get; set; }

        /// <summary>
        /// Gets or sets the routing key generator delegate.
        /// </summary>
        public required RoutingKeyGeneratorDelegate<T> RoutingKeyGenerator { get; set; }

        /// <summary>
        /// True if the underlying connection is open.
        /// </summary>
        public bool IsActive => _connection is not null && _connection.IsOpen;

        /// <summary>
        /// Initializes the RabbitMQ connection using the configured <see cref="IConnectionLifecycleProfile"/>.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> InitializeRabbitAsync() => await InitializeRabbitAsync(CancellationToken);

        /// <summary>
        /// Initializes the RabbitMQ connection using the configured <see cref="IConnectionLifecycleProfile"/>.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> InitializeRabbitAsync(CancellationToken cancellationToken)
        {
            int retryCount = -1;
            while (IsActive is false && retryCount <= LifecycleProfile.MaxRetries)
            {
                try
                {
                    ConnectionFactory factory = new()
                    {
                        Uri = RabbitConfiguration.ConnectionString,
                        AutomaticRecoveryEnabled = RabbitConfiguration.AutomaticRecovery,
                        Ssl = new SslOption()
                        {
                            Enabled = !string.IsNullOrEmpty(SslCert),
                            CertPath = SslCert,
                            AcceptablePolicyErrors = System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch
                        },
                        ClientProvidedName = $"{AppDomain.CurrentDomain.FriendlyName} on {Environment.MachineName}"
                    };

                    Logger?.LogDebug("About to create connection with {RabbitConfiguration.ConnectionString.Host}.", RabbitConfiguration.ConnectionString?.Host);

                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    if (RabbitConfiguration.EnableAcks)
                    {
                        Logger?.LogTrace($"Acks are enabled.");

                        _channel.BasicAcks += (sender, eventArgs) => { _publishSuccess = true; };
                        _channel.ConfirmSelect();
                    }
                }
                catch (Exception ex)
                {
                    Logger?.LogError("ERROR: Failure while configuring {GetType().Name}: {ex}", GetType().Name, ex);
                }

                if (IsActive is false)
                {
                    // If max retries is set to 0, we don't retry.
                    if (LifecycleProfile.MaxRetries <= 0)
                    {
                        return false;
                    }

                    retryCount++;
                    await Task.Delay(LifecycleProfile.ReconnectDelay, cancellationToken);
                }
            }

            return IsActive;
        }

        public abstract Task StartAsync(CancellationToken cancellationToken);
        public abstract Task StopAsync(CancellationToken cancellationToken);

        public abstract bool Publish(T message, Dictionary<string, object>? headers = null);
        public abstract bool Publish(T item, string routingKey, string contentType, Dictionary<string, object>? headers = null);

        public abstract Task<bool> PublishAsync(T message, Dictionary<string, object>? headers = null);
        public abstract Task<bool> PublishAsync(T item, string routingKey, string contentType, Dictionary<string, object>? headers = null);


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            _isDisposed = true;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        /// unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (disposing is false || _isDisposed)
                return;

            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
