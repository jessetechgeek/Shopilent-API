using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Identity.Commands.Logout.V1;

public sealed record LogoutCommandV1 : ICommand
{
    public string RefreshToken { get; init; }
    public string Reason { get; init; } = "User logged out";
}