using Common.AppsettingsModels;
using Common.Utils;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Services.DependencyInjection;
using Services.SpineNotificationServices;
using System.Collections.Generic;

namespace LocalFunctions;

public static class SetupServices
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services.AddSplinterServices();
    }

    public static MicrosoftIdentityAppCallsWebApiAuthenticationBuilder AddFusekiApis(this MicrosoftIdentityAppCallsWebApiAuthenticationBuilder builder, IConfiguration configuration)
    {
        var fusekis = configuration.GetSection(ApiKeys.Servers).Get<List<RdfServer>>();
        foreach (var fuseki in fusekis)
        {
            builder.AddDownstreamWebApi(fuseki.Name.ToLower(), options =>
            {
                options.BaseUrl = fuseki.BaseUrl;
                options.Scopes = fuseki.Scopes;
            });
        }
        return builder;
    }
}
