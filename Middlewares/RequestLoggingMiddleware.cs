namespace MyFirstApi.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next)
{

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path} nagi-request");
        await next(context);
        Console.WriteLine($"Response: {context.Response.StatusCode} nagi-response");
    }
}
