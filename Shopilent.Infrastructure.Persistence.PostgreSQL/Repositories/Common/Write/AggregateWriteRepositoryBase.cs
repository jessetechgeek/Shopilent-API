using Microsoft.Extensions.Logging;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Repositories.Write;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Write;

public abstract class AggregateWriteRepositoryBase<T> : WriteRepositoryBase<T>, IAggregateWriteRepository<T>
    where T : AggregateRoot
{
    protected AggregateWriteRepositoryBase(ApplicationDbContext dbContext, ILogger logger)
        : base(dbContext, logger)
    {
    }
}