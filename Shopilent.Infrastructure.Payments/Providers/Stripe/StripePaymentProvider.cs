using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopilent.Application.Abstractions.Payments;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Infrastructure.Payments.Abstractions;
using Shopilent.Infrastructure.Payments.Models;
using Shopilent.Infrastructure.Payments.Providers.Base;
using Shopilent.Infrastructure.Payments.Settings;
using Stripe;

namespace Shopilent.Infrastructure.Payments.Providers.Stripe;

internal class StripePaymentProvider : PaymentProviderBase
{
    private readonly StripeSettings _settings;
    private readonly PaymentIntentService _paymentIntentService;
    private readonly RefundService _refundService;
    private readonly CustomerService _customerService;
    private readonly SetupIntentService _setupIntentService;
    private readonly PaymentMethodService _paymentMethodService;
    private readonly ChargeService _chargeService;
    private readonly EventService _eventService;

    public override PaymentProvider Provider => PaymentProvider.Stripe;

    public StripePaymentProvider(
        IOptions<StripeSettings> settings,
        ILogger<StripePaymentProvider> logger) : base(logger)
    {
        _settings = settings.Value;

        StripeConfiguration.ApiKey = _settings.SecretKey;

        _paymentIntentService = new PaymentIntentService();
        _refundService = new RefundService();
        _customerService = new CustomerService();
        _setupIntentService = new SetupIntentService();
        _paymentMethodService = new PaymentMethodService();
        _chargeService = new ChargeService();
        _eventService = new EventService();
    }

    public override async Task<Result<PaymentResult>> ProcessPaymentAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Processing Stripe payment for amount {Amount}", request.Amount);

            var options = new PaymentIntentCreateOptions
            {
                Amount = ConvertToStripeAmount(request.Amount),
                Currency = request.Amount.Currency.ToLowerInvariant(),
                PaymentMethod = request.PaymentMethodToken,
                ConfirmationMethod = "automatic",
                Confirm = true,
                ReturnUrl = GetReturnUrl(request),
                Metadata = ConvertMetadata(request.Metadata),
                // Enable 3D Secure authentication when needed
                PaymentMethodOptions = new PaymentIntentPaymentMethodOptionsOptions
                {
                    Card = new PaymentIntentPaymentMethodOptionsCardOptions
                    {
                        RequestThreeDSecure = "automatic"
                    }
                }
            };

            // Add customer if provided
            if (!string.IsNullOrEmpty(request.CustomerId))
            {
                options.Customer = request.CustomerId;
                Logger.LogInformation("Using customer {CustomerId} for payment processing", request.CustomerId);
            }

            var paymentIntent = await _paymentIntentService.CreateAsync(options, cancellationToken: cancellationToken);

            Logger.LogInformation("Stripe payment intent created: {PaymentIntentId} with status: {Status}",
                paymentIntent.Id, paymentIntent.Status);

            var paymentProcessingResult = await BuildPaymentResultAsync(paymentIntent);

            return Result.Success(paymentProcessingResult);
        }
        catch (StripeException stripeEx)
        {
            Logger.LogError(stripeEx, "Stripe payment failed: {ErrorType} - {ErrorMessage}",
                stripeEx.StripeError?.Type, stripeEx.Message);

            var error = HandleStripeException(stripeEx);
            return Result.Failure<PaymentResult>(error);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error processing Stripe payment");
            return Result.Failure<PaymentResult>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(ex.Message));
        }
    }

    public override async Task<Result<string>> RefundPaymentAsync(
        string transactionId,
        Money amount = null,
        string reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Processing Stripe refund for transaction {TransactionId}", transactionId);

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

            Logger.LogInformation("Stripe refund created: {RefundId}", refund.Id);

            return Result.Success(refund.Id);
        }
        catch (StripeException stripeEx)
        {
            Logger.LogError(stripeEx, "Stripe refund failed: {ErrorMessage}", stripeEx.Message);
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(stripeEx.Message));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error processing Stripe refund");
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(ex.Message));
        }
    }

    public override async Task<Result<PaymentStatus>> GetPaymentStatusAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Getting Stripe payment status for transaction {TransactionId}", transactionId);

            var paymentIntent =
                await _paymentIntentService.GetAsync(transactionId, cancellationToken: cancellationToken);

            var status = ConvertStripeStatus(paymentIntent.Status);

            Logger.LogInformation("Stripe payment status retrieved: {Status}", status);

            return Result.Success(status);
        }
        catch (StripeException stripeEx)
        {
            Logger.LogError(stripeEx, "Failed to get Stripe payment status: {ErrorMessage}", stripeEx.Message);
            return Result.Failure<PaymentStatus>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(stripeEx.Message));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error getting Stripe payment status");
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
            "requires_confirmation" => PaymentStatus.RequiresConfirmation,
            "requires_action" => PaymentStatus.RequiresAction,
            "processing" => PaymentStatus.Processing,
            "succeeded" => PaymentStatus.Succeeded,
            "canceled" => PaymentStatus.Canceled,
            "payment_failed" => PaymentStatus.Failed,
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

    public override async Task<Result<string>> CreateCustomerAsync(
        string userId,
        string email,
        Dictionary<string, object> metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Creating Stripe customer for user {UserId}", userId);

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

            Logger.LogInformation("Stripe customer created: {CustomerId} for user {UserId}", customer.Id, userId);

            return Result.Success(customer.Id);
        }
        catch (StripeException stripeEx)
        {
            Logger.LogError(stripeEx, "Stripe customer creation failed: {ErrorMessage}", stripeEx.Message);
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(stripeEx.Message));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error creating Stripe customer");
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(ex.Message));
        }
    }

    public override async Task<Result<string>> AttachPaymentMethodToCustomerAsync(
        string paymentMethodToken,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Attaching payment method {PaymentMethodToken} to customer {CustomerId}",
                paymentMethodToken, customerId);

            var options = new PaymentMethodAttachOptions
            {
                Customer = customerId
            };

            var paymentMethod = await _paymentMethodService.AttachAsync(
                paymentMethodToken,
                options,
                cancellationToken: cancellationToken);

            Logger.LogInformation("Payment method {PaymentMethodId} attached to customer {CustomerId}",
                paymentMethod.Id, customerId);

            return Result.Success(paymentMethod.Id);
        }
        catch (StripeException stripeEx)
        {
            Logger.LogError(stripeEx, "Stripe payment method attachment failed: {ErrorMessage}", stripeEx.Message);
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(stripeEx.Message));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error attaching payment method to customer");
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(ex.Message));
        }
    }

    private async Task<PaymentResult> BuildPaymentResultAsync(PaymentIntent paymentIntent)
    {
        var result = new PaymentResult
        {
            TransactionId = paymentIntent.Id,
            Status = ConvertStripeStatus(paymentIntent.Status),
            ClientSecret = paymentIntent.ClientSecret,
            RequiresAction = paymentIntent.Status == "requires_action",
            Metadata = ConvertMetadataToObject(paymentIntent.Metadata)
        };

        // Handle next action type for 3D Secure
        if (paymentIntent.NextAction != null)
        {
            result.NextActionType = paymentIntent.NextAction.Type;
        }

        // Extract risk information if available from latest charge
        if (!string.IsNullOrEmpty(paymentIntent.LatestChargeId))
        {
            try
            {
                var charge = await _chargeService.GetAsync(paymentIntent.LatestChargeId);

                if (charge?.Outcome != null)
                {
                    result.RiskLevel = charge.Outcome.RiskLevel;

                    if (charge.Outcome.Type == "issuer_declined")
                    {
                        result.DeclineReason = charge.Outcome.Reason ?? "declined_by_issuer";
                    }
                }

                if (!string.IsNullOrEmpty(charge?.FailureCode))
                {
                    result.FailureReason = charge.FailureCode;
                }
            }
            catch (StripeException ex)
            {
                Logger.LogWarning(ex, "Failed to retrieve charge details for PaymentIntent {PaymentIntentId}",
                    paymentIntent.Id);
            }
        }

        return result;
    }

    private Domain.Common.Errors.Error HandleStripeException(StripeException stripeEx)
    {
        return stripeEx.StripeError?.Type switch
        {
            "card_error" => HandleCardError(stripeEx.StripeError),
            "validation_error" => Domain.Payments.Errors.PaymentErrors.ProcessingFailed(stripeEx.Message),
            "api_error" => Domain.Payments.Errors.PaymentErrors.ProcessingFailed(
                "Payment service temporarily unavailable"),
            "authentication_error" => Domain.Payments.Errors.PaymentErrors.ProcessingFailed(
                "Payment authentication failed"),
            "rate_limit_error" => Domain.Payments.Errors.PaymentErrors.ProcessingFailed(
                "Too many requests, please try again later"),
            "idempotency_error" => Domain.Payments.Errors.PaymentErrors.ProcessingFailed("Duplicate payment request"),
            _ => Domain.Payments.Errors.PaymentErrors.ProcessingFailed(stripeEx.Message)
        };
    }

    private Domain.Common.Errors.Error HandleCardError(StripeError error)
    {
        return error.Code switch
        {
            "card_declined" => DetermineDeclineReason(error.DeclineCode),
            "expired_card" => Domain.Payments.Errors.PaymentErrors.ExpiredCard,
            "insufficient_funds" => Domain.Payments.Errors.PaymentErrors.InsufficientFunds,
            "incorrect_cvc" => Domain.Payments.Errors.PaymentErrors.InvalidCard,
            "incorrect_number" => Domain.Payments.Errors.PaymentErrors.InvalidCard,
            "invalid_cvc" => Domain.Payments.Errors.PaymentErrors.InvalidCard,
            "invalid_expiry_month" => Domain.Payments.Errors.PaymentErrors.InvalidCard,
            "invalid_expiry_year" => Domain.Payments.Errors.PaymentErrors.InvalidCard,
            "invalid_number" => Domain.Payments.Errors.PaymentErrors.InvalidCard,
            "processing_error" => Domain.Payments.Errors.PaymentErrors.ProcessingFailed("Payment processing error"),
            "authentication_required" => Domain.Payments.Errors.PaymentErrors.AuthenticationRequired,
            _ => Domain.Payments.Errors.PaymentErrors.CardDeclined(error.Message ?? "Unknown reason")
        };
    }

    private Domain.Common.Errors.Error DetermineDeclineReason(string declineCode)
    {
        return declineCode switch
        {
            "insufficient_funds" => Domain.Payments.Errors.PaymentErrors.InsufficientFunds,
            "fraudulent" => Domain.Payments.Errors.PaymentErrors.FraudSuspected,
            "stolen_card" => Domain.Payments.Errors.PaymentErrors.FraudSuspected,
            "lost_card" => Domain.Payments.Errors.PaymentErrors.FraudSuspected,
            "pickup_card" => Domain.Payments.Errors.PaymentErrors.FraudSuspected,
            "restricted_card" => Domain.Payments.Errors.PaymentErrors.FraudSuspected,
            "security_violation" => Domain.Payments.Errors.PaymentErrors.FraudSuspected,
            "expired_card" => Domain.Payments.Errors.PaymentErrors.ExpiredCard,
            "incorrect_cvc" => Domain.Payments.Errors.PaymentErrors.InvalidCard,
            "processing_error" => Domain.Payments.Errors.PaymentErrors.ProcessingFailed("Payment processing error"),
            "issuer_not_available" => Domain.Payments.Errors.PaymentErrors.ProcessingFailed(
                "Card issuer temporarily unavailable"),
            "try_again_later" => Domain.Payments.Errors.PaymentErrors.ProcessingFailed("Please try again later"),
            "risk_threshold" => Domain.Payments.Errors.PaymentErrors.RiskLevelTooHigh,
            _ => Domain.Payments.Errors.PaymentErrors.CardDeclined(declineCode ?? "Unknown decline reason")
        };
    }

    private static Dictionary<string, object> ConvertMetadataToObject(Dictionary<string, string> metadata)
    {
        var result = new Dictionary<string, object>();

        foreach (var kvp in metadata)
        {
            result.Add(kvp.Key, kvp.Value);
        }

        return result;
    }

    public override async Task<Result<WebhookResult>> ProcessWebhookAsync(
        string webhookPayload,
        string signature = null,
        Dictionary<string, string> headers = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Processing Stripe webhook");

            // Verify webhook signature if provided
            Event stripeEvent;
            if (!string.IsNullOrEmpty(signature) && !string.IsNullOrEmpty(_settings.WebhookSecret))
            {
                try
                {
                    stripeEvent = EventUtility.ConstructEvent(
                        webhookPayload,
                        signature,
                        _settings.WebhookSecret);
                    Logger.LogInformation("Stripe webhook signature verified successfully");
                }
                catch (StripeException ex)
                {
                    Logger.LogError(ex, "Stripe webhook signature verification failed");
                    return Result.Failure<WebhookResult>(
                        Domain.Payments.Errors.PaymentErrors.ProcessingFailed("Invalid webhook signature"));
                }
            }
            else
            {
                // Parse webhook payload without signature verification (not recommended for production)
                Logger.LogWarning("Processing Stripe webhook without signature verification");
                try
                {
                    stripeEvent = Event.FromJson(webhookPayload);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to parse Stripe webhook payload");
                    return Result.Failure<WebhookResult>(
                        Domain.Payments.Errors.PaymentErrors.ProcessingFailed("Invalid webhook payload"));
                }
            }

            // Process the webhook event
            var webhookResult = await ProcessStripeEventAsync(stripeEvent, cancellationToken);

            return Result.Success(webhookResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error processing Stripe webhook");
            return Result.Failure<WebhookResult>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(ex.Message));
        }
    }

    private async Task<WebhookResult> ProcessStripeEventAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        var result = new WebhookResult
        {
            EventId = stripeEvent.Id,
            EventType = stripeEvent.Type,
            ProcessedAt = DateTime.UtcNow,
            EventData = new Dictionary<string, object>(),
            IsProcessed = false
        };

        Logger.LogInformation("Processing Stripe event: {EventType} with ID: {EventId}",
            stripeEvent.Type, stripeEvent.Id);

        try
        {
            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    await HandlePaymentIntentSucceeded(stripeEvent, result, cancellationToken);
                    break;

                case "payment_intent.payment_failed":
                    await HandlePaymentIntentFailed(stripeEvent, result, cancellationToken);
                    break;

                case "payment_intent.requires_action":
                    await HandlePaymentIntentRequiresAction(stripeEvent, result, cancellationToken);
                    break;

                case "payment_intent.canceled":
                    await HandlePaymentIntentCanceled(stripeEvent, result, cancellationToken);
                    break;

                case "charge.succeeded":
                    await HandleChargeSucceeded(stripeEvent, result, cancellationToken);
                    break;

                case "charge.dispute.created":
                    await HandleChargeDisputeCreated(stripeEvent, result, cancellationToken);
                    break;

                case "customer.created":
                    await HandleCustomerCreated(stripeEvent, result, cancellationToken);
                    break;

                case "customer.updated":
                    await HandleCustomerUpdated(stripeEvent, result, cancellationToken);
                    break;

                case "payment_method.attached":
                    await HandlePaymentMethodAttached(stripeEvent, result, cancellationToken);
                    break;

                default:
                    Logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    result.ProcessingMessage = $"Event type {stripeEvent.Type} is not handled";
                    result.IsProcessed = true; // Mark as processed to avoid retries
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing Stripe event {EventType} with ID {EventId}",
                stripeEvent.Type, stripeEvent.Id);
            result.ProcessingMessage = $"Error processing event: {ex.Message}";
            result.IsProcessed = false;
        }

        return result;
    }

    private async Task HandlePaymentIntentSucceeded(Event stripeEvent, WebhookResult result,
        CancellationToken cancellationToken)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null)
        {
            result.ProcessingMessage = "Invalid PaymentIntent data in webhook";
            return;
        }

        result.TransactionId = paymentIntent.Id;
        result.PaymentStatus = PaymentStatus.Succeeded;
        result.CustomerId = paymentIntent.CustomerId;
        result.EventData.Add("amount", paymentIntent.Amount);
        result.EventData.Add("currency", paymentIntent.Currency);
        result.EventData.Add("payment_method", paymentIntent.PaymentMethodId);

        if (paymentIntent.Metadata != null)
        {
            result.EventData.Add("metadata", paymentIntent.Metadata);
        }

        result.OrderId = paymentIntent.Metadata["orderId"];
        result.ProcessingMessage = "Payment succeeded";
        result.IsProcessed = true;

        Logger.LogInformation("Payment succeeded: {PaymentIntentId}", paymentIntent.Id);
    }

    private async Task HandlePaymentIntentFailed(Event stripeEvent, WebhookResult result,
        CancellationToken cancellationToken)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null)
        {
            result.ProcessingMessage = "Invalid PaymentIntent data in webhook";
            return;
        }

        result.TransactionId = paymentIntent.Id;
        result.PaymentStatus = PaymentStatus.Failed;
        result.CustomerId = paymentIntent.CustomerId;
        result.EventData.Add("last_payment_error", paymentIntent.LastPaymentError?.Message ?? "Unknown error");

        if (paymentIntent.Metadata != null)
        {
            result.EventData.Add("metadata", paymentIntent.Metadata);
        }

        result.OrderId = paymentIntent.Metadata["orderId"];
        result.ProcessingMessage = $"Payment failed: {paymentIntent.LastPaymentError?.Message ?? "Unknown error"}";
        result.IsProcessed = true;

        Logger.LogWarning("Payment failed: {PaymentIntentId}, Error: {Error}",
            paymentIntent.Id, paymentIntent.LastPaymentError?.Message);
    }

    private async Task HandlePaymentIntentRequiresAction(Event stripeEvent, WebhookResult result,
        CancellationToken cancellationToken)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null)
        {
            result.ProcessingMessage = "Invalid PaymentIntent data in webhook";
            return;
        }

        result.TransactionId = paymentIntent.Id;
        result.PaymentStatus = PaymentStatus.RequiresAction;
        result.CustomerId = paymentIntent.CustomerId;
        result.EventData.Add("next_action", paymentIntent.NextAction?.Type ?? "unknown");

        if (paymentIntent.Metadata != null)
        {
            result.EventData.Add("metadata", paymentIntent.Metadata);
        }

        result.OrderId = paymentIntent.Metadata["orderId"];
        result.ProcessingMessage = "Payment requires additional action";
        result.IsProcessed = true;

        Logger.LogInformation("Payment requires action: {PaymentIntentId}", paymentIntent.Id);
    }

    private async Task HandlePaymentIntentCanceled(Event stripeEvent, WebhookResult result,
        CancellationToken cancellationToken)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null)
        {
            result.ProcessingMessage = "Invalid PaymentIntent data in webhook";
            return;
        }

        result.TransactionId = paymentIntent.Id;
        result.PaymentStatus = PaymentStatus.Canceled;
        result.CustomerId = paymentIntent.CustomerId;

        if (paymentIntent.Metadata != null)
        {
            result.EventData.Add("metadata", paymentIntent.Metadata);
        }

        result.OrderId = paymentIntent.Metadata["orderId"];
        result.ProcessingMessage = "Payment was canceled";
        result.IsProcessed = true;

        Logger.LogInformation("Payment canceled: {PaymentIntentId}", paymentIntent.Id);
    }

    private async Task HandleChargeSucceeded(Event stripeEvent, WebhookResult result,
        CancellationToken cancellationToken)
    {
        var charge = stripeEvent.Data.Object as Charge;
        if (charge == null)
        {
            result.ProcessingMessage = "Invalid Charge data in webhook";
            return;
        }

        result.TransactionId = charge.PaymentIntentId ?? charge.Id;
        result.PaymentStatus = PaymentStatus.Succeeded;
        result.CustomerId = charge.CustomerId;
        result.EventData.Add("charge_id", charge.Id);
        result.EventData.Add("payment_intent_id", charge.PaymentIntentId ?? string.Empty);
        result.EventData.Add("amount", charge.Amount);
        result.EventData.Add("currency", charge.Currency);
        result.EventData.Add("payment_method", charge.PaymentMethod ?? string.Empty);

        // Add charge outcome information (risk assessment, etc.)
        if (charge.Outcome != null)
        {
            result.EventData.Add("risk_level", charge.Outcome.RiskLevel ?? "unknown");
            result.EventData.Add("outcome_type", charge.Outcome.Type ?? "unknown");
            result.EventData.Add("seller_message", charge.Outcome.SellerMessage ?? string.Empty);

            if (!string.IsNullOrEmpty(charge.Outcome.Reason))
            {
                result.EventData.Add("outcome_reason", charge.Outcome.Reason);
            }
        }

        // Add payment method details if available
        if (charge.PaymentMethodDetails?.Card != null)
        {
            var card = charge.PaymentMethodDetails.Card;
            result.EventData.Add("card_brand", card.Brand ?? string.Empty);
            result.EventData.Add("card_last4", card.Last4 ?? string.Empty);
            result.EventData.Add("card_country", card.Country ?? string.Empty);

            if (card.ThreeDSecure != null)
            {
                result.EventData.Add("three_d_secure_result", card.ThreeDSecure.Result ?? string.Empty);
                result.EventData.Add("three_d_secure_version", card.ThreeDSecure.Version ?? string.Empty);
            }
        }

        // Add charge metadata
        if (charge.Metadata != null && charge.Metadata.Count > 0)
        {
            result.EventData.Add("metadata", charge.Metadata);
        }

        // Add billing details if available
        if (charge.BillingDetails != null)
        {
            result.EventData.Add("billing_email", charge.BillingDetails.Email ?? string.Empty);
            result.EventData.Add("billing_name", charge.BillingDetails.Name ?? string.Empty);
        }

        result.OrderId = charge.Metadata["orderId"];
        result.ProcessingMessage = "Charge succeeded";
        result.IsProcessed = true;

        Logger.LogInformation(
            "Charge succeeded: {ChargeId} for PaymentIntent: {PaymentIntentId}, Amount: {Amount} {Currency}",
            charge.Id, charge.PaymentIntentId, charge.Amount, charge.Currency?.ToUpperInvariant());

        // Log 3DS information if available
        if (charge.PaymentMethodDetails?.Card?.ThreeDSecure != null)
        {
            var threeDSecure = charge.PaymentMethodDetails.Card.ThreeDSecure;
            Logger.LogInformation(
                "3D Secure authentication completed for charge {ChargeId}: Result={Result}, Version={Version}",
                charge.Id, threeDSecure.Result, threeDSecure.Version);
        }
    }

    private async Task HandleChargeDisputeCreated(Event stripeEvent, WebhookResult result,
        CancellationToken cancellationToken)
    {
        var dispute = stripeEvent.Data.Object as Dispute;
        if (dispute == null)
        {
            result.ProcessingMessage = "Invalid Dispute data in webhook";
            return;
        }

        result.TransactionId = dispute.ChargeId;
        result.EventData.Add("dispute_id", dispute.Id);
        result.EventData.Add("dispute_reason", dispute.Reason);
        result.EventData.Add("dispute_amount", dispute.Amount);
        result.EventData.Add("dispute_currency", dispute.Currency);

        result.OrderId = dispute.Metadata["orderId"];
        result.ProcessingMessage = $"Dispute created for charge: {dispute.ChargeId}";
        result.IsProcessed = true;

        Logger.LogWarning("Dispute created for charge: {ChargeId}, Reason: {Reason}",
            dispute.ChargeId, dispute.Reason);
    }

    private async Task HandleCustomerCreated(Event stripeEvent, WebhookResult result,
        CancellationToken cancellationToken)
    {
        var customer = stripeEvent.Data.Object as Customer;
        if (customer == null)
        {
            result.ProcessingMessage = "Invalid Customer data in webhook";
            return;
        }

        result.CustomerId = customer.Id;
        result.EventData.Add("email", customer.Email ?? string.Empty);

        if (customer.Metadata != null)
        {
            result.EventData.Add("metadata", customer.Metadata);
        }

        result.ProcessingMessage = $"Customer created: {customer.Id}";
        result.IsProcessed = true;

        Logger.LogInformation("Customer created: {CustomerId}", customer.Id);
    }

    private async Task HandleCustomerUpdated(Event stripeEvent, WebhookResult result,
        CancellationToken cancellationToken)
    {
        var customer = stripeEvent.Data.Object as Customer;
        if (customer == null)
        {
            result.ProcessingMessage = "Invalid Customer data in webhook";
            return;
        }

        result.CustomerId = customer.Id;
        result.EventData.Add("email", customer.Email ?? string.Empty);

        if (customer.Metadata != null)
        {
            result.EventData.Add("metadata", customer.Metadata);
        }

        result.ProcessingMessage = $"Customer updated: {customer.Id}";
        result.IsProcessed = true;

        Logger.LogInformation("Customer updated: {CustomerId}", customer.Id);
    }

    private async Task HandlePaymentMethodAttached(Event stripeEvent, WebhookResult result,
        CancellationToken cancellationToken)
    {
        var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
        if (paymentMethod == null)
        {
            result.ProcessingMessage = "Invalid PaymentMethod data in webhook";
            return;
        }

        result.CustomerId = paymentMethod.CustomerId;
        result.EventData.Add("payment_method_id", paymentMethod.Id);
        result.EventData.Add("payment_method_type", paymentMethod.Type);

        if (paymentMethod.Card != null)
        {
            result.EventData.Add("card_brand", paymentMethod.Card.Brand);
            result.EventData.Add("card_last4", paymentMethod.Card.Last4);
            result.EventData.Add("card_exp_month", paymentMethod.Card.ExpMonth);
            result.EventData.Add("card_exp_year", paymentMethod.Card.ExpYear);
        }

        result.ProcessingMessage = $"Payment method attached: {paymentMethod.Id}";
        result.IsProcessed = true;

        Logger.LogInformation("Payment method attached: {PaymentMethodId} to customer: {CustomerId}",
            paymentMethod.Id, paymentMethod.CustomerId);
    }
}