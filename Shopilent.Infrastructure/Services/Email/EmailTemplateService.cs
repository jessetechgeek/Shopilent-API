using System.Net;
using Shopilent.Application.Abstractions.Email;

namespace Shopilent.Infrastructure.Services.Email;

public class EmailTemplateService : IEmailTemplateService
{
    public string BuildEmailVerificationTemplate(string email, string verificationToken, string appUrl)
    {
        var verificationLink = $"{appUrl}/verify-email?token={WebUtility.UrlEncode(verificationToken)}";
        
        return $@"
            <html>
            <body>
                <h1>Email Verification</h1>
                <p>Thank you for registering with Shopilent. Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationLink}'>Verify Email</a></p>
                <p>If you did not create this account, you can ignore this email.</p>
                <br>
                <p>Best regards,<br>The Shopilent Team</p>
            </body>
            </html>";
    }

    public string BuildPasswordResetTemplate(string email, string resetToken, string appUrl)
    {
        var resetLink = $"{appUrl}/reset-password?token={WebUtility.UrlEncode(resetToken)}";
        
        return $@"
            <html>
            <body>
                <h1>Password Reset</h1>
                <p>You have requested to reset your password. Please click the link below to set a new password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you did not request this password reset, you can ignore this email.</p>
                <p>This link will expire in 24 hours for security reasons.</p>
                <br>
                <p>Best regards,<br>The Shopilent Team</p>
            </body>
            </html>";
    }

    public string BuildOrderConfirmationTemplate(Guid orderId, string customerEmail, string appUrl)
    {
        var orderLink = $"{appUrl}/orders/{orderId}";
        
        return $@"
            <html>
            <body>
                <h1>Order Confirmation</h1>
                <p>Thank you for your order! Your order has been received and is being processed.</p>
                <p><strong>Order ID:</strong> {orderId}</p>
                <p><a href='{orderLink}'>View Order Details</a></p>
                <p>You will receive another email when your order has been shipped.</p>
                <br>
                <p>Best regards,<br>The Shopilent Team</p>
            </body>
            </html>";
    }

    public string BuildShippingConfirmationTemplate(Guid orderId, string customerEmail, string trackingNumber, string appUrl)
    {
        var orderLink = $"{appUrl}/orders/{orderId}";
        var trackingInfo = !string.IsNullOrEmpty(trackingNumber)
            ? $"<p><strong>Tracking Number:</strong> {trackingNumber}</p>"
            : "";
        
        return $@"
            <html>
            <body>
                <h1>Shipping Confirmation</h1>
                <p>Good news! Your order has been shipped and is on its way to you.</p>
                <p><strong>Order ID:</strong> {orderId}</p>
                {trackingInfo}
                <p><a href='{orderLink}'>View Order Details</a></p>
                <p>Thank you for choosing Shopilent!</p>
                <br>
                <p>Best regards,<br>The Shopilent Team</p>
            </body>
            </html>";
    }

    public string BuildPaymentConfirmationTemplate(Guid orderId, string customerEmail, decimal amount, string appUrl)
    {
        var orderLink = $"{appUrl}/orders/{orderId}";
        
        return $@"
            <html>
            <body>
                <h1>Payment Confirmation</h1>
                <p>Your payment of <strong>{amount:C}</strong> has been processed successfully.</p>
                <p><strong>Order ID:</strong> {orderId}</p>
                <p><a href='{orderLink}'>View Order Details</a></p>
                <p>Thank you for your business!</p>
                <br>
                <p>Best regards,<br>The Shopilent Team</p>
            </body>
            </html>";
    }

    public string BuildRefundConfirmationTemplate(Guid orderId, string customerEmail, decimal refundAmount, string appUrl)
    {
        var orderLink = $"{appUrl}/orders/{orderId}";
        
        return $@"
            <html>
            <body>
                <h1>Refund Confirmation</h1>
                <p>Your refund of <strong>{refundAmount:C}</strong> has been processed and will appear in your account within 3-5 business days.</p>
                <p><strong>Order ID:</strong> {orderId}</p>
                <p><a href='{orderLink}'>View Order Details</a></p>
                <p>If you have any questions, please contact our customer support.</p>
                <br>
                <p>Best regards,<br>The Shopilent Team</p>
            </body>
            </html>";
    }

    public string BuildWelcomeEmailTemplate(string customerName, string email, string appUrl)
    {
        return $@"
            <html>
            <body>
                <h1>Welcome to Shopilent, {customerName}!</h1>
                <p>Thank you for joining Shopilent. We're excited to have you as part of our community.</p>
                <p>You can start exploring our products and enjoy a seamless shopping experience.</p>
                <p><a href='{appUrl}'>Start Shopping</a></p>
                <p>If you have any questions, feel free to contact our support team.</p>
                <br>
                <p>Best regards,<br>The Shopilent Team</p>
            </body>
            </html>";
    }

    public string BuildOrderStatusUpdateTemplate(Guid orderId, string customerEmail, string oldStatus, string newStatus, string appUrl)
    {
        var orderLink = $"{appUrl}/orders/{orderId}";
        
        return $@"
            <html>
            <body>
                <h1>Order Status Update</h1>
                <p>Your order status has been updated.</p>
                <p><strong>Order ID:</strong> {orderId}</p>
                <p><strong>Previous Status:</strong> {oldStatus}</p>
                <p><strong>Current Status:</strong> {newStatus}</p>
                <p><a href='{orderLink}'>View Order Details</a></p>
                <br>
                <p>Best regards,<br>The Shopilent Team</p>
            </body>
            </html>";
    }
}