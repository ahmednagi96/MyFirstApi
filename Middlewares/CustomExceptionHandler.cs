using Microsoft.AspNetCore.Diagnostics;
using MyFirstApi.Exceptions;
using MyFirstApi.Extensions;

namespace MyFirstApi.Middlewares;

public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger, IHostEnvironment env)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = MapException(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "An unhandled exception occurred during request {Path}. TraceId: {TraceId}",
                httpContext.Request.Path,
                httpContext.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Handled {ExceptionType} during request {Path}. TraceId: {TraceId}",
                exception.GetType().Name,
                httpContext.Request.Path,
                httpContext.TraceIdentifier);
        }

        await httpContext.WriteProblemAsync(statusCode, title, detail);
        return true;
    }

    private (int StatusCode, string Title, string Detail) MapException(Exception exception)
        => exception switch
        {
            KeyNotFoundException ex =>
                (StatusCodes.Status404NotFound, "Not Found", ex.Message),
            ConflictException ex =>
                (StatusCodes.Status409Conflict, "Conflict", ex.Message),
            UnauthorizedAccessException ex =>
                (StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message),
            ForbiddenException ex =>
                (StatusCodes.Status403Forbidden, "Forbidden", ex.Message),
            ArgumentException or BadHttpRequestException =>
                (StatusCodes.Status400BadRequest, "Bad Request", exception.Message),
            _ =>
            (
                StatusCodes.Status500InternalServerError,
                "Server Error",
                env.IsDevelopment()
                    ? exception.Message
                    : "حدث خطأ غير متوقع على الخادم. يرجى المحاولة لاحقاً."
            )
        };
}
