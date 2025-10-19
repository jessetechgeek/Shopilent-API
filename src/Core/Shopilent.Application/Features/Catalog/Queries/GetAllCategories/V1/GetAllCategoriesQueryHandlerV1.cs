using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Catalog.Queries.GetAllCategories.V1;

internal sealed class GetAllCategoriesQueryHandlerV1 : IQueryHandler<GetAllCategoriesQueryV1, IReadOnlyList<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllCategoriesQueryHandlerV1> _logger;

    public GetAllCategoriesQueryHandlerV1(
        IUnitOfWork unitOfWork,
        ILogger<GetAllCategoriesQueryHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(
        GetAllCategoriesQueryV1 request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _unitOfWork.CategoryReader.ListAllAsync(cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} categories", categories.Count);
            return Result.Success(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all categories");
            
            return Result.Failure<IReadOnlyList<CategoryDto>>(
                Error.Failure(
                    code: "Categories.GetAllFailed",
                    message: $"Failed to retrieve categories: {ex.Message}"));
        }
    }
}