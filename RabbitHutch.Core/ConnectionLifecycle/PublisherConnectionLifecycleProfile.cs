namespace RabbitHutch.Core.ConnectionLifecycle
{
    /// <summary>
    /// Publisher connection lifecycle profile.
    /// </summary>
    public class PublisherConnectionLifecycleProfile : ConnectionLifecycleProfileBase, IConnectionLifecycleProfile
    {
        /// <summary>
        /// Gets or sets the maximum number of retries.
        /// </summary>
        public override int MaxRetries { get => base.MaxRetries; set => base.MaxRetries = value; }

        /// <summary>
        /// Gets or sets the reconnect delay.
        /// </summary>
        public override TimeSpan ReconnectDelay { get => base.ReconnectDelay; set => base.ReconnectDelay = value; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublisherConnectionLifecycleProfile"/> class.
        /// </summary>
        public PublisherConnectionLifecycleProfile() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublisherConnectionLifecycleProfile"/> class.
        /// </summary>
        /// <param name="maxRetries"></param>
        /// <param name="reconnectDelay"></param>
        public PublisherConnectionLifecycleProfile(int maxRetries, TimeSpan reconnectDelay)
        {
            MaxRetries = maxRetries;
            ReconnectDelay = reconnectDelay;
        }
    }
}
