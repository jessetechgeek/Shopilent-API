using Dapper;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.DTOs;
using Shopilent.Domain.Identity.Repositories.Read;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Read;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Identity.Read;

public class RefreshTokenReadRepository : EntityReadRepositoryBase<RefreshToken, RefreshTokenDto>,
    IRefreshTokenReadRepository
{
    public RefreshTokenReadRepository(IDapperConnectionFactory connectionFactory,
        ILogger<RefreshTokenReadRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    public override async Task<RefreshTokenDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                user_id AS UserId,
                token AS Token,
                expires_at AS ExpiresAt,
                issued_at AS IssuedAt,
                is_revoked AS IsRevoked,
                revoked_reason AS RevokedReason,
                ip_address AS IpAddress,
                user_agent AS UserAgent,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM refresh_tokens
            WHERE id = @Id";

        return await Connection.QueryFirstOrDefaultAsync<RefreshTokenDto>(sql, new { Id = id });
    }

    public override async Task<IReadOnlyList<RefreshTokenDto>> ListAllAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                user_id AS UserId,
                token AS Token,
                expires_at AS ExpiresAt,
                issued_at AS IssuedAt,
                is_revoked AS IsRevoked,
                revoked_reason AS RevokedReason,
                ip_address AS IpAddress,
                user_agent AS UserAgent,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM refresh_tokens";

        var refreshTokenDtos = await Connection.QueryAsync<RefreshTokenDto>(sql);
        return refreshTokenDtos.ToList();
    }

    public async Task<RefreshTokenDto> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                user_id AS UserId,
                token AS Token,
                expires_at AS ExpiresAt,
                issued_at AS IssuedAt,
                is_revoked AS IsRevoked,
                revoked_reason AS RevokedReason,
                ip_address AS IpAddress,
                user_agent AS UserAgent,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM refresh_tokens
            WHERE token = @Token";

        return await Connection.QueryFirstOrDefaultAsync<RefreshTokenDto>(sql, new { Token = token });
    }

    public async Task<IReadOnlyList<RefreshTokenDto>> GetByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                user_id AS UserId,
                token AS Token,
                expires_at AS ExpiresAt,
                issued_at AS IssuedAt,
                is_revoked AS IsRevoked,
                revoked_reason AS RevokedReason,
                ip_address AS IpAddress,
                user_agent AS UserAgent,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM refresh_tokens
            WHERE user_id = @UserId";

        var tokenDtos = await Connection.QueryAsync<RefreshTokenDto>(sql, new { UserId = userId });
        return tokenDtos.ToList();
    }

    public async Task<IReadOnlyList<RefreshTokenDto>> GetActiveTokensAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                user_id AS UserId,
                token AS Token,
                expires_at AS ExpiresAt,
                issued_at AS IssuedAt,
                is_revoked AS IsRevoked,
                revoked_reason AS RevokedReason,
                ip_address AS IpAddress,
                user_agent AS UserAgent,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM refresh_tokens
            WHERE user_id = @UserId
            AND is_revoked = false
            AND expires_at > @Now";

        var tokenDtos = await Connection.QueryAsync<RefreshTokenDto>(
            sql, new { UserId = userId, Now = DateTime.UtcNow });

        return tokenDtos.ToList();
    }
}