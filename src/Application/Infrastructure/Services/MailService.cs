using Application.Common.Interfaces;

using Ardalis.GuardClauses;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Configuration;

using MimeKit;

namespace Infrastructure.Services;

public class MailService(IConfiguration configuration) : IMailService
{
    private readonly MailConfiguration? _mailSettings =
        configuration.GetSection("MailConfiguration").Get<MailConfiguration>();

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Guard.Against.Null(_mailSettings, "Mail settings are not provided in the app settings file!");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_mailSettings.FromName ?? _mailSettings.From, _mailSettings.From));
        message.To.Add(new MailboxAddress(email, email));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlMessage
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        // Port 465 uses implicit SSL (SecureSocketOptions.SslOnConnect)
        // Port 587 uses STARTTLS (SecureSocketOptions.StartTls)
        var secureSocketOptions = _mailSettings.Port == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await client.ConnectAsync(_mailSettings.Host, _mailSettings.Port, secureSocketOptions);
        await client.AuthenticateAsync(_mailSettings.From, _mailSettings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private class MailConfiguration
    {
        public required string Host { get; set; }
        public required int Port { get; set; }
        public required string Password { get; set; }
        public required string From { get; set; }
        public string? FromName { get; set; }
    }
}
