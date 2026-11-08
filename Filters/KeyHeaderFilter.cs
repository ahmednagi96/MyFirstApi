using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace MyFirstApi.Filters;

public class KeyHeaderFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var key = context.HttpContext.Request.Headers["X-Request-Id"].ToString();
        if (string.IsNullOrWhiteSpace(key))
        {
            //context.Result = new BadRequestResult();
            //return;
            context.Result = new BadRequestObjectResult(new ProblemDetails{
                Title = "Bad Request",
                Status = StatusCodes.Status400BadRequest,
                Detail = "X-Request-Id header is required"
            });
            return;
        }
    base.OnActionExecuting(context);
    }
}
