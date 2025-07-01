using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Catalog.Repositories.Write;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Write;
using Attribute = Shopilent.Domain.Catalog.Attribute;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Catalog.Write;

public class AttributeWriteRepository : AggregateWriteRepositoryBase<Attribute>, IAttributeWriteRepository
{
    public AttributeWriteRepository(ApplicationDbContext dbContext, ILogger<AttributeWriteRepository> logger)
        : base(dbContext, logger)
    {
    }

    public async Task<Attribute> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Attributes
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Attribute> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbContext.Attributes
            .FirstOrDefaultAsync(a => a.Name == name, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.Attributes.Where(a => a.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}