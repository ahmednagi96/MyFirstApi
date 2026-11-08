using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MyFirstApi.Middlewares;

public class PerformanceMiddleware(RequestDelegate next,ILogger<PerformanceMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        await next(context);
        stopwatch.Stop();
        var responseTime = stopwatch.ElapsedMilliseconds;
        if(responseTime > 10)
        {
            logger.LogWarning("Slow request {Method} {Path} took {ElapsedMs} ms",
                context.Request.Method,
                context.Request.Path,
                responseTime);
        }

    }
}