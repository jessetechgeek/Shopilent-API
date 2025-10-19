using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Models;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.Application.Features.Identity.Queries.GetUsersDatatable.V1;

internal sealed class GetUsersDatatableQueryHandlerV1 :
    IQueryHandler<GetUsersDatatableQueryV1, DataTableResult<UserDatatableDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUsersDatatableQueryHandlerV1> _logger;

    public GetUsersDatatableQueryHandlerV1(
        IUnitOfWork unitOfWork,
        ILogger<GetUsersDatatableQueryHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<DataTableResult<UserDatatableDto>>> Handle(
        GetUsersDatatableQueryV1 request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get datatable results from repository
            var result = await _unitOfWork.UserReader.GetDataTableAsync(
                request.Request,
                cancellationToken);

            // Map to UserDatatableDto
            var dtoItems = new List<UserDatatableDto>();
            foreach (var user in result.Data)
            {
                // Get user addresses count
                var addresses = await _unitOfWork.AddressReader.GetByUserIdAsync(
                    user.Id,
                    cancellationToken);

                var addressCount = addresses?.Count ?? 0;

                dtoItems.Add(new UserDatatableDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    Phone = user.Phone,
                    Role = user.Role,
                    RoleName = GetRoleName(user.Role),
                    IsActive = user.IsActive,
                    IsEmailVerified = user.EmailVerified,
                    LastLoginAt = user.LastLogin,
                    AddressCount = addressCount,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                });
            }

            // Create new datatable result with mapped DTOs
            var datatableResult = new DataTableResult<UserDatatableDto>(
                result.Draw,
                result.RecordsTotal,
                result.RecordsFiltered,
                dtoItems);

            _logger.LogInformation("Retrieved {Count} users for datatable", dtoItems.Count);
            return Result.Success(datatableResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for datatable");

            return Result.Failure<DataTableResult<UserDatatableDto>>(
                Error.Failure(
                    code: "Users.GetDataTableFailed",
                    message: $"Failed to retrieve users: {ex.Message}"));
        }
    }

    private static string GetRoleName(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => "Admin",
            UserRole.Manager => "Manager",
            UserRole.Customer => "Customer",
            _ => "Unknown"
        };
    }
}