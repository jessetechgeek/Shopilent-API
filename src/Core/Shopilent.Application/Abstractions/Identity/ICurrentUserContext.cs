namespace Shopilent.Application.Abstractions.Identity;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    string IpAddress { get; }
    string UserAgent { get; }
    bool IsInRole(string role);
}