namespace RabbitHutch.Core.ConnectionLifecycle
{
    /// <summary>
    /// Interface for connection lifecycle profile.
    /// </summary>
    public interface IConnectionLifecycleProfile
    {
        /// <summary>
        /// Gets or sets the maximum number of retries.
        /// </summary>
        int MaxRetries { get; set; }

        /// <summary>
        /// Gets or sets the reconnect delay.
        /// </summary>
        TimeSpan ReconnectDelay { get; set; }
    }
}
