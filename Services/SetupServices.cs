using Microsoft.Extensions.DependencyInjection;

namespace Services
{
    public static class SetupServices
    {
        public static IServiceCollection AddSplinterServices(this IServiceCollection services)
        {
            return services;
        }
    }
}