using System.Net;
using System.Text.Json;

namespace ECommerce.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, IStringLocalizer<SharedResource> localizer, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request.Path);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response has already started, cannot execute custom error handling for {Path}", context.Request.Path);
                throw;
            }

            var isApi = context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) ||
                        context.Request.Path.StartsWithSegments("/hubs", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(context.Request.Headers["Accept"], "application/json", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(context.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (isApi)
            {
                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var payload = JsonSerializer.Serialize(new
                {
                    message = _localizer["Common.UnexpectedError"].Value
                });

                await context.Response.WriteAsync(payload);
            }
            else
            {
                context.Response.Redirect("/Error");
            }
        }
    }
}
