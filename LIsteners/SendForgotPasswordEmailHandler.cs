using MediatR;
using Humanizer;
using MyFirstApi.Entities;
using MyFirstApi.Events;
using MyFirstApi.Services;
using Hangfire;
namespace MyFirstApi.LIsteners;

public class SendForgotPasswordEmailHandler(IBackgroundJobClient backgroundJobClient) : INotificationHandler<PasswordResetRequestedEvent>
{
    public Task Handle(PasswordResetRequestedEvent notificatiion,CancellationToken cancellationToken){

     backgroundJobClient.Enqueue<IEmailService>(job => job.SendEmailAsync(notificatiion.Email,notificatiion.Subject,notificatiion.Body,cancellationToken));
        return Task.CompletedTask;
    }
}
