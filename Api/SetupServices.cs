using Common.AppsettingsModels;
using Services.DependencyInjection;
using Microsoft.Identity.Web;
using Common.Utils;

namespace Api
{
    public static class SetupServices
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services.AddSplinterServices();
        }

        public static MicrosoftIdentityAppCallsWebApiAuthenticationBuilder AddFusekiApis(this MicrosoftIdentityAppCallsWebApiAuthenticationBuilder builder, IConfiguration configuration)
        {
            var servers = configuration.GetSection(ApiKeys.Servers).Get<Dictionary<string, RdfServer>>();
            foreach (var serverKey in servers.Keys)
            {
                var server = servers[serverKey];
                builder.AddDownstreamWebApi(serverKey.ToLower(), options =>
                {
                    options.BaseUrl = server.BaseUrl;
                    options.Scopes = server.Scopes;
                });
            }
            return builder;
        }
    }
}