using Microsoft.AspNetCore.Mvc;

namespace MyFirstApi.Extensions;

public static class HttpContextProblemDetailsExtensions
{
    public static async Task WriteProblemAsync(
        this HttpContext httpContext,
        int statusCode,
        string title,
        string detail)
    {
        if (httpContext.Response.HasStarted)
            return;

        httpContext.Response.StatusCode = statusCode;

  // problemDetailsService دي بتحول شكل الprojectDetails الي json
        var problemDetailsService = httpContext.RequestServices
            .GetRequiredService<IProblemDetailsService>();

        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            }
        });
    }
}
