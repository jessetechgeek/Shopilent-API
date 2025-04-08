namespace Shopilent.Application.Abstractions.Email;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
}