namespace Shopilent.Domain.Common.Repositories.Base.Read;

public interface IEntityReadRepository<TDto> : IReadRepository<TDto> where TDto : class
{
    // Read repository specialized for Entity objects returning DTOs
}