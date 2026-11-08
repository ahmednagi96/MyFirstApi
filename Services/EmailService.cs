using System.Net;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MyFirstApi.Configuration;
using Hangfire;
namespace MyFirstApi.Services;

public class EmailService(IOptions<GmailSettings> gmailSettings) : IEmailService
{

    private readonly GmailSettings _gmailSettings = gmailSettings.Value;
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_gmailSettings.SenderName, _gmailSettings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        var bodyBuilder = new BodyBuilder() 
        {
            HtmlBody = body
        };
        message.Body = bodyBuilder.ToMessageBody();
        // 3. الاتصال عبر MailKit
        using var client = new SmtpClient();
        // الاتصال السريع المشفّر بسيرفر Gmail
        await client.ConnectAsync(_gmailSettings.SmtpServer, _gmailSettings.Port, SecureSocketOptions.StartTls, ct);
        await client.AuthenticateAsync(_gmailSettings.SenderEmail, _gmailSettings.AppPassword, ct);        
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

    }
}

