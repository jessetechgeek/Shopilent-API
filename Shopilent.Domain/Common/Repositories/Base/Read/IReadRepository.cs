namespace Shopilent.Domain.Common.Repositories.Base.Read;

public interface IReadRepository<T> where T : class
{
    Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default);
}