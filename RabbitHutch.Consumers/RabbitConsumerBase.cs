using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitHutch.Consumers.Interfaces;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Delegates;
using RabbitHutch.Core.Routing;
using RabbitHutch.Core.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net;
using System.Text.Json;

namespace RabbitHutch.Consumers
{
    /// <summary>
    /// Represents a RabbitMQ Consumer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RabbitConsumerBase<T> : IRabbitConsumer<T>, IDisposable, IHostedService
    {
        protected IModel? _model;
        protected IConnection? _connection;
        protected EventingBasicConsumer? _inputConsumer;
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
        /// Gets or sets the name of the consumer.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets or sets the OnReceivedEvent.
        /// </summary>
        public EventHandler<BasicDeliverEventArgs>? OnReceivedEvent { get; set; }

        /// <summary>
        /// Gets or sets the RabbitConfiguration.
        /// </summary>
        public required IRabbitConsumerSettings RabbitConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the connection lifecycle profile.
        /// </summary>
        public required IConnectionLifecycleProfile LifecycleProfile { get; set; }

        /// <summary>
        /// Gets or sets the message deserializer delegate.
        /// </summary>
        public required MessageDeserializerFromBytesDelegate<T> Deserializer { get; set; }

        /// <summary>
        /// Gets or sets the new message callback delegate.
        /// </summary>
        public required AsyncNewMessageCallbackDelegate<T> MessageCallbackDelegate { get; set; }

        /// <summary>
        /// True if the underlying connection is open.
        /// </summary>
        public bool IsActive => _connection is not null && _connection.IsOpen && _model is not null && _model.IsClosed is false;

        /// <summary>
        /// Creates the default received delegate using the configured new message callback and deserializer.
        /// </summary>
        public EventHandler<BasicDeliverEventArgs> DefaultReceivedDelegate => async (ch, ea) =>
        {
            Logger?.LogDebug("New Message Received: {ea.RoutingKey} [{ea.Body.Length} bytes]", ea.RoutingKey, ea.Body.Length);

            T? deserializedType = Deserializer(ea.Body.ToArray());

            if (deserializedType is null)
            {
                Logger?.LogWarning("WARNING: Unable to deserialize the message [{tag}]", ea.DeliveryTag);
                return;
            }

            // Get the delegate result using the configured delegate.
            bool delegateResult = await MessageCallbackDelegate(deserializedType);

            // If success we ack otherwise we nack.
            if (delegateResult)
            {
                _model?.BasicAck(ea.DeliveryTag, false);
            }
            else
            {
                if (RabbitConfiguration.NackOnFalse)
                    _model?.BasicNack(ea.DeliveryTag, false, RabbitConfiguration.RequeueOnNack);
            }

            // Set the result of the TaskCompletionSource
            if (ch is TaskCompletionSource<bool> tcs)
            {
                tcs.SetResult(delegateResult);
            }
        };

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
                    _model = _connection.CreateModel();

                    _model.BasicQos(0, RabbitConfiguration.PrefetchCount, false);

                    if (RabbitConfiguration.DeclareExchange) DeclareExchange();

                    if (RabbitConfiguration.DeclareQueue) DeclareQueue();

                    ConfigureBindings();

                    if (RabbitConfiguration.ManagementBaseUri is not null)
                    {
                        try
                        {
                            await RemoveUnexpectedQueueBindingsAsync(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            Logger?.LogWarning("ERROR: Failed to remove unexpected queue bindings - {ex}", ex);
                        }
                    }

                    _inputConsumer = new EventingBasicConsumer(_model);

                    _inputConsumer.Shutdown += (ch, ea) =>
                    {
                        Logger?.LogWarning("WARNING: Model is shutting down.");
                    };

                    _inputConsumer.ConsumerCancelled += (ch, ea) =>
                    {
                        Logger?.LogWarning("WARNING: Consumer has been cancelled.");
                    };

                    _inputConsumer.Received += OnReceivedEvent ?? DefaultReceivedDelegate;

                }
                catch (Exception ex)
                {
                    Logger?.LogError("ERROR: Failure while configuring {GetType().Name}: {ex}", GetType().Name, ex);
                }

                if (IsActive is false)
                {
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

        /// <summary>
        /// Removes any unexpected queue bindings from the queue.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RemoveUnexpectedQueueBindingsAsync(CancellationToken cancellationToken)
        {
            if (RabbitConfiguration.ManagementBaseUri is null)
            {
                return false;
            }

            using HttpClient apiClient = new(new HttpClientHandler
            {
                Credentials = new NetworkCredential(RabbitConfiguration.ManagementUser,
                RabbitConfiguration.ManagementPassword)
            });

            string baseUrl = $"{RabbitConfiguration.ManagementBaseUri}{(RabbitConfiguration.ManagementBaseUri.ToString().EndsWith('/') ? "" : '/')}";
            string url = $"{baseUrl}queues/{Uri.EscapeDataString(RabbitConfiguration.ManagementVirtualHost)}/{Uri.EscapeDataString(RabbitConfiguration.QueueName ?? string.Empty)}/bindings";

            List<Binding>? currentBindings = [];
            bool success = false;
            while (!success)
            {
                try
                {
                    Logger?.LogInformation("Listing Bindings for Queue: {queueName}", RabbitConfiguration.QueueName);

                    HttpResponseMessage response = await apiClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                    currentBindings = JsonSerializer.Deserialize<List<Binding>>(responseString);

                    success = response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Logger?.LogError("Error GETting QueueBindings via Management API: {url}{Environment.NewLine}{ex}", url, Environment.NewLine, ex);
                    await Task.Delay(500, cancellationToken);
                }
            }

            if (currentBindings?.Count > 0)
            {
                List<Binding> bindingsToRemove = currentBindings;

                bindingsToRemove.RemoveAll(b => string.IsNullOrEmpty(b?.Source));
                bindingsToRemove.RemoveAll(b => RabbitConfiguration.RoutingKeys.Any(eb => eb.Equals(b.RoutingKey, StringComparison.OrdinalIgnoreCase)));

                foreach (Binding bindingToRemove in bindingsToRemove)
                {
                    Logger?.LogDebug("Removing Unused Binding for Queue: {queueName} ({bindingToRemove.RoutingKey})", RabbitConfiguration.QueueName, bindingToRemove.RoutingKey);

                    success = false;
                    while (success is false)
                    {
                        string removeUrl = $"{baseUrl}bindings/{Uri.EscapeDataString(RabbitConfiguration.ManagementVirtualHost)}/e/{Uri.EscapeDataString(bindingToRemove.Source ?? string.Empty)}/q/{bindingToRemove.Destination}/{bindingToRemove.PropertiesKey}";
                        try
                        {
                            HttpResponseMessage response = await apiClient.DeleteAsync(removeUrl, cancellationToken);
                            success = response.IsSuccessStatusCode;
                        }
                        catch (Exception exDelete)
                        {
                            Logger?.LogError("Error DELETEing QueueBindings via Management API: {removeUrl}{Environment.NewLine}{exDelete}", removeUrl, Environment.NewLine, exDelete);
                            await Task.Delay(500, cancellationToken);
                        }
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Attempts to delcare the configured exchange.
        /// </summary>
        public void DeclareExchange()
        {
            if (RabbitConfiguration.ExchangeDeclarationSettings!.Passive)
            {
                try
                {
                    _model?.ExchangeDeclarePassive(RabbitConfiguration.ExchangeName);
                }
                catch
                {
                    Logger?.LogWarning("Passive declaration of exchange {name} has failed.", RabbitConfiguration.ExchangeName);
                }
            }
            else
            {
                try
                {
                    _model?.ExchangeDeclare(RabbitConfiguration.ExchangeName,
                                            RabbitConfiguration.ExchangeDeclarationSettings.ExchangeType.ToString(),
                                            RabbitConfiguration.ExchangeDeclarationSettings.Durable,
                                            RabbitConfiguration.ExchangeDeclarationSettings.AutoDelete,
                                            RabbitConfiguration.ExchangeDeclarationSettings.Arguments);
                }
                catch
                {
                    Logger?.LogWarning("Declaration of exchange {name} has failed.", RabbitConfiguration.ExchangeName);
                }
            }
        }

        /// <summary>
        /// Attempts to declare the configured queue.
        /// </summary>
        public void DeclareQueue()
        {
            if (RabbitConfiguration.QueueDeclarationSettings!.Passive)
            {
                try
                {
                    _model?.QueueDeclarePassive(RabbitConfiguration.QueueName);
                }
                catch
                {
                    Logger?.LogWarning("Passive declaration of queue {name} has failed.", RabbitConfiguration.QueueName);
                }
            }
            else
            {
                try
                {
                    _model?.QueueDeclare(RabbitConfiguration.QueueName,
                                         RabbitConfiguration.QueueDeclarationSettings.Durable,
                                         RabbitConfiguration.QueueDeclarationSettings.Exclusive,
                                         RabbitConfiguration.QueueDeclarationSettings.AutoDelete,
                                         RabbitConfiguration.QueueDeclarationSettings.Arguments);
                }
                catch
                {
                    Logger?.LogWarning("Declaration of queue {name} has failed.", RabbitConfiguration.QueueName);
                }
            }
        }

        /// <summary>
        /// Attempts to bind the queue to the specified routing keys.
        /// </summary>
        public void ConfigureBindings() => RabbitConfiguration.RoutingKeys.ToList().ForEach(key =>
        {
            try
            {
                _model?.QueueBind(RabbitConfiguration.QueueName, RabbitConfiguration.ExchangeName, key, null);
            }
            catch
            {
                Logger?.LogWarning("Binding of routing-key {key} to queue {name} has failed.", key, RabbitConfiguration.QueueName);
            }
        });

        /// <summary>
        /// Hosted service StartAsync().
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Hosted service StopAsync().
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task StopAsync(CancellationToken cancellationToken);

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

            _model?.Dispose();
            _connection?.Dispose();
        }
    }
}
