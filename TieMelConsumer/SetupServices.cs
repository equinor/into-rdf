using Common.AppsettingsModels;
using Common.Utils;
using Doc2Rdf.Library.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Services.FusekiService;
using Services.ProvenanceService;
using Services.RdfService;
using Services.SpineNotificationService;
using Services.TieMessageService;
using System.Collections.Generic;

namespace TieMelConsumer;

public static class SetupServices
{
    public static IServiceCollection AddSplinterServices(this IServiceCollection services)
    {
        services.AddScoped<IRdfService, RdfService>();
        services.AddScoped<IFusekiService, FusekiService>();
        services.AddScoped<ITieMessageService, TieMessageService>();
        services.AddScoped <IProvenanceService, ProvenanceService>(); 
        services.AddDoc2RdfLibraryServices();

        return services;
    }

    public static IServiceCollection AddServiceBusClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISpineNotificationService, SpineNotificationService>();
        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClient(configuration.GetConnectionString(ApiKeys.ServiceBus));
        });
        return services;
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
