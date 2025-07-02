namespace Shopilent.Infrastructure.Identity.Configuration.Settings;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Shopilent";
    public string Audience { get; set; } = "ShopilentClient";
    public int TokenLifetimeMinutes { get; set; } = 15;
    public int RefreshTokenLifetimeDays { get; set; } = 7;
}