using FluentValidation;
using MediatR;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.Errors;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentValidation.Results;

namespace Shopilent.Application.Abstractions.Behaviors;

public class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            // Group validation errors by property name
            var validationMetadata = failures.GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).ToArray());

            // Try to map to domain error
            var error = MapToDomainError(failures.First(), validationMetadata);

            // Check if response type is Result<T>
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                // Create a failure result with validation errors for Result<T>
                var resultType = typeof(Result);
                var failureMethod = resultType.GetMethods()
                    .Where(m => m.Name == "Failure" && m.IsGenericMethod && m.GetParameters().Length == 1 &&
                                m.GetParameters()[0].ParameterType == typeof(Error))
                    .First()
                    .MakeGenericMethod(typeof(TResponse).GetGenericArguments()[0]);

                return (TResponse)failureMethod.Invoke(null, new object[] { error });
            }
            // Check if response type is Result (non-generic)
            else if (typeof(TResponse) == typeof(Result))
            {
                // Create a failure result for non-generic Result
                var failureResult = Result.Failure(error);
                return (TResponse)(object)failureResult;
            }

            // For other response types, throw validation exception
            throw new ValidationException(failures);
        }

        return await next();
    }

    private Error MapToDomainError(ValidationFailure failure, Dictionary<string, string[]> metadata)
    {
        // Determine the right domain error based on property name and error message
        string propertyName = failure.PropertyName.ToLowerInvariant();
        string errorMessage = failure.ErrorMessage.ToLowerInvariant();

        // Map to domain-specific errors
        if (propertyName == "email")
        {
            if (errorMessage.Contains("email format"))
                return UserErrors.InvalidEmailFormat;
            if (errorMessage.Contains("email is required"))
                return UserErrors.EmailRequired;
        }

        if (propertyName == "firstname" && errorMessage.Contains("required"))
            return UserErrors.FirstNameRequired;

        if (propertyName == "lastname" && errorMessage.Contains("required"))
            return UserErrors.LastNameRequired;

        if (propertyName == "phone" && errorMessage.Contains("valid"))
            return UserErrors.InvalidPhoneFormat;

        // Handle RefreshToken related validations
        if (propertyName == "refreshtoken")
        {
            if (errorMessage.Contains("required") || errorMessage.Contains("empty"))
                return RefreshTokenErrors.EmptyToken;
            if (errorMessage.Contains("expired"))
                return RefreshTokenErrors.Expired;
            if (errorMessage.Contains("revoked"))
                return RefreshTokenErrors.Revoked("Token was revoked");
            if (errorMessage.Contains("invalid"))
                return RefreshTokenErrors.InvalidToken;
        }

        // Handle various password validation scenarios
        if (propertyName == "password" || propertyName == "currentpassword" ||
            propertyName == "newpassword" || propertyName == "confirmpassword")
        {
            if (errorMessage.Contains("required"))
                return UserErrors.PasswordRequired;
            if (errorMessage.Contains("at least 8 characters"))
                return UserErrors.PasswordTooShort;
            if (errorMessage.Contains("match") || errorMessage.Contains("mismatch"))
                return Error.Validation(
                    code: "User.PasswordMismatch",
                    message: "Passwords do not match");
            // Add other password validations as needed
        }

        // Default to generic validation error
        return Error.Validation(
            code: "Validation.Failed",
            message: failure.ErrorMessage,
            metadata: metadata);
    }
}