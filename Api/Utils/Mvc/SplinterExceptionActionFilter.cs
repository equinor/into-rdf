using Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace Api.Utils.Mvc;

public class SplinterExceptionActionFilter : ExceptionFilterAttribute
{
    private readonly ILogger<SplinterExceptionActionFilter> _logger;

    public SplinterExceptionActionFilter(ILogger<SplinterExceptionActionFilter> logger)
    {
        _logger = logger;
    }

    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is MissingScopeException msException)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Missing access",
                Status = (int)MissingScopeException.StatusCode(),
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Detail = msException.Message,
                Instance = msException.FusekiName
            };
            _logger.LogWarning(context.Exception, "Exception for route {Route}", context.HttpContext.Request.Host);
            context.Result = new JsonResult(problemDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            context.HttpContext.Response.StatusCode = (int)problemDetails.Status;
            context.HttpContext.Response.ContentType = "application/json";
        }

        base.OnException(context);
    }
}