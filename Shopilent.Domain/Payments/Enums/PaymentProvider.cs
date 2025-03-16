namespace Shopilent.Domain.Payments.Enums;

public enum PaymentProvider
{
    Stripe,
    PayPal,
    Braintree,
    Square,
    Adyen,
    Authorize,
    Custom
}