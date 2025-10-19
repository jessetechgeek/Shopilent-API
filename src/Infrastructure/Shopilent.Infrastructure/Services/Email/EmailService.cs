using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Shopilent.Application.Abstractions.Email;
using Shopilent.Infrastructure.Settings;

namespace Shopilent.Infrastructure.Services.Email;

internal class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject ?? string.Empty;

            var bodyBuilder = new BodyBuilder();
            if (isHtml)
            {
                bodyBuilder.HtmlBody = body ?? string.Empty;
            }
            else
            {
                bodyBuilder.TextBody = body ?? string.Empty;
            }

            message.Body = bodyBuilder.ToMessageBody();

            // In development mode, we might want to just log emails instead of sending them
            if (_emailSettings.SendEmails)
            {
                using var client = new SmtpClient();

                // Set timeouts
                client.Timeout = _emailSettings.ConnectionTimeoutSeconds * 1000;

                // Determine security options
                var secureSocketOptions = SecureSocketOptions.Auto;
                if (_emailSettings.UseSslOnConnect)
                {
                    secureSocketOptions = SecureSocketOptions.SslOnConnect;
                }
                else if (_emailSettings.EnableSsl)
                {
                    secureSocketOptions = SecureSocketOptions.StartTls;
                }
                else
                {
                    secureSocketOptions = SecureSocketOptions.None;
                }

                // Connect to the SMTP server
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, secureSocketOptions);

                // Authenticate if credentials are provided (basic auth for now, OAuth2 support can be added later)
                if (!string.IsNullOrEmpty(_emailSettings.SmtpUsername) &&
                    !string.IsNullOrEmpty(_emailSettings.SmtpPassword))
                {
                    await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent to {Recipient} with subject: {Subject}", to, subject);
            }
            else
            {
                _logger.LogInformation(
                    "Email sending is disabled. Would have sent to {Recipient} with subject: {Subject}", to, subject);
                _logger.LogInformation("Email content: {Body}", body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Recipient} with subject: {Subject}", to, subject);
            throw;
        }
    }
}
