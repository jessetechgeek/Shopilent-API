using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopilent.Application.Abstractions.Email;
using Shopilent.Application.Settings.Email;

namespace Shopilent.Infrastructure.Services.Email;

public class EmailService : IEmailService
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
            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(to);

            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                EnableSsl = _emailSettings.EnableSsl
            };

            // In development mode, we might want to just log emails instead of sending them
            if (_emailSettings.SendEmails)
            {
                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent to {Recipient} with subject: {Subject}", to, subject);
            }
            else
            {
                _logger.LogInformation("Email sending is disabled. Would have sent to {Recipient} with subject: {Subject}", to, subject);
                _logger.LogInformation("Email content: {Body}", body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Recipient} with subject: {Subject}", to, subject);
            throw;
        }
    }

    public async Task SendEmailVerificationAsync(string email, string token)
    {
        var verificationLink = $"{_emailSettings.AppUrl}/verify-email?token={WebUtility.UrlEncode(token)}";
        var subject = "Verify Your Email Address";
        var body = $@"
            <html>
            <body>
                <h1>Email Verification</h1>
                <p>Thank you for registering with Shopilent. Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationLink}'>Verify Email</a></p>
                <p>If you did not create this account, you can ignore this email.</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetAsync(string email, string token)
    {
        var resetLink = $"{_emailSettings.AppUrl}/reset-password?token={WebUtility.UrlEncode(token)}";
        var subject = "Reset Your Password";
        var body = $@"
            <html>
            <body>
                <h1>Password Reset</h1>
                <p>You have requested to reset your password. Please click the link below to set a new password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you did not request this password reset, you can ignore this email.</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendOrderConfirmationAsync(Guid orderId, string email)
    {
        var orderLink = $"{_emailSettings.AppUrl}/orders/{orderId}";
        var subject = "Order Confirmation";
        var body = $@"
            <html>
            <body>
                <h1>Order Confirmation</h1>
                <p>Thank you for your order! Your order has been received and is being processed.</p>
                <p>Order ID: {orderId}</p>
                <p><a href='{orderLink}'>View Order Details</a></p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendShippingConfirmationAsync(Guid orderId, string email, string trackingNumber = null)
    {
        var orderLink = $"{_emailSettings.AppUrl}/orders/{orderId}";
        var subject = "Your Order Has Been Shipped";
        var trackingInfo = !string.IsNullOrEmpty(trackingNumber)
            ? $"<p>Tracking Number: {trackingNumber}</p>"
            : "";

        var body = $@"
            <html>
            <body>
                <h1>Shipping Confirmation</h1>
                <p>Good news! Your order has been shipped.</p>
                <p>Order ID: {orderId}</p>
                {trackingInfo}
                <p><a href='{orderLink}'>View Order Details</a></p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPaymentConfirmationAsync(Guid orderId, string email, decimal amount)
    {
        var orderLink = $"{_emailSettings.AppUrl}/orders/{orderId}";
        var subject = "Payment Confirmation";
        var body = $@"
            <html>
            <body>
                <h1>Payment Confirmation</h1>
                <p>Your payment of {amount:C} has been processed successfully.</p>
                <p>Order ID: {orderId}</p>
                <p><a href='{orderLink}'>View Order Details</a></p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendRefundConfirmationAsync(Guid orderId, string email, decimal amount)
    {
        var orderLink = $"{_emailSettings.AppUrl}/orders/{orderId}";
        var subject = "Refund Confirmation";
        var body = $@"
            <html>
            <body>
                <h1>Refund Confirmation</h1>
                <p>Your refund of {amount:C} has been processed.</p>
                <p>Order ID: {orderId}</p>
                <p><a href='{orderLink}'>View Order Details</a></p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }
}