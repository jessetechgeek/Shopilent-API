using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Identity.Commands.VerifyEmail.V1;

public sealed record VerifyEmailCommandV1 : ICommand
{
    public string Token { get; init; }
}