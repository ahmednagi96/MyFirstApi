using MediatR;
namespace MyFirstApi.Events;

public record PasswordResetRequestedEvent(int UserId, string Name,
string Email, string Subject, string Body) : INotification;
