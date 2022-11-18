using System.Net.Mime;
using Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

public static class SetupExceptionHandling
{
    public static WebApplication AddExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler(exceptionHandlerApp
            => exceptionHandlerApp.Run(async context =>
        {
            context.Response.ContentType = MediaTypeNames.Text.Plain;

            var exceptionHandlerPathFeature =
                context.Features.Get<IExceptionHandlerPathFeature>();

            switch (exceptionHandlerPathFeature?.Error)
            {
                case InvalidOperationException:
                case FusekiException:
                case ShapeValidationException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;
                case ConflictOnInsertException:
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    break;
                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            await context.Response.WriteAsync(exceptionHandlerPathFeature?.Error?.Message ?? "");
        }));

        return app;
    }
}