namespace RabbitHutch.Core.ConnectionLifecycle
{
    /// <summary>
    /// Lifecycle profiles.
    /// </summary>
    public static class ConnectionLifecycleProfiles
    {
        /// <summary>
        /// Default publisher lifecycle profile.
        /// This profile will attempt to retry publication indefinitely with a 5 second delay between retries.
        /// </summary>
        /// <returns></returns>
        public static IConnectionLifecycleProfile DefaultPublisherConnectionLifecycleProfile() => new PublisherConnectionLifecycleProfile();

        /// <summary>
        /// Default publisher lifecycle profile.
        /// Provides options to specify a maximum number of retries and a reconnect delay.
        /// </summary>
        /// <param name="maxRetries"></param>
        /// <param name="reconnectDelay"></param>
        /// <returns></returns>
        public static IConnectionLifecycleProfile DefaultPublisherConnectionLifecycleProfile(int maxRetries, TimeSpan reconnectDelay)
            => new PublisherConnectionLifecycleProfile(maxRetries, reconnectDelay);
    }
}
