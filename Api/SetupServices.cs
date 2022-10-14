using Services.DependencyInjection;
using Microsoft.Identity.Web;
using Common.Utils;
using Common.AppsettingsModels;

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
            var fusekis = configuration.GetSection(ApiKeys.Servers).Get<List<RdfServer>>();
            if (fusekis == null)
            {
                throw new Exception("No downstream fusekis configured, see README.md");
            }
            foreach (var fuseki in fusekis)
            {
                builder.AddDownstreamWebApi(fuseki.Name, options =>
                {
                    options.BaseUrl = fuseki.BaseUrl;
                    options.Scopes = fuseki.Scopes;
                });
            }
            return builder;
        }
    }
}