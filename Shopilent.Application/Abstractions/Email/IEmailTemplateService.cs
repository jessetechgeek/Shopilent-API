namespace Shopilent.Application.Abstractions.Email;

public interface IEmailTemplateService
{
    string BuildEmailVerificationTemplate(string email, string verificationToken, string appUrl);
    string BuildPasswordResetTemplate(string email, string resetToken, string appUrl);
    string BuildOrderConfirmationTemplate(Guid orderId, string customerEmail, string appUrl);
    string BuildShippingConfirmationTemplate(Guid orderId, string customerEmail, string trackingNumber, string appUrl);
    string BuildPaymentConfirmationTemplate(Guid orderId, string customerEmail, decimal amount, string appUrl);
    string BuildRefundConfirmationTemplate(Guid orderId, string customerEmail, decimal refundAmount, string appUrl);
    string BuildWelcomeEmailTemplate(string customerName, string email, string appUrl);
    string BuildOrderStatusUpdateTemplate(Guid orderId, string customerEmail, string oldStatus, string newStatus, string appUrl);
}