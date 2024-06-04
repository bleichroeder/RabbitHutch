using Microsoft.Extensions.Hosting;

namespace RabbitHutch.DependencyInjection
{
    /// <summary>
    /// Wrapper for registering a <see cref="IRabbitPublisher{T}"/> as a hosted service.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HostedRabbitPublisherWrapper<T>(T instance) : IHostedService where T : IHostedService
    {
        private readonly T _instance = instance;

        /// <summary>
        /// Start the hosted service.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken) => _instance.StartAsync(cancellationToken);

        /// <summary>
        /// Stop the hosted service.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken) => _instance.StopAsync(cancellationToken);
    }
}
