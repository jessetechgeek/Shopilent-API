using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Payments;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Payments.Repositories.Write;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Write;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Payments.Write;

public class PaymentMethodWriteRepository : AggregateWriteRepositoryBase<PaymentMethod>, IPaymentMethodWriteRepository
{
    public PaymentMethodWriteRepository(ApplicationDbContext dbContext, ILogger<PaymentMethodWriteRepository> logger)
        : base(dbContext, logger)
    {
    }

    public async Task<PaymentMethod> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentMethod>> GetByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.PaymentMethods
            .Where(pm => pm.UserId == userId)
            .OrderByDescending(pm => pm.IsDefault)
            .ThenByDescending(pm => pm.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentMethod> GetDefaultForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.PaymentMethods
            .Where(pm => pm.UserId == userId && pm.IsDefault && pm.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentMethod>> GetByTypeAsync(Guid userId, PaymentMethodType type,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.PaymentMethods
            .Where(pm => pm.UserId == userId && pm.Type == type && pm.IsActive)
            .OrderByDescending(pm => pm.IsDefault)
            .ThenByDescending(pm => pm.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentMethod> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        return await DbContext.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Token == token, cancellationToken);
    }

    public async Task<bool> TokenExistsAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        return await DbContext.PaymentMethods
            .AnyAsync(pm => pm.Token == token, cancellationToken);
    }
}