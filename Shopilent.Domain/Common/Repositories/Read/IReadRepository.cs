using Shopilent.Domain.Common.Models;

namespace Shopilent.Domain.Common.Repositories.Read;

public interface IReadRepository<TDto> where TDto : class
{
    Task<TDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TDto>> ListAllAsync(CancellationToken cancellationToken = default);
    
    Task<PaginatedResult<TDto>> GetPaginatedAsync(
        int pageNumber, 
        int pageSize, 
        string sortColumn = null, 
        bool sortDescending = false,
        CancellationToken cancellationToken = default);
        
    Task<DataTableResult<TDto>> GetDataTableAsync(
        DataTableRequest request,
        CancellationToken cancellationToken = default);
}