namespace Shopilent.Domain.Common.Repositories.Base.Write;

public interface IEntityWriteRepository<T> : IWriteRepository<T> where T : Entity
{
    // Write repository specialized for Entity objects
}