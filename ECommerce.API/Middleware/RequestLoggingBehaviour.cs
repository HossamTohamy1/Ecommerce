using System.Diagnostics;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ECommerce.API.Middleware;

public class RequestLoggingBehaviour
{
    private readonly RequestDelegate _next;
    private static readonly ILogger Logger = Log.ForContext<RequestLoggingBehaviour>();

    public RequestLoggingBehaviour(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var response = context.Response;

            var level = response.StatusCode >= 500 ? Serilog.Events.LogEventLevel.Error
                : response.StatusCode >= 400 ? Serilog.Events.LogEventLevel.Warning
                : Serilog.Events.LogEventLevel.Information;

            Logger.Write(
                level,
                "HTTP {Method} {Path} responded {StatusCode} in {Elapsed:0.0000} ms",
                request.Method,
                request.Path,
                response.StatusCode,
                stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}

public static class RequestLoggingBehaviourExtensions
{
    public static IApplicationBuilder UseRequestLoggingBehaviour(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingBehaviour>();
}
