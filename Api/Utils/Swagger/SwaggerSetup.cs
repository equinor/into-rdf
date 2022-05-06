using Api.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Utils.Swagger
{
    public static class SwaggerSetup
    {
        public static void SetupCustomSwagger(this WebApplicationBuilder builder)
        {
            var azureAdConfig = builder.Configuration.GetSection("AzureAd").Get<AzureAdConfig>();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Spine Splinter", Version = "v1" });
                var xmlPath = Path.Combine(AppContext.BaseDirectory, "Api.xml");
                options.IncludeXmlComments(xmlPath);
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
                options.AddAuthRequirementToAllSwaggerEndpoints();
            });
        }

        public static void SetupCustomSwaggerUi(this IApplicationBuilder app, IConfiguration config)
        {
            var azureAdConfig = config.GetSection("AzureAd").Get<AzureAdConfig>();

            app.UseSwagger();
            app.UseSwaggerUI(setup =>
            {
                setup.SwaggerEndpoint("/swagger/v1/swagger.json", "Spine Splinter v1");
                setup.OAuthAppName("Spine Splinter");
                setup.OAuthUsePkce();
                setup.OAuthClientId(azureAdConfig.ClientId);
                //https://github.com/swagger-api/swagger-ui/issues/6290 Ongoing issue with hiding field for client_secret as it is not in use with PKCE
                setup.ConfigObject.AdditionalItems.Add("syntaxHighlight", false);
            });
        }

        private static void AddAuthRequirementToAllSwaggerEndpoints(this SwaggerGenOptions options)
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
    }
}
