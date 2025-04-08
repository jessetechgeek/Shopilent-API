using FluentValidation;
using MediatR;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

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
            // Check if response type is Result<T>
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var validationMetadata = failures.GroupBy(f => f.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(f => f.ErrorMessage).ToArray());

                var error = Error.Validation(
                    code: "Validation.Failed",
                    message: failures.First().ErrorMessage,
                    metadata: validationMetadata);

                // Create a failure result with validation errors
                var resultType = typeof(Result);
                // Be more specific about which method we want to get
                var failureMethod = resultType.GetMethods()
                    .Where(m => m.Name == "Failure" && m.IsGenericMethod && m.GetParameters().Length == 1 &&
                                m.GetParameters()[0].ParameterType == typeof(Error))
                    .First()
                    .MakeGenericMethod(typeof(TResponse).GetGenericArguments()[0]);

                return (TResponse)failureMethod.Invoke(null, new object[] { error });
            }

            // For other types, just throw validation exception
            throw new ValidationException(failures);
        }

        return await next();
    }
}