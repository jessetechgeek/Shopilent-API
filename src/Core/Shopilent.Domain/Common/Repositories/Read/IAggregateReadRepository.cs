namespace Shopilent.Domain.Common.Repositories.Read;

public interface IAggregateReadRepository<TDto> : IReadRepository<TDto> where TDto : class
{
    // Read repository specialized for AggregateRoot objects returning DTOs
}