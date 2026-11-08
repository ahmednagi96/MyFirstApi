using MediatR;
namespace MyFirstApi.Domain.MediatR;

public class SafeNotificationPublisher(ILogger<SafeNotificationPublisher> logger)
    : INotificationPublisher
{
    public async Task Publish(
        IEnumerable<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken)
    {
        foreach (var handler in handlerExecutors)
        {
            try
            {
                await handler.HandlerCallback(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Handler {Handler} failed for {Event}",
                    handler.HandlerInstance.GetType().Name,
                    notification.GetType().Name);
            }
        }
    }
}