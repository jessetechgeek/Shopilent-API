namespace Shopilent.Domain.Payments.Enums;

public enum PaymentStatus
{
    Pending,
    Processing,
    Succeeded,
    Failed,
    Refunded,
    Disputed,
    Canceled,
    RequiresAction,
    RequiresConfirmation,
    Declined
}