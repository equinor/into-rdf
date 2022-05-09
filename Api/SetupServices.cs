using Microsoft.Extensions.DependencyInjection;
using Services.Doc2RdfService;

namespace Api
{
    public static class SetupServices
    {
        public static IServiceCollection AddSplinterServices(this IServiceCollection services)
        {
            services.AddScoped<IDoc2RdfService, Doc2RdfService>();
            return services;
        }
    }
}