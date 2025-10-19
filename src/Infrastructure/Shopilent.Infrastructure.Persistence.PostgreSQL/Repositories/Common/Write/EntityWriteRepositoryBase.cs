using Microsoft.Extensions.Logging;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Repositories.Write;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Write;

public abstract class EntityWriteRepositoryBase<T> : WriteRepositoryBase<T>, IEntityWriteRepository<T> where T : Entity
{
    protected EntityWriteRepositoryBase(ApplicationDbContext dbContext, ILogger logger)
        : base(dbContext, logger)
    {
    }
}