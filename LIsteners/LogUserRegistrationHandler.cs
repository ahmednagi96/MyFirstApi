using MediatR;
using MyFirstApi.Entities;
using MyFirstApi.Events;
namespace MyFirstApi.LIsteners;

public class LogUserRegistrationHandler(
    //ILogger<LogUserRegistrationHandler> logger
    ) : INotificationHandler<PasswordResetRequestedEvent>
{
    public Task Handle(PasswordResetRequestedEvent notification,CancellationToken cancellationToken){
        throw new NotImplementedException();
        //
        //logger.LogInformation("📝 [Audit Log]: User {UserId} ({Name}) registered successfully at {Time}",
        //    notification.UserId, notification.Name, DateTime.UtcNow);
          //  return Task.CompletedTask;
    }
}
