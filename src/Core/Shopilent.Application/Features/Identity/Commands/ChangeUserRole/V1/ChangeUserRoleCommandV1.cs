using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.Application.Features.Identity.Commands.ChangeUserRole.V1;

public sealed record ChangeUserRoleCommandV1 : ICommand<string>
{
    public Guid UserId { get; init; }
    public UserRole NewRole { get; init; }
}