namespace Shopilent.Domain.Common.Repositories.Write;

public interface IEntityWriteRepository<T> : IWriteRepository<T> where T : Entity
{
    // Write repository specialized for Entity objects
}