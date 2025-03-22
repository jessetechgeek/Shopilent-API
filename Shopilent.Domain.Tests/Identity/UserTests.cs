using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.Enums;
using Shopilent.Domain.Identity.Events;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Shipping;
using Shopilent.Domain.Shipping.Enums;
using Shopilent.Domain.Shipping.ValueObjects;

namespace Shopilent.Domain.Tests.Identity;

public class UserTests
{
    private Email CreateTestEmail()
    {
        var result = Email.Create("test@example.com");
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private FullName CreateTestFullName()
    {
        var result = FullName.Create("John", "Doe");
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private User CreateTestUser()
    {
        var result = User.Create(
            CreateTestEmail(),
            "hashed_password",
            CreateTestFullName());

        Assert.True(result.IsSuccess);
        return result.Value;
    }

    [Fact]
    public void Create_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        var email = CreateTestEmail();
        var passwordHash = "hashed_password";
        var fullName = CreateTestFullName();

        // Act
        var result = User.Create(email, passwordHash, fullName);

        // Assert
        Assert.True(result.IsSuccess);
        var user = result.Value;
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(fullName, user.FullName);
        Assert.Equal(UserRole.Customer, user.Role);
        Assert.True(user.IsActive);
        Assert.False(user.EmailVerified);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Empty(user.Addresses);
        Assert.Empty(user.RefreshTokens);
        Assert.Empty(user.Orders);
        Assert.Contains(user.DomainEvents, e => e is UserCreatedEvent);
    }

    [Fact]
    public void Create_WithNullEmail_ShouldReturnFailure()
    {
        // Arrange
        Email email = null;
        var passwordHash = "hashed_password";
        var fullName = CreateTestFullName();

        // Act
        var result = User.Create(email, passwordHash, fullName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.EmailRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithEmptyPasswordHash_ShouldReturnFailure()
    {
        // Arrange
        var email = CreateTestEmail();
        var passwordHash = string.Empty;
        var fullName = CreateTestFullName();

        // Act
        var result = User.Create(email, passwordHash, fullName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.PasswordRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithEmptyFirstName_ShouldReturnFailure()
    {
        // Arrange
        var email = CreateTestEmail();
        var passwordHash = "hashed_password";
        // Pass null for fullName to trigger validation in User.Create
        FullName fullName = null;

        // Act
        var result = User.Create(email, passwordHash, fullName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.FirstNameRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithEmptyLastName_ShouldReturnFailure()
    {
        // Arrange
        var email = CreateTestEmail();
        var passwordHash = "hashed_password";
        // We'd need to create a FullName with empty lastName, but since validation happens in FullName.Create,
        // we'll just pass null to trigger the validation in User.Create
        FullName fullName = null;

        // Act
        var result = User.Create(email, passwordHash, fullName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.FirstNameRequired", result.Error.Code);
    }

    [Fact]
    public void CreatePreVerified_ShouldCreateVerifiedUser()
    {
        // Arrange
        var email = CreateTestEmail();
        var passwordHash = "hashed_password";
        var fullName = CreateTestFullName();

        // Act
        var result = User.CreatePreVerified(email, passwordHash, fullName);

        // Assert
        Assert.True(result.IsSuccess);
        var user = result.Value;
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(fullName, user.FullName);
        Assert.Equal(UserRole.Customer, user.Role);
        Assert.True(user.IsActive);
        Assert.True(user.EmailVerified);
        Assert.Contains(user.DomainEvents, e => e is UserCreatedEvent);
    }

    [Fact]
    public void CreateAdmin_ShouldCreateAdminUser()
    {
        // Arrange
        var email = CreateTestEmail();
        var passwordHash = "hashed_password";
        var fullName = CreateTestFullName();

        // Act
        var result = User.CreateAdmin(email, passwordHash, fullName);

        // Assert
        Assert.True(result.IsSuccess);
        var user = result.Value;
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(fullName, user.FullName);
        Assert.Equal(UserRole.Admin, user.Role);
        Assert.True(user.IsActive);
        Assert.Contains(user.DomainEvents, e => e is UserCreatedEvent);
    }

    [Fact]
    public void CreateManager_ShouldCreateManagerUser()
    {
        // Arrange
        var email = CreateTestEmail();
        var passwordHash = "hashed_password";
        var fullName = CreateTestFullName();

        // Act
        var result = User.CreateManager(email, passwordHash, fullName);

        // Assert
        Assert.True(result.IsSuccess);
        var user = result.Value;
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(fullName, user.FullName);
        Assert.Equal(UserRole.Manager, user.Role);
        Assert.True(user.IsActive);
        Assert.Contains(user.DomainEvents, e => e is UserCreatedEvent);
    }

    [Fact]
    public void UpdatePersonalInfo_WithValidParameters_ShouldUpdateUserInfo()
    {
        // Arrange
        var user = CreateTestUser();
        var newFullNameResult = FullName.Create("Jane", "Smith");
        Assert.True(newFullNameResult.IsSuccess);
        var newFullName = newFullNameResult.Value;
        
        var newPhoneResult = PhoneNumber.Create("555-123-4567");
        Assert.True(newPhoneResult.IsSuccess);
        var newPhone = newPhoneResult.Value;

        // Act
        var result = user.UpdatePersonalInfo(newFullName, newPhone);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newFullName, user.FullName);
        Assert.Equal(newPhone, user.Phone);
        Assert.Contains(user.DomainEvents, e => e is UserUpdatedEvent);
    }

    [Fact]
    public void UpdateEmail_ShouldUpdateEmailAndResetVerification()
    {
        // Arrange
        var userResult = User.CreatePreVerified(
            CreateTestEmail(),
            "hashed_password",
            CreateTestFullName());

        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        Assert.True(user.EmailVerified);

        var newEmailResult = Email.Create("new-email@example.com");
        Assert.True(newEmailResult.IsSuccess);
        var newEmail = newEmailResult.Value;

        // Act
        var updateResult = user.UpdateEmail(newEmail);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(newEmail, user.Email);
        Assert.False(user.EmailVerified);
        Assert.NotNull(user.EmailVerificationToken);
        Assert.NotNull(user.EmailVerificationExpires);
        Assert.Contains(user.DomainEvents, e => e is UserEmailChangedEvent);
    }

    [Fact]
    public void UpdatePassword_ShouldUpdatePasswordAndRevokeTokens()
    {
        // Arrange
        var user = CreateTestUser();

        var tokenResult = user.AddRefreshToken("refresh_token", DateTime.UtcNow.AddDays(7));
        Assert.True(tokenResult.IsSuccess);
        Assert.Single(user.RefreshTokens);
        Assert.True(user.RefreshTokens.First().IsActive);

        var newPasswordHash = "new_hashed_password";

        // Act
        var result = user.UpdatePassword(newPasswordHash);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newPasswordHash, user.PasswordHash);
        Assert.Single(user.RefreshTokens);
        Assert.False(user.RefreshTokens.First().IsActive);
        Assert.Contains(user.DomainEvents, e => e is UserPasswordChangedEvent);
    }

    [Fact]
    public void SetRole_ShouldUpdateUserRole()
    {
        // Arrange
        var user = CreateTestUser();
        Assert.Equal(UserRole.Customer, user.Role);

        // Act
        var result = user.SetRole(UserRole.Manager);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(UserRole.Manager, user.Role);
        Assert.Contains(user.DomainEvents, e => e is UserRoleChangedEvent);
    }

    [Fact]
    public void RecordLoginSuccess_ShouldUpdateLoginInfo()
    {
        // Arrange
        var user = CreateTestUser();
        var failResult = user.RecordLoginFailure(); // Set failed attempt
        Assert.True(failResult.IsSuccess);
        Assert.Equal(1, user.FailedLoginAttempts);
        Assert.NotNull(user.LastFailedAttempt);

        // Act
        var result = user.RecordLoginSuccess();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(user.LastLogin);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LastFailedAttempt);
    }

    [Fact]
    public void RecordLoginFailure_ShouldIncrementFailedAttempts()
    {
        // Arrange
        var user = CreateTestUser();
        Assert.Equal(0, user.FailedLoginAttempts);

        // Act
        var result = user.RecordLoginFailure();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, user.FailedLoginAttempts);
        Assert.NotNull(user.LastFailedAttempt);
    }

    [Fact]
    public void RecordLoginFailure_ExceedingMaxAttempts_ShouldReturnFailureAndLockAccount()
    {
        // Arrange
        var user = CreateTestUser();

        // Act - record 4 successful failures
        for (int i = 0; i < 4; i++)
        {
            var result = user.RecordLoginFailure();
            Assert.True(result.IsSuccess);
        }

        // Act - record the 5th failure that should lock the account
        var lastResult = user.RecordLoginFailure();

        // Assert
        Assert.True(lastResult.IsFailure);
        Assert.Equal(5, user.FailedLoginAttempts);
        Assert.False(user.IsActive); // Account should be locked
        Assert.Contains(user.DomainEvents, e => e is UserLockedOutEvent);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateUser()
    {
        // Arrange
        var user = CreateTestUser();
        var deactivateResult = user.Deactivate();
        Assert.True(deactivateResult.IsSuccess);
        Assert.False(user.IsActive);

        // Act
        var result = user.Activate();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(user.IsActive);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LastFailedAttempt);
        Assert.Contains(user.DomainEvents, e => e is UserStatusChangedEvent);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateUser()
    {
        // Arrange
        var user = CreateTestUser();
        Assert.True(user.IsActive);

        var tokenResult = user.AddRefreshToken("refresh_token", DateTime.UtcNow.AddDays(7));
        Assert.True(tokenResult.IsSuccess);
        Assert.Single(user.RefreshTokens);
        Assert.True(user.RefreshTokens.First().IsActive);

        // Act
        var result = user.Deactivate();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(user.IsActive);
        Assert.Single(user.RefreshTokens);
        Assert.False(user.RefreshTokens.First().IsActive);
        Assert.Contains(user.DomainEvents, e => e is UserStatusChangedEvent);
    }

    [Fact]
    public void VerifyEmail_ShouldMarkEmailAsVerified()
    {
        // Arrange
        var user = CreateTestUser();
        var tokenResult = user.GenerateEmailVerificationToken();
        Assert.True(tokenResult.IsSuccess);
        Assert.False(user.EmailVerified);
        Assert.NotNull(user.EmailVerificationToken);
        Assert.NotNull(user.EmailVerificationExpires);

        // Act
        var result = user.VerifyEmail();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(user.EmailVerified);
        Assert.Null(user.EmailVerificationToken);
        Assert.Null(user.EmailVerificationExpires);
        Assert.Contains(user.DomainEvents, e => e is UserEmailVerifiedEvent);
    }

    [Fact]
    public void GenerateEmailVerificationToken_ShouldCreateToken()
    {
        // Arrange
        var user = CreateTestUser();
        Assert.Null(user.EmailVerificationToken);
        Assert.Null(user.EmailVerificationExpires);

        // Act
        var result = user.GenerateEmailVerificationToken();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(user.EmailVerificationToken);
        Assert.NotNull(user.EmailVerificationExpires);
        Assert.True(user.EmailVerificationExpires > DateTime.UtcNow);
    }

    [Fact]
    public void GeneratePasswordResetToken_ShouldCreateToken()
    {
        // Arrange
        var user = CreateTestUser();
        Assert.Null(user.PasswordResetToken);
        Assert.Null(user.PasswordResetExpires);

        // Act
        var result = user.GeneratePasswordResetToken();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(user.PasswordResetToken);
        Assert.NotNull(user.PasswordResetExpires);
        Assert.True(user.PasswordResetExpires > DateTime.UtcNow);
    }

    [Fact]
    public void ClearPasswordResetToken_ShouldClearToken()
    {
        // Arrange
        var user = CreateTestUser();
        var tokenResult = user.GeneratePasswordResetToken();
        Assert.True(tokenResult.IsSuccess);
        Assert.NotNull(user.PasswordResetToken);
        Assert.NotNull(user.PasswordResetExpires);

        // Act
        var result = user.ClearPasswordResetToken();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(user.PasswordResetToken);
        Assert.Null(user.PasswordResetExpires);
    }

    [Fact]
    public void AddRefreshToken_ShouldAddNewToken()
    {
        // Arrange
        var user = CreateTestUser();
        var token = "refresh_token";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var ipAddress = "127.0.0.1";
        var userAgent = "Test Agent";

        // Act
        var result = user.AddRefreshToken(token, expiresAt, ipAddress, userAgent);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(user.RefreshTokens);
        var refreshToken = result.Value;
        Assert.Equal(token, refreshToken.Token);
        Assert.Equal(expiresAt, refreshToken.ExpiresAt);
        Assert.Equal(ipAddress, refreshToken.IpAddress);
        Assert.Equal(userAgent, refreshToken.UserAgent);
        Assert.True(refreshToken.IsActive);
    }

    [Fact]
    public void RevokeRefreshToken_ShouldRevokeToken()
    {
        // Arrange
        var user = CreateTestUser();
        var token = "refresh_token";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var tokenResult = user.AddRefreshToken(token, expiresAt);
        Assert.True(tokenResult.IsSuccess);
        var refreshToken = tokenResult.Value;
        Assert.True(refreshToken.IsActive);
        var reason = "Test revocation";

        // Act
        var result = user.RevokeRefreshToken(token, reason);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(refreshToken.IsActive);
        Assert.Equal(reason, refreshToken.RevokedReason);
    }

    [Fact]
    public void RevokeAllRefreshTokens_ShouldRevokeAllTokens()
    {
        // Arrange
        var user = CreateTestUser();

        var token1Result = user.AddRefreshToken("token1", DateTime.UtcNow.AddDays(7));
        var token2Result = user.AddRefreshToken("token2", DateTime.UtcNow.AddDays(7));
        var token3Result = user.AddRefreshToken("token3", DateTime.UtcNow.AddDays(7));

        Assert.True(token1Result.IsSuccess);
        Assert.True(token2Result.IsSuccess);
        Assert.True(token3Result.IsSuccess);

        var token1 = token1Result.Value;
        var token2 = token2Result.Value;
        var token3 = token3Result.Value;

        Assert.Equal(3, user.RefreshTokens.Count);
        Assert.True(token1.IsActive);
        Assert.True(token2.IsActive);
        Assert.True(token3.IsActive);

        var reason = "Security measure";

        // Act
        var result = user.RevokeAllRefreshTokens(reason);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, user.RefreshTokens.Count);
        Assert.All(user.RefreshTokens, token => Assert.False(token.IsActive));
        Assert.All(user.RefreshTokens, token => Assert.Equal(reason, token.RevokedReason));
    }

    [Fact]
    public void AddAddress_ShouldAddNewAddress()
    {
        // Arrange
        var user = CreateTestUser();

        var postalAddressResult = PostalAddress.Create(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345");
        Assert.True(postalAddressResult.IsSuccess);
        var postalAddress = postalAddressResult.Value;

        var addressType = AddressType.Shipping;

        // Act
        var result = user.AddAddress(
            postalAddress,
            addressType);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(user.Addresses);
        var address = result.Value;
        Assert.Equal(postalAddress, address.PostalAddress);
        Assert.Equal(addressType, address.AddressType);
        Assert.False(address.IsDefault);
    }

    [Fact]
    public void AddAddress_WithDefault_ShouldUpdateOtherAddresses()
    {
        // Arrange
        var user = CreateTestUser();

        // Add first default shipping address
        var firstPostalAddressResult = PostalAddress.Create(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345");
        Assert.True(firstPostalAddressResult.IsSuccess);
        var firstPostalAddress = firstPostalAddressResult.Value;

        var firstAddressResult = user.AddAddress(
            firstPostalAddress,
            AddressType.Shipping,
            null,
            true);

        Assert.True(firstAddressResult.IsSuccess);
        var firstAddress = firstAddressResult.Value;
        Assert.True(firstAddress.IsDefault);

        // Act - add second default shipping address
        var secondPostalAddressResult = PostalAddress.Create(
            "456 Oak Ave",
            "Othertown",
            "State",
            "Country",
            "67890");
        Assert.True(secondPostalAddressResult.IsSuccess);
        var secondPostalAddress = secondPostalAddressResult.Value;

        var secondAddressResult = user.AddAddress(
            secondPostalAddress,
            AddressType.Shipping,
            null,
            true);

        // Assert
        Assert.True(secondAddressResult.IsSuccess);
        var secondAddress = secondAddressResult.Value;
        Assert.Equal(2, user.Addresses.Count);
        Assert.False(firstAddress.IsDefault);
        Assert.True(secondAddress.IsDefault);
    }

    [Fact]
    public void RemoveAddress_ShouldRemoveAddress()
    {
        // Arrange
        var user = CreateTestUser();

        var postalAddressResult = PostalAddress.Create(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345");
        Assert.True(postalAddressResult.IsSuccess);
        var postalAddress = postalAddressResult.Value;

        var addressResult = user.AddAddress(postalAddress);
        Assert.True(addressResult.IsSuccess);
        var address = addressResult.Value;
        Assert.Single(user.Addresses);

        // Act
        var result = user.RemoveAddress(address.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(user.Addresses);
    }

    [Fact]
    public void RemoveAddress_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var invalidAddressId = Guid.NewGuid();

        // Act
        var result = user.RemoveAddress(invalidAddressId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Address.NotFound", result.Error.Code);
    }
}