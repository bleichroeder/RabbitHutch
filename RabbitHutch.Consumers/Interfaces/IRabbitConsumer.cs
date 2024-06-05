using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitHutch.Core.ConnectionLifecycle;
using RabbitHutch.Core.Delegates;
using RabbitHutch.Core.Serialization;

namespace RabbitHutch.Consumers.Interfaces
{
    public interface IRabbitConsumer<T> : IDisposable, IHostedService
    {
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        ILogger? Logger { get; set; }

        /// <summary>
        /// Gets or sets the lifecycle profile.
        /// </summary>
        IConnectionLifecycleProfile LifecycleProfile { get; set; }

        /// <summary>
        /// Gets or sets the serializer.
        /// </summary>
        MessageDeserializerFromBytesDelegate<T> Deserializer { get; set; }

        /// <summary>
        /// Gets or sets the message received delegate.
        /// </summary>
        AsyncNewMessageCallbackDelegate<T> MessageCallbackDelegate { get; set; }

        /// <summary>
        /// True if the underlying connection is active.
        /// </summary>
        bool IsActive { get; }
    }
}
