using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Repositories.Base.Read;

namespace Shopilent.Domain.Catalog.Repositories.Read;

public interface IAttributeReadRepository : IAggregateReadRepository<AttributeDto>
{
    Task<AttributeDto> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttributeDto>> GetVariantAttributesAsync(CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}