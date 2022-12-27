using Api.Authorization;
using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;
using Common.Exceptions;

namespace Api;
public static class AppBuilderExtensions
{

    public static B UseSwaggerWithUI<B>(this B app, IConfiguration config)
        where B : IApplicationBuilder
    {
        var azureAdConfig = config.GetSection("AzureAd").Get<AzureAdConfig>();
        // var azureAdConfig = builder.Configuration.GetSection("AzureAd").Get<AzureAdConfig>();

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
        return app;
    }

    public static B UseCorsFromAppsettings<B>(this B app, IConfiguration conf, IWebHostEnvironment env)
        where B : IApplicationBuilder
    {
        if (env.IsDevelopment())
        // if (builder.Environment.IsDevelopment())
        {
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        }
        else
        {


            var allowedOrigins = conf.GetSection("Cors:AllowedOrigins")
            // var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")
                                     .Get<string[]>()
                                 ?? Array.Empty<string>();

            if (!allowedOrigins.Any()) Console.WriteLine("Warning: No CORS configured!");
            app.UseCors(builder => builder
                .WithOrigins(allowedOrigins)
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyHeader()
                .AllowAnyMethod());
        }
        return app;
    }

    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(exceptionHandlerApp
            => exceptionHandlerApp.Run(async context =>
        {
            context.Response.ContentType = MediaTypeNames.Text.Plain;

            var exceptionHandlerPathFeature =
                context.Features.Get<IExceptionHandlerPathFeature>();

            switch (exceptionHandlerPathFeature?.Error)
            {
                case FusekiException:
                case ShapeValidationException:
                case RevisionValidationException:
                case RevisionTrainValidationException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;
                case FileNotFoundException:
                case ObjectNotFoundException:
                    context.Response.StatusCode =StatusCodes.Status404NotFound;
                    break;
                case UnsupportedContentTypeException:
                    context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                    break;
                case ConflictOnInsertException:
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    break;
                case BadGatewayException:
                    context.Response.StatusCode = StatusCodes.Status502BadGateway;
                    break;
                case InvalidOperationException:
                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            await context.Response.WriteAsync(exceptionHandlerPathFeature?.Error?.Message ?? "");
        }));

        return app;
    }
}
