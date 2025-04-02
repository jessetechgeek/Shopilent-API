using Shopilent.Domain.Common.Repositories.Read;
using Shopilent.Domain.Identity.DTOs;

namespace Shopilent.Domain.Identity.Repositories.Read;

public interface IUserReadRepository : IAggregateReadRepository<UserDto>
{
    Task<UserDetailDto> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User> GetUserByCredentialsAsync(string email, string passwordHash,
        CancellationToken cancellationToken = default);

    Task<UserDto> GetByRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<UserDto> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<UserDto> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> GetByRoleAsync(string role, CancellationToken cancellationToken = default);
}