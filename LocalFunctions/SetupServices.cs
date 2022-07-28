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
