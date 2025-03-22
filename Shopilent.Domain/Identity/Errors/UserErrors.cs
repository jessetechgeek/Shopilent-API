using Shopilent.Domain.Common.Errors;

namespace Shopilent.Domain.Identity.Errors;

public static class UserErrors
{
    public static Error EmailRequired => Error.Validation(
        code: "User.EmailRequired",
        message: "Email cannot be empty.");

    public static Error InvalidEmailFormat => Error.Validation(
        code: "User.InvalidEmailFormat",
        message: "Invalid email format.");

    public static Error PasswordRequired => Error.Validation(
        code: "User.PasswordRequired",
        message: "Password hash cannot be empty.");

    public static Error FirstNameRequired => Error.Validation(
        code: "User.FirstNameRequired",
        message: "First name cannot be empty.");

    public static Error LastNameRequired => Error.Validation(
        code: "User.LastNameRequired",
        message: "Last name cannot be empty.");

    public static Error EmailAlreadyExists(string email) => Error.Conflict(
        code: "User.EmailAlreadyExists",
        message: $"A user with email '{email}' already exists.");

    public static Error NotFound(Guid id) => Error.NotFound(
        code: "User.NotFound",
        message: $"User with ID {id} was not found.");

    public static Error AccountLocked => Error.Unauthorized(
        code: "User.AccountLocked",
        message: "Account is locked due to too many failed login attempts.");

    public static Error AccountInactive => Error.Unauthorized(
        code: "User.AccountInactive",
        message: "Account is not active.");

    public static Error EmailNotVerified => Error.Unauthorized(
        code: "User.EmailNotVerified",
        message: "Email is not verified.");

    public static Error InvalidCredentials => Error.Unauthorized(
        code: "User.InvalidCredentials",
        message: "Invalid login credentials.");
    
    public static Error PhoneRequired => Error.Validation(
        code: "User.PhoneRequired",
        message: "Phone number cannot be empty.");

    public static Error InvalidPhoneFormat => Error.Validation(
        code: "User.InvalidPhoneFormat",
        message: "Invalid phone number format.");
}