namespace Shopilent.Domain.Common.Repositories.Write;

public interface IAggregateWriteRepository<T> : IWriteRepository<T> where T : AggregateRoot
{
    // Write repository specialized for AggregateRoot objects
}