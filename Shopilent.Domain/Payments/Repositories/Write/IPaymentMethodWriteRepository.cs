using Shopilent.Domain.Common.Repositories.Base.Write;
using Shopilent.Domain.Payments.Enums;

namespace Shopilent.Domain.Payments.Repositories.Write;

public interface IPaymentMethodWriteRepository : IAggregateWriteRepository<PaymentMethod>
{
    // EF Core will be used for reads in write repository as well
    Task<PaymentMethod> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentMethod>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PaymentMethod> GetDefaultForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentMethod>> GetByTypeAsync(Guid userId, PaymentMethodType type, CancellationToken cancellationToken = default);
    Task<PaymentMethod> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> TokenExistsAsync(string token, CancellationToken cancellationToken = default);
}