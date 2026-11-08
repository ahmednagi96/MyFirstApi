using MediatR;
using System.Diagnostics;
namespace MyFirstApi.Domain.MediatR.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>
            : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch stopwatch;
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        stopwatch = new Stopwatch();
    }
   public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        stopwatch.Start();
        var response = await next();
        stopwatch.Stop();
       var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        // لو الطلب استغرق أكثر من 500 ملي ثانية (نصف ثانية)، سجل تحذير أداء
        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogWarning("⚠️ [SLOW REQUEST] Long Running Request: {RequestName} ({ElapsedMilliseconds} ms) | Payload: {@Request}",
                requestName, elapsedMilliseconds, request);
        }
        return response;
    }
}