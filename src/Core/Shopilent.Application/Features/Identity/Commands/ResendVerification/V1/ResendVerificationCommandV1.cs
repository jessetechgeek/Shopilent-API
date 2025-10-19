using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Identity.Commands.ResendVerification.V1;

public sealed record ResendVerificationCommandV1 : ICommand
{
    public string Email { get; init; }
}