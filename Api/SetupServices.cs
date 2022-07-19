﻿using Common.AppsettingsModels;
using Services.FusekiService;
using Services.ProvenanceService;
using Services.RdfService;
using Services.TieMessageService;
using Doc2Rdf.Library.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Common.Utils;

namespace Api
{
    public static class SetupServices
    {
        public static IServiceCollection AddSplinterServices(this IServiceCollection services)
        {
            services.AddScoped<IRdfService, RdfService>();
            services.AddScoped<IFusekiService, FusekiService>();
            services.AddScoped<ITieMessageService, TieMessageService>();
            services.AddScoped<IProvenanceService, ProvenanceService>();
            services.AddDoc2RdfLibraryServices();

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
}