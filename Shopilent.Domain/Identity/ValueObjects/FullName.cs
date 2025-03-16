using Shopilent.Domain.Common;

namespace Shopilent.Domain.Identity.ValueObjects;

public class FullName : ValueObject
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string MiddleName { get; private set; }

    protected FullName()
    {
    }

    public FullName(string firstName, string lastName, string middleName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
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