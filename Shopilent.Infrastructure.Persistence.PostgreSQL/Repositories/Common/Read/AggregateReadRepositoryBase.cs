using Microsoft.Extensions.Logging;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Repositories.Read;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Read;

public abstract class AggregateReadRepositoryBase<TAggregate, TDto> : ReadRepositoryBase<TAggregate, TDto>, IAggregateReadRepository<TDto>
    where TAggregate : AggregateRoot
    where TDto : class
{
    protected AggregateReadRepositoryBase(IDapperConnectionFactory connectionFactory, ILogger logger)
        : base(connectionFactory, logger)
    {
    }
}