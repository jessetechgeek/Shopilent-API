using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Infrastructure.Payments.Configuration;
using Stripe;

namespace Shopilent.Infrastructure.Payments.Providers;

public class StripePaymentProvider : IPaymentProvider
{
    private readonly StripeSettings _settings;
    private readonly ILogger<StripePaymentProvider> _logger;
    private readonly PaymentIntentService _paymentIntentService;
    private readonly RefundService _refundService;
    private readonly CustomerService _customerService;
    private readonly SetupIntentService _setupIntentService;
    private readonly PaymentMethodService _paymentMethodService;

    public PaymentProvider Provider => PaymentProvider.Stripe;

    public StripePaymentProvider(
        IOptions<StripeSettings> settings,
        ILogger<StripePaymentProvider> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        StripeConfiguration.ApiKey = _settings.SecretKey;

        _paymentIntentService = new PaymentIntentService();
        _refundService = new RefundService();
        _customerService = new CustomerService();
        _setupIntentService = new SetupIntentService();
        _paymentMethodService = new PaymentMethodService();
    }

    public async Task<Result<string>> ProcessPaymentAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing Stripe payment for amount {Amount}", request.Amount);

            var options = new PaymentIntentCreateOptions
            {
                Amount = ConvertToStripeAmount(request.Amount),
                Currency = request.Amount.Currency.ToLowerInvariant(),
                PaymentMethod = request.PaymentMethodToken,
                ConfirmationMethod = "manual",
                Confirm = true,
                ReturnUrl = GetReturnUrl(request),
                Metadata = ConvertMetadata(request.Metadata)
            };

            // Add customer if provided
            if (!string.IsNullOrEmpty(request.CustomerId))
            {
                options.Customer = request.CustomerId;
                _logger.LogInformation("Using customer {CustomerId} for payment processing", request.CustomerId);
            }

            // Add external reference if provided
            if (!string.IsNullOrEmpty(request.ExternalReference))
            {
                options.Metadata.Add("external_reference", request.ExternalReference);
            }

            var paymentIntent = await _paymentIntentService.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Stripe payment intent created: {PaymentIntentId}", paymentIntent.Id);

            return Result.Success(paymentIntent.Id);
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Stripe payment failed: {ErrorMessage}", stripeEx.Message);
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(stripeEx.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing Stripe payment");
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(ex.Message));
        }
    }

    public async Task<Result<string>> RefundPaymentAsync(
        string transactionId,
        Money amount = null,
        string reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing Stripe refund for transaction {TransactionId}", transactionId);

            var options = new RefundCreateOptions
            {
                PaymentIntent = transactionId,
                Reason = ConvertRefundReason(reason)
            };

            if (amount != null)
            {
                options.Amount = ConvertToStripeAmount(amount);
            }

            var refund = await _refundService.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Stripe refund created: {RefundId}", refund.Id);

            return Result.Success(refund.Id);
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Stripe refund failed: {ErrorMessage}", stripeEx.Message);
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(stripeEx.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing Stripe refund");
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(ex.Message));
        }
    }

    public async Task<Result<PaymentStatus>> GetPaymentStatusAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting Stripe payment status for transaction {TransactionId}", transactionId);

            var paymentIntent =
                await _paymentIntentService.GetAsync(transactionId, cancellationToken: cancellationToken);

            var status = ConvertStripeStatus(paymentIntent.Status);

            _logger.LogInformation("Stripe payment status retrieved: {Status}", status);

            return Result.Success(status);
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Failed to get Stripe payment status: {ErrorMessage}", stripeEx.Message);
            return Result.Failure<PaymentStatus>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(stripeEx.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting Stripe payment status");
            return Result.Failure<PaymentStatus>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(ex.Message));
        }
    }

    private static long ConvertToStripeAmount(Money amount)
    {
        // Stripe expects amounts in cents for most currencies
        // This is a simplified conversion - in production, you'd need to handle
        // zero-decimal currencies like JPY differently
        return (long)(amount.Amount * 100);
    }

    private static PaymentStatus ConvertStripeStatus(string stripeStatus)
    {
        return stripeStatus switch
        {
            "requires_payment_method" => PaymentStatus.Pending,
            "requires_confirmation" => PaymentStatus.Pending,
            "requires_action" => PaymentStatus.Pending,
            "processing" => PaymentStatus.Processing,
            "succeeded" => PaymentStatus.Succeeded,
            "canceled" => PaymentStatus.Canceled,
            _ => PaymentStatus.Failed
        };
    }

    private static string ConvertRefundReason(string reason)
    {
        return reason switch
        {
            "duplicate" => "duplicate",
            "fraudulent" => "fraudulent",
            "requested_by_customer" => "requested_by_customer",
            _ => "requested_by_customer"
        };
    }

    private static Dictionary<string, string> ConvertMetadata(Dictionary<string, object> metadata)
    {
        var stripeMetadata = new Dictionary<string, string>();

        foreach (var kvp in metadata)
        {
            stripeMetadata.Add(kvp.Key, kvp.Value?.ToString() ?? string.Empty);
        }

        return stripeMetadata;
    }

    private static string GetReturnUrl(PaymentRequest request)
    {
        // In a real implementation, this would come from configuration
        // or be passed in the request
        return "https://your-app.com/payment/return";
    }

    public async Task<Result<string>> CreateCustomerAsync(
        string userId,
        string email,
        Dictionary<string, object> metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Stripe customer for user {UserId}", userId);

            var options = new CustomerCreateOptions
            {
                Email = email,
                Metadata = new Dictionary<string, string>
                {
                    ["user_id"] = userId
                }
            };

            if (metadata != null)
            {
                foreach (var kvp in metadata)
                {
                    options.Metadata.Add($"custom_{kvp.Key}", kvp.Value?.ToString() ?? string.Empty);
                }
            }

            var customer = await _customerService.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Stripe customer created: {CustomerId} for user {UserId}", customer.Id, userId);

            return Result.Success(customer.Id);
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Stripe customer creation failed: {ErrorMessage}", stripeEx.Message);
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(stripeEx.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating Stripe customer");
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(ex.Message));
        }
    }

    public async Task<Result<string>> AttachPaymentMethodToCustomerAsync(
        string paymentMethodToken,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attaching payment method {PaymentMethodToken} to customer {CustomerId}",
                paymentMethodToken, customerId);

            var options = new PaymentMethodAttachOptions
            {
                Customer = customerId
            };

            var paymentMethod = await _paymentMethodService.AttachAsync(
                paymentMethodToken,
                options,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Payment method {PaymentMethodId} attached to customer {CustomerId}",
                paymentMethod.Id, customerId);

            return Result.Success(paymentMethod.Id);
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Stripe payment method attachment failed: {ErrorMessage}", stripeEx.Message);
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(stripeEx.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error attaching payment method to customer");
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(ex.Message));
        }
    }
}