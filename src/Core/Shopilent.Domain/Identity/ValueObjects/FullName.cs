using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.Errors;

namespace Shopilent.Domain.Identity.ValueObjects;

public class FullName : ValueObject
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? MiddleName { get; private set; }

    protected FullName()
    {
    }

    private FullName(string firstName, string lastName, string? middleName = null)
    {
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
    }
    
    public static Result<FullName> Create(string firstName, string lastName, string middleName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<FullName>(UserErrors.FirstNameRequired);

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<FullName>(UserErrors.LastNameRequired);

        return Result.Success(new FullName(firstName, lastName, middleName));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return MiddleName ?? string.Empty;
    }

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(MiddleName))
            return $"{FirstName} {LastName}";

        return $"{FirstName} {MiddleName} {LastName}";
    }
}