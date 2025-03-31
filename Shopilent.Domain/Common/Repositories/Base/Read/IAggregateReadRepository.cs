namespace Shopilent.Domain.Common.Repositories.Base.Read;

public interface IAggregateReadRepository<T> : IReadRepository<T> where T : AggregateRoot
{
    // Read repository specialized for AggregateRoot objects
}