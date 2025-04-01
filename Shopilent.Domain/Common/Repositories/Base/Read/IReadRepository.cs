namespace Shopilent.Domain.Common.Repositories.Base.Read;

public interface IReadRepository<TDto> where TDto : class
{
    Task<TDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TDto>> ListAllAsync(CancellationToken cancellationToken = default);
}