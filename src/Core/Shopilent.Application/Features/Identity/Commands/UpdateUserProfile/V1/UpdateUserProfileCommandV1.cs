using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Identity.Commands.UpdateUserProfile.V1;

public sealed record UpdateUserProfileCommandV1 : ICommand<UpdateUserProfileResponseV1>
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string? MiddleName { get; init; }
    public string? Phone { get; init; }
}