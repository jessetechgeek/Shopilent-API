using Shopilent.Domain.Identity.DTOs;
using Shopilent.Domain.Payments.DTOs;

namespace Shopilent.Domain.Sales.DTOs;

public class OrderDetailDto : OrderDto
{
    public IReadOnlyList<OrderItemDto> Items { get; set; }
    public UserDto User { get; set; }
    public PaymentMethodDto PaymentMethod { get; set; }
    public IReadOnlyList<PaymentDto> Payments { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? LastModified { get; set; }
}