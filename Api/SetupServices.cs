using Api.Authorization;
using Common.AppsettingsModels;
using Services.FusekiService;
using Services.RdfService;
using Doc2Rdf.Library.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace Api
{
    public static class SetupServices
    {
        public static IServiceCollection AddSplinterServices(this IServiceCollection services)
        {
            services.AddScoped<IRdfService, RdfService>();
            services.AddScoped<IFusekiService, FusekiService>();
            services.AddDoc2RdfLibraryServices();

            return services;
        }

        public static MicrosoftIdentityAppCallsWebApiAuthenticationBuilder AddFusekiApis(this MicrosoftIdentityAppCallsWebApiAuthenticationBuilder builder, IConfiguration configuration)
        {
            foreach (var server in configuration.GetSection(ApiKeys.Servers).Get<List<RdfServer>>())
            {
                builder.AddDownstreamWebApi(server.Name.ToLower(), options =>
                {
                    options.BaseUrl = server.BaseUrl;
                    options.Scopes = server.Scopes;
                });
            }
            return builder;
        }
    }
}