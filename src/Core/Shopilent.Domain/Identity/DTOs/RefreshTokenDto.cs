namespace Shopilent.Domain.Identity.DTOs;

public class RefreshTokenDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime IssuedAt { get; set; }
    public bool IsRevoked { get; set; }
    public string RevokedReason { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}