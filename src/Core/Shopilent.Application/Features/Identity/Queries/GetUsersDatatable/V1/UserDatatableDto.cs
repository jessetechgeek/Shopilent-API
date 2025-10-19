using Shopilent.Domain.Identity.Enums;

namespace Shopilent.Application.Features.Identity.Queries.GetUsersDatatable.V1;

public sealed class UserDatatableDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public UserRole Role { get; set; }
    public string RoleName { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int AddressCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}