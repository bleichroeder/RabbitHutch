using Microsoft.Extensions.Logging;
using RabbitHutch.Consumers.Interfaces;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Delegates;
using RabbitHutch.Core.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace RabbitHutch.Consumers
{
    public class RabbitConsumer<T> : RabbitConsumerBase<T>
    {
        protected bool _stop;
        protected bool _isRunning;
        protected bool _stopNow;

        private readonly Thread _consumerThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicRabbitConsumer{T}"/>
        /// </summary>
        /// <param name="rabbitConfiguration"></param>
        /// <param name="logger"></param>
        [SetsRequiredMembers]
        public RabbitConsumer(IRabbitConsumerSettings rabbitConfiguration, ILogger? logger)
            : this(rabbitConfiguration, MessageReceivedDelegates.DefaultNewMessageCallback<T>(), logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicRabbitConsumer{T}"/>
        /// </summary>
        /// <param name="rabbitConfiguration"></param>
        /// <param name="asyncNewMessageCallback"></param>
        /// <param name="logger"></param>
        [SetsRequiredMembers]
        public RabbitConsumer(IRabbitConsumerSettings rabbitConfiguration,
                              AsyncNewMessageCallbackDelegate<T> asyncNewMessageCallback,
                              ILogger? logger)
            : this(rabbitConfiguration, asyncNewMessageCallback, MessageDeserializers.DefaultMessageDeserializerFromBytes<T>(), logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicRabbitConsumer{T}"/>
        /// </summary>
        /// <param name="rabbitConfiguration"></param>
        /// <param name="asyncNewMessageCallback"></param>
        /// <param name="deserializer"></param>
        /// <param name="logger"></param>
        [SetsRequiredMembers]
        public RabbitConsumer(IRabbitConsumerSettings rabbitConfiguration,
                              AsyncNewMessageCallbackDelegate<T> asyncNewMessageCallback,
                              MessageDeserializerFromBytesDelegate<T> deserializer,
                              ILogger? logger)
            : this(rabbitConfiguration, ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile(), deserializer, asyncNewMessageCallback, logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicRabbitConsumer{T}"/>
        /// </summary>
        /// <param name="connectionString">The host.</param>
        /// <param name="exchangeName">Name of the exchange.</param>
        [SetsRequiredMembers]
        public RabbitConsumer(IRabbitConsumerSettings rabbitConfiguration,
                              IConnectionLifecycleProfile lifecycleProfile,
                              MessageDeserializerFromBytesDelegate<T> deserializer,
                              AsyncNewMessageCallbackDelegate<T> asyncNewMessageCallback,
                              ILogger? logger)
        {
            LifecycleProfile = lifecycleProfile;
            Deserializer = deserializer ?? MessageDeserializers.DefaultMessageDeserializerFromBytes<T>();
            RabbitConfiguration = rabbitConfiguration;
            MessageCallbackDelegate = asyncNewMessageCallback;
            Logger = logger;

            _consumerThread = new Thread(async () => await RunAsync())
            {
                Name = $"{typeof(T).Name}_{GetType().Name}",
            };
        }

        /// <summary>
        /// Starts the <see cref="QueueingRabbitPublisher{T}"/>
        /// </summary>
        public void Start()
        {
            Logger?.LogInformation("The {typeof(T).Name} {GetType().Name} is starting.", typeof(T).Name, GetType().Name);

            _stop = false;
            _isRunning = true;

            _consumerThread.Start();
        }

        /// <summary>
        /// Stops the <see cref="QueueingRabbitPublisher{T}"/>
        /// </summary>
        public async Task ShutdownAsync()
        {
            Logger?.LogInformation("The {typeof(T).Name} {GetType().Name} is shutting down.", typeof(T).Name, GetType().Name);

            _stop = true;

            DateTime timeout = DateTime.Now.AddSeconds(30);

            while (_isRunning)
            {
                if (DateTime.Now > timeout)
                {
                    Logger?.LogWarning("WARNING: Timed out shutting down {GetType().Name}. (Possible loss of {typeof(T).Name} messages)", GetType().Name, typeof(T).Name);

                    _stopNow = true;

                    await Task.Delay(1000, CancellationToken);

                    break;
                }

                await Task.Delay(100, CancellationToken);
            }
        }

        /// <summary>
        /// Hosted service start.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Hosted service stop.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StopAsync(CancellationToken cancellationToken) => Task.Run(Dispose, cancellationToken);

        /// <summary>
        /// Runs the consumer.
        /// </summary>
        /// <returns></returns>
        private async Task RunAsync()
        {
            _isRunning = true;
            _stop = false;
            _stopNow = false;

            // Keep trying to connect until the consumer is stopped.
            while (!_stop)
            {
                try
                {
                    if (await InitializeRabbitAsync() is false)
                    {
                        Logger?.LogWarning("WARNING: The connection to {_connection?.Endpoint.HostName} is not active.", _connection?.Endpoint.HostName);
                    }

                    try
                    {
                        _model?.BasicConsume(RabbitConfiguration.QueueName, RabbitConfiguration.AutoAck, _consumerThread.Name, false, false, null, _inputConsumer);

                        Logger?.LogTrace("{_consumerThread.Name} is now running.", _consumerThread.Name);

                        // Wait until a stop has been requested.
                        while (_stop is false)
                        {
                            // Check that the connection is still open.
                            // If not, we'll break out of the loop and try to re-establish the connection.
                            if (IsActive)
                            {
                                Logger?.LogWarning($"WARNING: The connection to RabbitMQ has been lost; attempting to re-establish...");
                                break;
                            }

                            // Or break if we've been told to stop.
                            if (_stopNow) break;

                            await Task.Delay(250);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError("ERROR: {ex}", ex);

                        await Task.Delay(1000);
                    }

                    try { _model?.Close(); } catch { Logger?.LogWarning("WARNING: Unable to close model."); };
                    try { _connection?.Close(); } catch { Logger?.LogWarning("WARNING: Unable to close Connection."); };
                }
                catch (Exception ex)
                {
                    Logger?.LogError("ERROR: {ex}", ex);
                }
            }

            _isRunning = false;
        }
    }
}
