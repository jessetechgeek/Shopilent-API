using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.DTOs;
using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Application.Abstractions.Identity;

public interface IAuthenticationService
{
    Task<Result<AuthTokenResponse>> LoginAsync(
        Domain.Identity.ValueObjects.Email email,
        string password,
        string ipAddress = null,
        string userAgent = null,
        CancellationToken cancellationToken = default);

    Task<Result<AuthTokenResponse>> RegisterAsync(
        Domain.Identity.ValueObjects.Email email,
        string password,
        string firstName,
        string lastName,
        string phone = null,
        string ipAddress = null,
        string userAgent = null,
        CancellationToken cancellationToken = default);

    Task<Result<AuthTokenResponse>> RefreshTokenAsync(
        string refreshToken,
        string ipAddress = null,
        string userAgent = null,
        CancellationToken cancellationToken = default);

    Task<Result> RevokeTokenAsync(
        string refreshToken,
        string reason = "User logged out",
        CancellationToken cancellationToken = default);

    Task<Result<UserDto>> ValidateTokenAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<Result> SendEmailVerificationAsync(
        Domain.Identity.ValueObjects.Email email,
        CancellationToken cancellationToken = default);

    Task<Result> VerifyEmailAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<Result> RequestPasswordResetAsync(
        Domain.Identity.ValueObjects.Email email,
        CancellationToken cancellationToken = default);

    Task<Result> ResetPasswordAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);
}