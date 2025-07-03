using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Catalog.Queries.GetRootCategories.V1;

internal sealed class GetRootCategoriesQueryHandlerV1 : IQueryHandler<GetRootCategoriesQueryV1, IReadOnlyList<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetRootCategoriesQueryHandlerV1> _logger;

    public GetRootCategoriesQueryHandlerV1(
        IUnitOfWork unitOfWork,
        ILogger<GetRootCategoriesQueryHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(
        GetRootCategoriesQueryV1 request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var rootCategories = await _unitOfWork.CategoryReader.GetRootCategoriesAsync(cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} root categories", rootCategories.Count);
            return Result.Success(rootCategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving root categories");
            
            return Result.Failure<IReadOnlyList<CategoryDto>>(
                Error.Failure(
                    code: "Categories.GetRootCategoriesFailed",
                    message: $"Failed to retrieve root categories: {ex.Message}"));
        }
    }
}