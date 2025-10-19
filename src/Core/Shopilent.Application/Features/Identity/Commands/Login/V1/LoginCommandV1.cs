using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Identity.Commands.Login.V1;

public sealed record LoginCommandV1 : ICommand<LoginResponseV1>
{
    public string Email { get; init; }
    public string Password { get; init; }
    public string IpAddress { get; init; }
    public string UserAgent { get; init; }
}