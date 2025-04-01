using Shopilent.Domain.Common.Repositories.Base.Read;
using Shopilent.Domain.Payments.DTOs;
using Shopilent.Domain.Payments.Enums;

namespace Shopilent.Domain.Payments.Repositories.Read;

public interface IPaymentMethodReadRepository : IAggregateReadRepository<PaymentMethodDto>
{
    Task<IReadOnlyList<PaymentMethodDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PaymentMethodDto> GetDefaultForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentMethodDto>> GetByTypeAsync(Guid userId, PaymentMethodType type, CancellationToken cancellationToken = default);
    Task<PaymentMethodDto> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> TokenExistsAsync(string token, CancellationToken cancellationToken = default);
}