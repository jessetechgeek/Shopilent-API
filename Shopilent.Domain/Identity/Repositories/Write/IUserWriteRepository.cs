using Shopilent.Domain.Common.Repositories.Write;

namespace Shopilent.Domain.Identity.Repositories.Write;

public interface IUserWriteRepository : IAggregateWriteRepository<User>
{
    // EF Core will be used for reads in write repository as well
    Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User> GetByRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<User> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<User> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
}