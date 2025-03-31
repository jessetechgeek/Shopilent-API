namespace Shopilent.Domain.Common.Repositories.Base.Read;

public interface IEntityReadRepository<T> : IReadRepository<T> where T : Entity
{
    // Read repository specialized for Entity objects
}