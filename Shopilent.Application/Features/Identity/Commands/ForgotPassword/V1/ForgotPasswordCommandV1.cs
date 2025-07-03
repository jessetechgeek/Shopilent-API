using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Identity.Commands.ForgotPassword.V1;

public sealed record ForgotPasswordCommandV1 : ICommand
{
    public string Email { get; init; }
}