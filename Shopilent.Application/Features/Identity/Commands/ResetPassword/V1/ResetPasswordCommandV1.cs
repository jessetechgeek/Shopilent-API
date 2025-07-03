using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Identity.Commands.ResetPassword.V1;

public sealed record ResetPasswordCommandV1 : ICommand
{
    public string Token { get; init; }
    public string NewPassword { get; init; }
    public string ConfirmPassword { get; init; }
}