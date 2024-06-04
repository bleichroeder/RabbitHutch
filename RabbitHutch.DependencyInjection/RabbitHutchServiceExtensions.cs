using Microsoft.Extensions.DependencyInjection;

namespace RabbitHutch.DependencyInjection
{
    /// <summary>
    /// RabbitHutch service extensions.
    /// </summary>
    public static class RabbitHutchServiceExtensions
    {
        /// <summary>
        /// Provides methods for configuring a RabbitHutch.
        /// A RabbitHutch is a collection of RabbitPublishers.
        /// The RabbitHutch is registered as a singleton in the DI container.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureContext"></param>
        /// <param name="buildRabbitHutch"></param>
        /// <returns></returns>
        public static IServiceCollection BuildRabbitHutch(this IServiceCollection services, Action<RabbitHutchBuilder> buildRabbitHutch)
        {
            RabbitHutchBuilder builder = new(services);
            buildRabbitHutch(builder);

            return services;
        }
    }
}
