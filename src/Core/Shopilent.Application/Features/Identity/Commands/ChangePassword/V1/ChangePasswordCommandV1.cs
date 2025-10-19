using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Identity.Commands.ChangePassword.V1;

public sealed record ChangePasswordCommandV1 : ICommand
{
    public Guid UserId { get; init; }
    public string CurrentPassword { get; init; }
    public string NewPassword { get; init; }
    public string ConfirmPassword { get; init; }
}