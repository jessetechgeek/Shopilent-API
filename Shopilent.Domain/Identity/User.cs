using Shopilent.Domain.Common;
using Shopilent.Domain.Identity.Enums;
using Shopilent.Domain.Identity.Events;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Shipping;
using Shopilent.Domain.Shipping.Enums;
using Shopilent.Domain.Shipping.ValueObjects;

namespace Shopilent.Domain.Identity;

public class User : AggregateRoot
{
    private User()
    {
        // Required by EF Core
    }

    private User(Email email, string passwordHash, FullName fullName, UserRole role = UserRole.Customer)
    {
        if (email == null)
            throw new ArgumentNullException(nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(fullName.FirstName))
            throw new ArgumentException("First name cannot be empty", nameof(fullName.FirstName));

        if (string.IsNullOrWhiteSpace(fullName.LastName))
            throw new ArgumentException("Last name cannot be empty", nameof(fullName.LastName));

        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        IsActive = true;
        EmailVerified = false;
        FailedLoginAttempts = 0;

        _addresses = new List<Address>();
        _refreshTokens = new List<RefreshToken>();
        _orders = new List<Order>();
    }

    public static User Create(Email email, string passwordHash, FullName fullName,
        UserRole role = UserRole.Customer)
    {
        var user = new User(email, passwordHash, fullName, role);
        user.AddDomainEvent(new UserCreatedEvent(user.Id));
        return user;
    }

    public static User CreatePreVerified(Email email, string passwordHash, FullName fullName,
        UserRole role = UserRole.Customer)
    {
        var user = Create(email, passwordHash, fullName, role);
        user.EmailVerified = true;
        return user;
    }

    public static User CreateAdmin(Email email, string passwordHash, FullName fullName)
    {
        return Create(email, passwordHash, fullName, UserRole.Admin);
    }

    public static User CreateManager(Email email, string passwordHash, FullName fullName)
    {
        return Create(email, passwordHash, fullName, UserRole.Manager);
    }


    public Email Email { get; private set; }
    public FullName FullName { get; private set; }
    public string PasswordHash { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLogin { get; private set; }
    public bool EmailVerified { get; private set; }
    public string EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationExpires { get; private set; }
    public string PasswordResetToken { get; private set; }
    public DateTime? PasswordResetExpires { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LastFailedAttempt { get; private set; }

    private readonly List<Address> _addresses = new();
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private readonly List<Cart> _carts = new();
    public IReadOnlyCollection<Cart> Carts => _carts.AsReadOnly();

    private readonly List<Order> _orders = new();
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    public void UpdatePersonalInfo(FullName fullName, PhoneNumber phone = null)
    {
        if (string.IsNullOrWhiteSpace(fullName.FirstName))
            throw new ArgumentException("First name cannot be empty", nameof(fullName.FirstName));

        if (string.IsNullOrWhiteSpace(fullName.LastName))
            throw new ArgumentException("Last name cannot be empty", nameof(fullName.LastName));

        FullName = fullName;
        Phone = phone;

        AddDomainEvent(new UserUpdatedEvent(Id));
    }

    public void UpdateEmail(Email email)
    {
        if (email == null)
            throw new ArgumentNullException(nameof(email));

        Email = email;
        EmailVerified = false;
        GenerateEmailVerificationToken();

        AddDomainEvent(new UserEmailChangedEvent(Id, email.Value));
    }

    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        PasswordHash = passwordHash;
        RevokeAllRefreshTokens("Password changed");

        AddDomainEvent(new UserPasswordChangedEvent(Id));
    }

    public void SetRole(UserRole role)
    {
        Role = role;
        AddDomainEvent(new UserRoleChangedEvent(Id, role));
    }

    public void RecordLoginSuccess()
    {
        LastLogin = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LastFailedAttempt = null;
    }

    public void RecordLoginFailure()
    {
        FailedLoginAttempts++;
        LastFailedAttempt = DateTime.UtcNow;

        if (FailedLoginAttempts >= 5)
        {
            IsActive = false;
            AddDomainEvent(new UserLockedOutEvent(Id));
        }
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        FailedLoginAttempts = 0;
        LastFailedAttempt = null;

        AddDomainEvent(new UserStatusChangedEvent(Id, true));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        RevokeAllRefreshTokens("User deactivated");

        AddDomainEvent(new UserStatusChangedEvent(Id, false));
    }

    public void VerifyEmail()
    {
        EmailVerified = true;
        EmailVerificationToken = null;
        EmailVerificationExpires = null;

        AddDomainEvent(new UserEmailVerifiedEvent(Id));
    }

    public void GenerateEmailVerificationToken()
    {
        EmailVerificationToken = Guid.NewGuid().ToString("N");
        EmailVerificationExpires = DateTime.UtcNow.AddDays(1);
    }

    public void GeneratePasswordResetToken()
    {
        PasswordResetToken = Guid.NewGuid().ToString("N");
        PasswordResetExpires = DateTime.UtcNow.AddHours(1);
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetExpires = null;
    }

    public RefreshToken AddRefreshToken(string token, DateTime expiresAt, string ipAddress = null,
        string userAgent = null)
    {
        var refreshToken = RefreshToken.Create(this, token, expiresAt, ipAddress, userAgent);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public void RevokeRefreshToken(string token, string reason)
    {
        var refreshToken = _refreshTokens.Find(rt => rt.Token == token && !rt.IsRevoked);
        if (refreshToken != null)
        {
            refreshToken.Revoke(reason);
        }
    }

    public void RevokeAllRefreshTokens(string reason)
    {
        foreach (var token in _refreshTokens.FindAll(rt => !rt.IsRevoked))
        {
            token.Revoke(reason);
        }
    }

    public Address AddAddress(
        PostalAddress postalAddress,
        AddressType addressType = AddressType.Shipping,
        PhoneNumber phone = null,
        bool isDefault = false)
    {
        var address = Address.Create(
            this,
            postalAddress,
            addressType,
            phone,
            isDefault);

        if (isDefault)
        {
            // Update other addresses of the same type
            foreach (var existingAddress in _addresses.FindAll(a => a.AddressType == addressType && a.IsDefault))
            {
                existingAddress.SetDefault(false);
            }
        }

        _addresses.Add(address);
        return address;
    }

    public void RemoveAddress(Guid addressId)
    {
        var address = _addresses.Find(a => a.Id == addressId);
        if (address != null)
        {
            _addresses.Remove(address);
        }
    }
}