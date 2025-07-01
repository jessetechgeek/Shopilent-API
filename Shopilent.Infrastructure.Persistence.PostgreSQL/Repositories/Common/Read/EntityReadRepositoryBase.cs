using Microsoft.Extensions.Logging;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Repositories.Read;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Read;

public abstract class EntityReadRepositoryBase<TEntity, TDto> : ReadRepositoryBase<TEntity, TDto>,
    IEntityReadRepository<TDto>
    where TEntity : Entity
    where TDto : class
{
    protected EntityReadRepositoryBase(IDapperConnectionFactory connectionFactory, ILogger logger)
        : base(connectionFactory, logger)
    {
    }
}