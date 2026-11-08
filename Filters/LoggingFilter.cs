using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace MyFirstApi.Filters;

public class LoggingFilter(ILogger<LoggingFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var actionDetails = context.ActionDescriptor;
        var actionName = actionDetails.DisplayName;
        var controllerName = context.RouteData.Values["controller"];
        var method = context.HttpContext.Request.Method;
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress;
        var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();
        var queryString = context.HttpContext.Request.QueryString.ToString();
        var body = context.HttpContext.Request.Body.ToString();
        var arguments = context.ActionArguments;
        logger.LogInformation("Executing action: {ActionName} in controller: {ControllerName} with method: {Method} from IP: {IpAddress} and User-Agent: {UserAgent} with query string: {QueryString} and body: {Body} and arguments: {Arguments}", actionName, controllerName, method, ipAddress, userAgent, queryString, body, arguments
        );
        await next();
    }
}

