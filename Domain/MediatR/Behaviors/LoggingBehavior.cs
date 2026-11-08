using MediatR;
namespace MyFirstApi.Domain.MediatR.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            logger.LogInformation("🚀 [START] Handling Request: {RequestName} | Payload: {@Request}",
                requestName, request);
            var response = await next(); // تنفيذ الـ Handler
            logger.LogInformation("✅ [END] Handled Request: {RequestName} | Response: {@Response}", requestName, response);
            return response;
        }
}