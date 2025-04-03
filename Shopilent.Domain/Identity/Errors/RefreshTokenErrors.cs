using Shopilent.Domain.Common.Errors;

namespace Shopilent.Domain.Identity.Errors;

public static class RefreshTokenErrors
{
    public static Error NotFound(string token) => Error.NotFound(
        code: "RefreshToken.NotFound",
        message: "The refresh token was not found.");

    public static Error Expired => Error.Unauthorized(
        code: "RefreshToken.Expired",
        message: "The refresh token has expired.");

    public static Error Revoked(string reason) => Error.Unauthorized(
        code: "RefreshToken.Revoked",
        message: $"The refresh token has been revoked. Reason: {reason}");

    public static Error InvalidToken => Error.Unauthorized(
        code: "RefreshToken.Invalid",
        message: "The refresh token is invalid.");

    public static Error UserMismatch => Error.Unauthorized(
        code: "RefreshToken.UserMismatch",
        message: "The refresh token does not belong to the specified user.");

    public static Error EmptyToken => Error.Validation(
        code: "RefreshToken.EmptyToken",
        message: "The refresh token cannot be empty.");

    public static Error InvalidExpiry => Error.Validation(
        code: "RefreshToken.InvalidExpiry",
        message: "The token expiry date must be in the future.");

    public static Error AlreadyRevoked => Error.Validation(
        code: "RefreshToken.AlreadyRevoked",
        message: "The refresh token has already been revoked.");
}