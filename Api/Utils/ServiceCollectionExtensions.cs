using Api.Authorization;
using Api.Authorization.Handlers.Fallback;
using Api.Controllers;
using Common.AppsettingsModels;
using Common.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Services.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api;
public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddFallbackAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            //Setup safeguard if not Authorize or AllowAnonymous is present on endpoint
            var fallbackPolicyBuilder = new AuthorizationPolicyBuilder();
            fallbackPolicyBuilder.Requirements.Add(new FallbackSafeguardRequirement());
            var fallbackPolicy = fallbackPolicyBuilder.Build();
            options.FallbackPolicy = fallbackPolicy;

        });

        services.AddSingleton<IAuthorizationHandler, FallbackSafeguardHandler>();
        return services;
    }

    internal class RdfAcceptFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Responses.TryGetValue("200", out var okres)
                && okres.Content.Keys.Intersect(new[] { "text/turtle", "application/trig", "application/sparql-results+json" }).Any())
            {
                var prev = okres.Content.ToArray();
                okres.Content.Clear();
                okres.Content.Add("*/*", new OpenApiMediaType());
                foreach(var kv in prev) okres.Content.Add(kv);
            }

        }
    }
    internal class SparqlContentTypeFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if(!(operation.RequestBody is OpenApiRequestBody rb && rb.Content.ContainsKey("application/sparql-query"))) return;
            foreach(var (key, type) in rb.Content)
                switch (key){
                    case "application/sparql-query":
                    case "text/plain":
                        if(type.Examples.Count > 0) continue;
                        type.Examples.Add("CONSTRUCT", new OpenApiExample {Value = new Microsoft.OpenApi.Any.OpenApiString("CONSTRUCT  {?s ?p ?o} where {?s ?p ?o} LIMIT 100")});
                        type.Examples.Add("SELECT", new OpenApiExample {Value = new Microsoft.OpenApi.Any.OpenApiString("SELECT * where {?s ?p ?o} LIMIT 100")});
                    break;
                    case "application/json":
                        if(type.Examples.Count > 0) continue;
                        type.Examples.Add("CONSTRUCT", new OpenApiExample {Value = new Microsoft.OpenApi.Any.OpenApiObject{ {"query", new Microsoft.OpenApi.Any.OpenApiString("CONSTRUCT  {?s ?p ?o} where {?s ?p ?o} LIMIT 100")}}});
                        type.Examples.Add("SELECT", new OpenApiExample {Value = new Microsoft.OpenApi.Any.OpenApiObject{ {"query", new Microsoft.OpenApi.Any.OpenApiString("SELECT * where {?s ?p ?o} LIMIT 100")}}});
                    break;
                    case "multipart/form-data":
                    case "application/x-www-form-urlencoded":
                        var schema = context.SchemaRepository.Schemas[nameof(SparqlQuery)];
                        if(schema.Required.Contains("query")) continue;
                        schema.Required.Add("query");
                        schema.Properties["query"].Example = new Microsoft.OpenApi.Any.OpenApiString("CONSTRUCT  {?s ?p ?o} where {?s ?p ?o} LIMIT 100", false);
                        // type.Examples.Add("SELECT", new OpenApiExample {Value = new Microsoft.OpenApi.Any.OpenApiObject{ {"query", new Microsoft.OpenApi.Any.OpenApiString("SELECT * where {?s ?p ?o} LIMIT 100", true)}}});
                    break;
                }
        }

    }
    public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration config)
    {
        services.AddEndpointsApiExplorer();

        var azureAdConfig = config.GetSection("AzureAd").Get<AzureAdConfig>();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Spine Splinter", Version = "v1" });
            var xmlPath = Path.Combine(AppContext.BaseDirectory, "Api.xml");
            options.IncludeXmlComments(xmlPath);
            options.OperationFilter<RdfAcceptFilter>();
            options.OperationFilter<SparqlContentTypeFilter>();
            options.AddSecurityDefinition(SecuritySchemeType.OAuth2.ToString(), new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Description = "Spine Splinter OpenId Security Scheme",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = azureAdConfig.AuthorizationUrl,
                        TokenUrl = azureAdConfig.TokenUrl,
                        Scopes = new Dictionary<string, string>
                        {
                        {$"api://{azureAdConfig.ClientId}/user_impersonation", "Sign in on your behalf"}
                        }
                    }
                }
            });
            {
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        { Type = ReferenceType.SecurityScheme, Id = SecuritySchemeType.OAuth2.ToString() }
                    }] = Array.Empty<string>()
                });
            }
        });
        return services;
    }

    public static IServiceCollection AddMSFTAuthentication<C>(this IServiceCollection services, C config)
        where C : IConfiguration, IConfigurationBuilder
    {
        config.AddKeyVault();

        var apiCallBuilder = services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(config.GetSection("AzureAd"))
            .EnableTokenAcquisitionToCallDownstreamApi();

        // Register downstream API services from configuration
        foreach (var fuseki in config
            .GetSection(ApiKeys.Servers)
            .Get<List<RdfServer>>()
            ?? throw new Exception("No downstream fusekis configured, see README.md"))
        {
            apiCallBuilder.AddDownstreamWebApi(fuseki.Name.ToLower(), options =>
            {
                options.BaseUrl = fuseki.BaseUrl;
                options.Scopes = fuseki.Scopes;
            });
        }
        apiCallBuilder
            .AddDownstreamWebApi(ApiKeys.CommonLib, config.GetSection(ApiKeys.CommonLib))
            .AddInMemoryTokenCaches();

        return services;
    }

    public static IServiceCollection AddAPIServices(this IServiceCollection services)
    {
        services.AddSplinterServices();
        services.AddSplinterRepositories();
        return services;
    }

    public static IMvcBuilder AddAPIEndpoints(this IServiceCollection services)
    {
        services.AddMvc(options =>
        {
            options.InputFormatters.Insert(0,new SparqlQueryInputFormatter());
        });
        return services.AddControllers();
        // return services;
    }
}