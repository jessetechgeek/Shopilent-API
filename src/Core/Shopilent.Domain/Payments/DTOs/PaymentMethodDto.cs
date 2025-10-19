using Shopilent.Domain.Payments.Enums;

namespace Shopilent.Domain.Payments.DTOs;

public class PaymentMethodDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public PaymentMethodType Type { get; set; }
    public PaymentProvider Provider { get; set; }
    public string Token { get; set; }
    public string DisplayName { get; set; }
    public string CardBrand { get; set; }
    public string LastFourDigits { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}