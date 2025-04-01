using Shopilent.Domain.Common.Repositories.Write;

namespace Shopilent.Domain.Catalog.Repositories.Write;

public interface IAttributeWriteRepository : IAggregateWriteRepository<Attribute>
{
    // EF Core will be used for reads in write repository as well
    Task<Attribute> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Attribute> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}