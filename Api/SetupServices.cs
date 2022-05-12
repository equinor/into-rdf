using Api.Authorization;
using Common.AppsettingsModels;
using Services.FusekiService;
using Services.RdfService;

namespace Api
{
    public static class SetupServices
    {
        public static IServiceCollection AddSplinterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRdfService, RdfService>();
            services.AddScoped<IFusekiService, FusekiService>();
            foreach(var server in configuration.GetSection(ApiKeys.Servers).Get<List<RdfServer>>())
            {
                services.AddHttpClient(server.Name, client =>
                {
                    client.BaseAddress = new Uri(server.BaseUrl);
                });
            }
            return services;
        }
    }
}