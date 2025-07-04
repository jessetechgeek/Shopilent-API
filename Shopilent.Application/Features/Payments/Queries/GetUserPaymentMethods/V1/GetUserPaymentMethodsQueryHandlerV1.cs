using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Payments.DTOs;

namespace Shopilent.Application.Features.Payments.Queries.GetUserPaymentMethods.V1;

internal sealed class
    GetUserPaymentMethodsQueryHandlerV1 : IQueryHandler<GetUserPaymentMethodsQueryV1, IReadOnlyList<PaymentMethodDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserPaymentMethodsQueryHandlerV1> _logger;

    public GetUserPaymentMethodsQueryHandlerV1(
        IUnitOfWork unitOfWork,
        ILogger<GetUserPaymentMethodsQueryHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<PaymentMethodDto>>> Handle(
        GetUserPaymentMethodsQueryV1 request,
        CancellationToken cancellationToken)
    {
        try
        {
            var paymentMethods = await _unitOfWork.PaymentMethodReader
                .GetByUserIdAsync(request.UserId, cancellationToken);

            _logger.LogInformation("Retrieved {Count} payment methods for user {UserId}",
                paymentMethods.Count, request.UserId);

            return Result.Success(paymentMethods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment methods for user {UserId}", request.UserId);

            return Result.Failure<IReadOnlyList<PaymentMethodDto>>(
                Error.Failure(
                    code: "PaymentMethods.GetUserPaymentMethodsFailed",
                    message: $"Failed to retrieve payment methods: {ex.Message}"));
        }
    }
}