using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shopilent.Application.Abstractions.Identity;

namespace Shopilent.Infrastructure.Identity.Services;

public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim?.Value != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
    }


    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;


    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string IpAddress
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            string ipAddress = null;

            if (httpContext != null)
            {
                // Try to get IP from X-Forwarded-For header
                ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

                // If not available, use RemoteIpAddress
                if (string.IsNullOrEmpty(ipAddress) && httpContext.Connection?.RemoteIpAddress != null)
                {
                    ipAddress = httpContext.Connection.RemoteIpAddress.ToString();
                }
            }

            return ipAddress ?? "Unknown";
        }
    }

    public string UserAgent => _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
}