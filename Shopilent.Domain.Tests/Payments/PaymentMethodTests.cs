using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Payments;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Payments.ValueObjects;

namespace Shopilent.Domain.Tests.Payments;

public class PaymentMethodTests
{
    private User CreateTestUser()
    {
        var emailResult = Email.Create("test@example.com");
        var fullNameResult = FullName.Create("Test", "User");
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        Assert.True(userResult.IsSuccess);
        return userResult.Value;
    }

    [Fact]
    public void CreateCardMethod_WithValidParameters_ShouldCreateCardMethod()
    {
        // Arrange
        var user = CreateTestUser();
        var provider = PaymentProvider.Stripe;
        var token = "tok_visa_123";
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;
        var isDefault = true;

        // Act
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            provider,
            token,
            cardDetails,
            isDefault);

        // Assert
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;
        Assert.Equal(user.Id, paymentMethod.UserId);
        Assert.Equal(PaymentMethodType.CreditCard, paymentMethod.Type);
        Assert.Equal(provider, paymentMethod.Provider);
        Assert.Equal(token, paymentMethod.Token);
        Assert.Equal("Visa ending in 4242", paymentMethod.DisplayName);
        Assert.Equal(cardDetails.Brand, paymentMethod.CardBrand);
        Assert.Equal(cardDetails.LastFourDigits, paymentMethod.LastFourDigits);
        Assert.Equal(cardDetails.ExpiryDate, paymentMethod.ExpiryDate);
        Assert.Equal(isDefault, paymentMethod.IsDefault);
        Assert.True(paymentMethod.IsActive);
        Assert.Empty(paymentMethod.Metadata);
    }

    [Fact]
    public void CreateCardMethod_WithNullUser_ShouldReturnFailure()
    {
        // Arrange
        User user = null;
        var provider = PaymentProvider.Stripe;
        var token = "tok_visa_123";
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;

        // Act
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            provider,
            token,
            cardDetails);

        // Assert
        Assert.True(paymentMethodResult.IsFailure);
        Assert.Equal("User.NotFound", paymentMethodResult.Error.Code);
    }

    [Fact]
    public void CreateCardMethod_WithEmptyToken_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var provider = PaymentProvider.Stripe;
        var token = string.Empty;
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;

        // Act
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            provider,
            token,
            cardDetails);

        // Assert
        Assert.True(paymentMethodResult.IsFailure);
        Assert.Equal("PaymentMethod.TokenRequired", paymentMethodResult.Error.Code);
    }

    [Fact]
    public void CreateCardMethod_WithNullCardDetails_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var provider = PaymentProvider.Stripe;
        var token = "tok_visa_123";
        PaymentCardDetails cardDetails = null;

        // Act
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            provider,
            token,
            cardDetails);

        // Assert
        Assert.True(paymentMethodResult.IsFailure);
        Assert.Equal("PaymentMethod.InvalidCardDetails", paymentMethodResult.Error.Code);
    }

    [Fact]
    public void CreatePayPalMethod_WithValidParameters_ShouldCreatePayPalMethod()
    {
        // Arrange
        var user = CreateTestUser();
        var token = "paypal_token_123";
        var email = "customer@example.com";
        var isDefault = true;

        // Act
        var paymentMethodResult = PaymentMethod.CreatePayPalMethod(
            user,
            token,
            email,
            isDefault);

        // Assert
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;
        Assert.Equal(user.Id, paymentMethod.UserId);
        Assert.Equal(PaymentMethodType.PayPal, paymentMethod.Type);
        Assert.Equal(PaymentProvider.PayPal, paymentMethod.Provider);
        Assert.Equal(token, paymentMethod.Token);
        Assert.Equal($"PayPal ({email})", paymentMethod.DisplayName);
        Assert.Null(paymentMethod.CardBrand);
        Assert.Null(paymentMethod.LastFourDigits);
        Assert.Null(paymentMethod.ExpiryDate);
        Assert.Equal(isDefault, paymentMethod.IsDefault);
        Assert.True(paymentMethod.IsActive);
        Assert.True(paymentMethod.Metadata.ContainsKey("email"));
        Assert.Equal(email, paymentMethod.Metadata["email"]);
    }

    [Fact]
    public void UpdateDisplayName_ShouldUpdateName()
    {
        // Arrange
        var user = CreateTestUser();
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            cardDetails);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        var newDisplayName = "My Primary Card";

        // Act
        var updateResult = paymentMethod.UpdateDisplayName(newDisplayName);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(newDisplayName, paymentMethod.DisplayName);
    }

    [Fact]
    public void UpdateDisplayName_WithEmptyValue_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            cardDetails);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        var emptyName = string.Empty;

        // Act
        var updateResult = paymentMethod.UpdateDisplayName(emptyName);

        // Assert
        Assert.True(updateResult.IsFailure);
        Assert.Equal("PaymentMethod.DisplayNameRequired", updateResult.Error.Code);
    }

    [Fact]
    public void SetDefault_ShouldUpdateIsDefault()
    {
        // Arrange
        var user = CreateTestUser();
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            cardDetails,
            false);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        Assert.False(paymentMethod.IsDefault);

        // Act
        var updateResult = paymentMethod.SetDefault(true);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.True(paymentMethod.IsDefault);
    }

    [Fact]
    public void Activate_ShouldActivatePaymentMethod()
    {
        // Arrange
        var user = CreateTestUser();
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            cardDetails);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        var deactivateResult = paymentMethod.Deactivate();
        Assert.True(deactivateResult.IsSuccess);
        Assert.False(paymentMethod.IsActive);

        // Act
        var activateResult = paymentMethod.Activate();

        // Assert
        Assert.True(activateResult.IsSuccess);
        Assert.True(paymentMethod.IsActive);
    }

    [Fact]
    public void Deactivate_ShouldDeactivatePaymentMethod()
    {
        // Arrange
        var user = CreateTestUser();
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            cardDetails);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        Assert.True(paymentMethod.IsActive);

        // Act
        var deactivateResult = paymentMethod.Deactivate();

        // Assert
        Assert.True(deactivateResult.IsSuccess);
        Assert.False(paymentMethod.IsActive);
    }

    [Fact]
    public void UpdateToken_ShouldUpdateToken()
    {
        // Arrange
        var user = CreateTestUser();
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            cardDetails);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        var newToken = "tok_visa_456";

        // Act
        var updateResult = paymentMethod.UpdateToken(newToken);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(newToken, paymentMethod.Token);
    }

    [Fact]
    public void UpdateToken_WithEmptyValue_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            cardDetails);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        var emptyToken = string.Empty;

        // Act
        var updateResult = paymentMethod.UpdateToken(emptyToken);

        // Assert
        Assert.True(updateResult.IsFailure);
        Assert.Equal("PaymentMethod.TokenRequired", updateResult.Error.Code);
    }

    [Fact]
    public void UpdateCardDetails_ShouldUpdateCardDetails()
    {
        // Arrange
        var user = CreateTestUser();
        var oldCardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(oldCardDetailsResult.IsSuccess);
        var oldCardDetails = oldCardDetailsResult.Value;
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            oldCardDetails);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        var newCardDetailsResult = PaymentCardDetails.Create("Mastercard", "5678", DateTime.UtcNow.AddYears(2));
        Assert.True(newCardDetailsResult.IsSuccess);
        var newCardDetails = newCardDetailsResult.Value;

        // Act
        var updateResult = paymentMethod.UpdateCardDetails(newCardDetails);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(newCardDetails.Brand, paymentMethod.CardBrand);
        Assert.Equal(newCardDetails.LastFourDigits, paymentMethod.LastFourDigits);
        Assert.Equal(newCardDetails.ExpiryDate, paymentMethod.ExpiryDate);
        Assert.Equal($"{newCardDetails.Brand} ending in {newCardDetails.LastFourDigits}", paymentMethod.DisplayName);
    }

    [Fact]
    public void UpdateCardDetails_WithNullDetails_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            cardDetails);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        PaymentCardDetails newCardDetails = null;

        // Act
        var updateResult = paymentMethod.UpdateCardDetails(newCardDetails);

        // Assert
        Assert.True(updateResult.IsFailure);
        Assert.Equal("PaymentMethod.InvalidCardDetails", updateResult.Error.Code);
    }

    [Fact]
    public void UpdateCardDetails_WithNonCardMethod_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var paypalMethodResult = PaymentMethod.CreatePayPalMethod(
            user,
            "paypal_token_123",
            "customer@example.com");
        Assert.True(paypalMethodResult.IsSuccess);
        var paymentMethod = paypalMethodResult.Value;

        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;

        // Act
        var updateResult = paymentMethod.UpdateCardDetails(cardDetails);

        // Assert
        Assert.True(updateResult.IsFailure);
        Assert.Equal("PaymentMethod.InvalidCardDetails", updateResult.Error.Code);
    }

    [Fact]
    public void UpdateMetadata_ShouldAddOrUpdateValue()
    {
        // Arrange
        var user = CreateTestUser();
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            cardDetails);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        var key = "billing_zip";
        var value = "90210";

        // Act
        var updateResult = paymentMethod.UpdateMetadata(key, value);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.True(paymentMethod.Metadata.ContainsKey(key));
        Assert.Equal(value, paymentMethod.Metadata[key]);
    }

    [Fact]
    public void UpdateMetadata_WithEmptyKey_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            cardDetails);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        var emptyKey = string.Empty;
        var value = "test";

        // Act
        var updateResult = paymentMethod.UpdateMetadata(emptyKey, value);

        // Assert
        Assert.True(updateResult.IsFailure);
        Assert.Equal("PaymentMethod.InvalidMetadataKey", updateResult.Error.Code);
    }
}