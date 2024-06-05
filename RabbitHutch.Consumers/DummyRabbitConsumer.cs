using Microsoft.Extensions.Logging;
using RabbitHutch.Consumers.Interfaces;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Delegates;
using RabbitHutch.Core.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace RabbitHutch.Consumers
{
    public class DummyRabbitConsumer<T> : RabbitConsumerBase<T> where T : new()
    {
        protected bool _stop;
        protected bool _isRunning;
        protected bool _stopNow;

        private readonly Thread _consumerThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicRabbitConsumer{T}"/>
        /// </summary>
        /// <param name="connectionString">The host.</param>
        /// <param name="exchangeName">Name of the exchange.</param>
        [SetsRequiredMembers]
        public DummyRabbitConsumer(IRabbitConsumerSettings rabbitConfiguration,
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

                    await Task.Delay(1000);

                    break;
                }

                await Task.Delay(100);
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
                // Simulate new message consumption.
                await Task.Delay(1000);

                T message = new();

                if (message is not null)
                {
                    await MessageCallbackDelegate(message);
                }

                Logger?.LogInformation("Simulated consumption of {Type} [{RoutingKey}] from [{QueueName}]", typeof(T).Name, RabbitConfiguration.RoutingKeys[0], RabbitConfiguration.QueueName);

                if (_stopNow)
                {
                    break;
                }
            }

            _isRunning = false;
        }
    }
}
