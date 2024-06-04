namespace RabbitHutch.Core.ConnectionLifecycle
{
    /// <summary>
    /// Connection lifecycle profile.
    /// </summary>
    public abstract class ConnectionLifecycleProfileBase : IConnectionLifecycleProfile
    {
        /// <summary>
        /// Gets or sets the maximum number of retries.
        /// Defaults to -1 (infinite).
        /// </summary>
        public virtual int MaxRetries { get; set; } = -1;

        /// <summary>
        /// Gets or sets the reconnect delay.
        /// </summary>
        public virtual TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(5);
    }
}
