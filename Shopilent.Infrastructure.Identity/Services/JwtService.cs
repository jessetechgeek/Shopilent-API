using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.Enums;
using Microsoft.IdentityModel.Tokens;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Infrastructure.Identity.Abstractions;
using Shopilent.Infrastructure.Identity.Configuration.Settings;

namespace Shopilent.Infrastructure.Identity.Services;

internal class JwtService : IJwtService
{
    private readonly JwtSettings _settings;
    private readonly SigningCredentials _signingCredentials;
    private readonly TokenValidationParameters _validationParameters;

    public JwtService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;

        // Configure signing credentials
        var keyBytes = Encoding.UTF8.GetBytes(_settings.Secret);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Configure token validation parameters
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email.Value),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, $"{user.FullName.FirstName} {user.FullName.LastName}"),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        // Add additional claims if user is admin or manager
        if (user.Role == UserRole.Admin || user.Role == UserRole.Manager)
        {
            claims.Add(new Claim("IsStaff", "true"));
        }

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.TokenLifetimeMinutes),
            signingCredentials: _signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public bool ValidateAccessToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            tokenHandler.ValidateToken(token, _validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public (bool isValid, string email, Guid userId) DecodeAccessToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return (false, string.Empty, Guid.Empty);

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, _validationParameters, out _);

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var emailClaim = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return (false, string.Empty, Guid.Empty);

            return (true, emailClaim ?? string.Empty, userId);
        }
        catch
        {
            return (false, string.Empty, Guid.Empty);
        }
    }

    public DateTime GetAccessTokenExpiration(string token)
    {
        if (string.IsNullOrEmpty(token))
            return DateTime.MinValue;

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            return jwtToken?.ValidTo ?? DateTime.MinValue;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }
}