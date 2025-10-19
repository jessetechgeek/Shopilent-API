using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Identity.Commands.UpdateUser.V1;

public sealed record UpdateUserCommandV1 : ICommand<UpdateUserResponseV1>
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string MiddleName { get; init; }
    public string Phone { get; init; }
    public string IpAddress { get; init; }
    public string UserAgent { get; init; }
}