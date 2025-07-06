using Dapper;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Payments;
using Shopilent.Domain.Payments.DTOs;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Payments.Repositories.Read;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Read;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Payments.Read;

public class PaymentMethodReadRepository : AggregateReadRepositoryBase<PaymentMethod, PaymentMethodDto>,
    IPaymentMethodReadRepository
{
    public PaymentMethodReadRepository(IDapperConnectionFactory connectionFactory,
        ILogger<PaymentMethodReadRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    public override async Task<PaymentMethodDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                user_id AS UserId,
                type AS Type,
                provider AS Provider,
                token AS Token,
                display_name AS DisplayName,
                card_brand AS CardBrand,
                last_four_digits AS LastFourDigits,
                expiry_date AS ExpiryDate,
                is_default AS IsDefault,
                is_active AS IsActive,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM payment_methods
            WHERE id = @Id";

        return await Connection.QueryFirstOrDefaultAsync<PaymentMethodDto>(sql, new { Id = id });
    }

    public override async Task<IReadOnlyList<PaymentMethodDto>> ListAllAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                user_id AS UserId,
                type AS Type,
                provider AS Provider,
                display_name AS DisplayName,
                card_brand AS CardBrand,
                last_four_digits AS LastFourDigits,
                expiry_date AS ExpiryDate,
                is_default AS IsDefault,
                is_active AS IsActive,
                metadata AS Metadata,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM payment_methods
            ORDER BY created_at DESC";

        var paymentMethodDtos = await Connection.QueryAsync<PaymentMethodDto>(sql);
        return paymentMethodDtos.ToList();
    }

    public async Task<IReadOnlyList<PaymentMethodDto>> GetByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                user_id AS UserId,
                type AS Type,
                provider AS Provider,
                display_name AS DisplayName,
                card_brand AS CardBrand,
                last_four_digits AS LastFourDigits,
                expiry_date AS ExpiryDate,
                is_default AS IsDefault,
                is_active AS IsActive,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM payment_methods
            WHERE user_id = @UserId
            ORDER BY is_default DESC, created_at DESC";

        var paymentMethodDtos = await Connection.QueryAsync<PaymentMethodDto>(sql, new { UserId = userId });
        return paymentMethodDtos.ToList();
    }

    public async Task<PaymentMethodDto> GetDefaultForUserAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                user_id AS UserId,
                type AS Type,
                provider AS Provider,
                display_name AS DisplayName,
                card_brand AS CardBrand,
                last_four_digits AS LastFourDigits,
                expiry_date AS ExpiryDate,
                is_default AS IsDefault,
                is_active AS IsActive,
                metadata AS Metadata,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM payment_methods
            WHERE user_id = @UserId
            AND is_default = true
            AND is_active = true
            LIMIT 1";

        return await Connection.QueryFirstOrDefaultAsync<PaymentMethodDto>(sql, new { UserId = userId });
    }

    public async Task<IReadOnlyList<PaymentMethodDto>> GetByTypeAsync(Guid userId, PaymentMethodType type,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                user_id AS UserId,
                type AS Type,
                provider AS Provider,
                display_name AS DisplayName,
                card_brand AS CardBrand,
                last_four_digits AS LastFourDigits,
                expiry_date AS ExpiryDate,
                is_default AS IsDefault,
                is_active AS IsActive,
                metadata AS Metadata,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM payment_methods
            WHERE user_id = @UserId
            AND type = @Type
            AND is_active = true
            ORDER BY is_default DESC, created_at DESC";

        var paymentMethodDtos = await Connection.QueryAsync<PaymentMethodDto>(
            sql, new { UserId = userId, Type = type.ToString() });

        return paymentMethodDtos.ToList();
    }

    public async Task<PaymentMethodDto> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        const string sql = @"
            SELECT 
                id AS Id,
                user_id AS UserId,
                type AS Type,
                provider AS Provider,
                display_name AS DisplayName,
                card_brand AS CardBrand,
                last_four_digits AS LastFourDigits,
                expiry_date AS ExpiryDate,
                is_default AS IsDefault,
                is_active AS IsActive,
                metadata AS Metadata,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM payment_methods
            WHERE token = @Token";

        return await Connection.QueryFirstOrDefaultAsync<PaymentMethodDto>(sql, new { Token = token });
    }

    public async Task<bool> TokenExistsAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        const string sql = @"
            SELECT COUNT(1) FROM payment_methods
            WHERE token = @Token";

        int count = await Connection.ExecuteScalarAsync<int>(sql, new { Token = token });
        return count > 0;
    }
}