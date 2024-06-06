using Microsoft.Extensions.Logging;
using RabbitHutch.Consumers.Interfaces;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Delegates;
using RabbitHutch.Core.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics.CodeAnalysis;

namespace RabbitHutch.Consumers
{
    public class SingleFetchRabbitConsumer<T> : RabbitConsumerBase<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicRabbitConsumer{T}"/>
        /// </summary>
        /// <param name="rabbitConfiguration"></param>
        /// <param name="logger"></param>
        [SetsRequiredMembers]
        public SingleFetchRabbitConsumer(IRabbitConsumerSettings rabbitConfiguration, ILogger? logger)
            : this(rabbitConfiguration, MessageReceivedDelegates.DefaultNewMessageCallback<T>(), logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicRabbitConsumer{T}"/>
        /// </summary>
        /// <param name="rabbitConfiguration"></param>
        /// <param name="asyncNewMessageCallback"></param>
        /// <param name="logger"></param>
        [SetsRequiredMembers]
        public SingleFetchRabbitConsumer(IRabbitConsumerSettings rabbitConfiguration,
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
        public SingleFetchRabbitConsumer(IRabbitConsumerSettings rabbitConfiguration,
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
        public SingleFetchRabbitConsumer(IRabbitConsumerSettings rabbitConfiguration,
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
        }

        /// <summary>
        /// Attempts to consume and process a single message.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> FetchMessageAsync() => await FetchMessageAsync(CancellationToken);

        /// <summary>
        /// Attempts to consume and process a single message.
        /// </summary>
        /// <param name="receievedFunction"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> FetchMessageAsync(CancellationToken cancellationToken)
        {
            if (await InitializeRabbitAsync(cancellationToken) is false)
            {
                Logger?.LogWarning("WARNING: The connection to {_connection?.Endpoint.HostName} is not active.", _connection?.Endpoint.HostName);
            }
            else
            {
                try
                {
                    BasicGetResult? basicGetResult = _model?.BasicGet(RabbitConfiguration.QueueName, RabbitConfiguration.AutoAck);

                    if (basicGetResult is not null)
                    {
                        BasicDeliverEventArgs eventArgs = new()
                        {
                            DeliveryTag = basicGetResult.DeliveryTag,
                            Redelivered = basicGetResult.Redelivered,
                            Exchange = basicGetResult.Exchange,
                            RoutingKey = basicGetResult.RoutingKey,
                            BasicProperties = basicGetResult.BasicProperties,
                            Body = basicGetResult.Body
                        };


                        TaskCompletionSource<bool> tcs = new();

                        void ReceivedDelegateHandler(object? sender, BasicDeliverEventArgs args) => DefaultReceivedDelegate(tcs, args);

                        ReceivedDelegateHandler(this, eventArgs);

                        return await tcs.Task;
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Logger?.LogError("ERROR: Failure while performing {get}: {ex}", nameof(FetchMessageAsync), ex);
                }
            }

            return false;
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
    }
}
