using Shopilent.Domain.Shipping.DTOs;

namespace Shopilent.Domain.Identity.DTOs;

public class UserDetailDto : UserDto
{
    public IReadOnlyList<AddressDto> Addresses { get; set; }
    public IReadOnlyList<RefreshTokenDto> RefreshTokens { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LastFailedAttempt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? LastModified { get; set; }
}