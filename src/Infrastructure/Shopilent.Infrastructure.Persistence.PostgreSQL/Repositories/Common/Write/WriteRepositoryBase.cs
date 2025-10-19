using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Common.Repositories.Write;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Write;

public abstract class WriteRepositoryBase<T> : IWriteRepository<T> where T : class
{
    protected readonly ApplicationDbContext DbContext;
    protected readonly DbSet<T> DbSet;
    protected readonly ILogger Logger;

    protected WriteRepositoryBase(ApplicationDbContext dbContext, ILogger logger)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DbSet = dbContext.Set<T>();
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entry = DbContext.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            DbSet.Attach(entity);
        }

        entry.State = EntityState.Modified;

        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }
}