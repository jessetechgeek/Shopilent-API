using Dapper;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.DTOs;
using Shopilent.Domain.Identity.Repositories.Read;
using Shopilent.Domain.Shipping.DTOs;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Read;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Identity.Read;

public class UserReadRepository : AggregateReadRepositoryBase<User, UserDto>, IUserReadRepository
{
    public UserReadRepository(IDapperConnectionFactory connectionFactory, ILogger<UserReadRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    public override async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                email AS Email,
                first_name AS FirstName,
                last_name AS LastName,
                middle_name AS MiddleName,
                phone AS Phone,
                role AS Role,
                is_active AS IsActive,
                last_login AS LastLogin,
                email_verified AS EmailVerified,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM users
            WHERE id = @Id";

        return await Connection.QueryFirstOrDefaultAsync<UserDto>(sql, new { Id = id });
    }

    public override async Task<IReadOnlyList<UserDto>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                email AS Email,
                first_name AS FirstName,
                last_name AS LastName,
                middle_name AS MiddleName,
                phone AS Phone,
                role AS Role,
                is_active AS IsActive,
                last_login AS LastLogin,
                email_verified AS EmailVerified,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM users";

        var userDtos = await Connection.QueryAsync<UserDto>(sql);
        return userDtos.ToList();
    }

    public async Task<UserDetailDto> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string userSql = @"
            SELECT 
                id AS Id,
                email AS Email,
                first_name AS FirstName,
                last_name AS LastName,
                middle_name AS MiddleName,
                phone AS Phone,
                role AS Role,
                is_active AS IsActive,
                last_login AS LastLogin,
                email_verified AS EmailVerified,
                failed_login_attempts AS FailedLoginAttempts,
                last_failed_attempt AS LastFailedAttempt,
                created_by AS CreatedBy,
                modified_by AS ModifiedBy,
                last_modified AS LastModified,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM users
            WHERE id = @Id";

        var userDetail = await Connection.QueryFirstOrDefaultAsync<UserDetailDto>(userSql, new { Id = id });

        if (userDetail != null)
        {
            // Load addresses
            const string addressesSql = @"
                SELECT 
                    id AS Id,
                    user_id AS UserId,
                    address_line1 AS AddressLine1,
                    address_line2 AS AddressLine2,
                    city AS City,
                    state AS State,
                    postal_code AS PostalCode,
                    country AS Country,
                    phone AS Phone,
                    is_default AS IsDefault,
                    address_type AS AddressType,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM addresses
                WHERE user_id = @UserId";

            userDetail.Addresses = (await Connection.QueryAsync<AddressDto>(
                addressesSql, new { UserId = id })).ToList();

            // Load refresh tokens
            const string tokensSql = @"
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

            userDetail.RefreshTokens = (await Connection.QueryAsync<RefreshTokenDto>(
                tokensSql, new { UserId = id })).ToList();
        }

        return userDetail;
    }

    public async Task<UserDto> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                email AS Email,
                first_name AS FirstName,
                last_name AS LastName,
                middle_name AS MiddleName,
                phone AS Phone,
                role AS Role,
                is_active AS IsActive,
                last_login AS LastLogin,
                email_verified AS EmailVerified,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM users
            WHERE email = @Email";

        return await Connection.QueryFirstOrDefaultAsync<UserDto>(sql, new { Email = email });
    }

    public async Task<User> GetUserByCredentialsAsync(string email, string passwordHash,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM users
            WHERE email = @Email
            AND password_hash = @PasswordHash
            AND is_active = true";

        return await Connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email, PasswordHash = passwordHash });
    }

    public async Task<UserDto> GetByRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                u.id AS Id,
                u.email AS Email,
                u.first_name AS FirstName,
                u.last_name AS LastName,
                u.middle_name AS MiddleName,
                u.phone AS Phone,
                u.role AS Role,
                u.is_active AS IsActive,
                u.last_login AS LastLogin,
                u.email_verified AS EmailVerified,
                u.created_at AS CreatedAt,
                u.updated_at AS UpdatedAt
            FROM users u
            JOIN refresh_tokens rt ON u.id = rt.user_id
            WHERE rt.token = @Token
            AND rt.is_revoked = false
            AND rt.expires_at > @Now";

        return await Connection.QueryFirstOrDefaultAsync<UserDto>(sql, new { Token = token, Now = DateTime.UtcNow });
    }

    public async Task<UserDto> GetByEmailVerificationTokenAsync(string token,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                email AS Email,
                first_name AS FirstName,
                last_name AS LastName,
                middle_name AS MiddleName,
                phone AS Phone,
                role AS Role,
                is_active AS IsActive,
                last_login AS LastLogin,
                email_verified AS EmailVerified,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM users
            WHERE email_verification_token = @Token
            AND email_verification_expires > @Now";

        return await Connection.QueryFirstOrDefaultAsync<UserDto>(sql, new { Token = token, Now = DateTime.UtcNow });
    }

    public async Task<UserDto> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                email AS Email,
                first_name AS FirstName,
                last_name AS LastName,
                middle_name AS MiddleName,
                phone AS Phone,
                role AS Role,
                is_active AS IsActive,
                last_login AS LastLogin,
                email_verified AS EmailVerified,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM users
            WHERE password_reset_token = @Token
            AND password_reset_expires > @Now";

        return await Connection.QueryFirstOrDefaultAsync<UserDto>(sql, new { Token = token, Now = DateTime.UtcNow });
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        string sql;
        object parameters;

        if (excludeId.HasValue)
        {
            sql = @"
                SELECT COUNT(1) FROM users
                WHERE email = @Email AND id != @ExcludeId";
            parameters = new { Email = email, ExcludeId = excludeId.Value };
        }
        else
        {
            sql = @"
                SELECT COUNT(1) FROM users
                WHERE email = @Email";
            parameters = new { Email = email };
        }

        int count = await Connection.ExecuteScalarAsync<int>(sql, parameters);
        return count > 0;
    }

    public async Task<IReadOnlyList<UserDto>> GetByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                email AS Email,
                first_name AS FirstName,
                last_name AS LastName,
                middle_name AS MiddleName,
                phone AS Phone,
                role AS Role,
                is_active AS IsActive,
                last_login AS LastLogin,
                email_verified AS EmailVerified,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM users
            WHERE role = @Role";

        var users = await Connection.QueryAsync<UserDto>(sql, new { Role = role });
        return users.ToList();
    }

    public async Task<IReadOnlyList<UserDto>> GetByIdsAsync(IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids == null || !ids.Any())
            return new List<UserDto>();

        // Convert the IDs to an array for SQL parameters
        var idArray = ids.ToArray();

        const string sql = @"
        SELECT 
            id AS Id,
            email AS Email,
            first_name AS FirstName,
            last_name AS LastName,
            middle_name AS MiddleName,
            phone AS Phone,
            role AS Role,
            is_active AS IsActive,
            last_login AS LastLogin,
            email_verified AS EmailVerified,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM users
        WHERE id = ANY(@Ids)
        ORDER BY array_position(@Ids, id)";

        var parameters = new { Ids = idArray };
        var userDtos = await Connection.QueryAsync<UserDto>(sql, parameters);
        return userDtos.ToList();
    }
}