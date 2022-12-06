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