using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Identity.Commands.UpdateUserStatus.V1;

public sealed record UpdateUserStatusCommandV1 : ICommand
{
    public Guid Id { get; init; }
    public bool IsActive { get; init; }
}