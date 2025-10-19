namespace Shopilent.Application.Features.Identity.Commands.UpdateUserProfile.V1;

public sealed class UpdateUserProfileResponseV1
{
    public Guid Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string? MiddleName { get; init; }
    public string? Phone { get; init; }
    public DateTime UpdatedAt { get; init; }
}