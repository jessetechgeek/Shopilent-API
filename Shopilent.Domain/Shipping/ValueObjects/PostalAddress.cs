using Shopilent.Domain.Common;

namespace Shopilent.Domain.Shipping.ValueObjects;

public class PostalAddress : ValueObject
{
    public string AddressLine1 { get; private set; }
    public string AddressLine2 { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }

    protected PostalAddress()
    {
    }

    public PostalAddress(
        string addressLine1,
        string city,
        string state,
        string country,
        string postalCode,
        string addressLine2 = null)
    {
        if (string.IsNullOrWhiteSpace(addressLine1))
            throw new ArgumentException("Address line 1 cannot be empty", nameof(addressLine1));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty", nameof(state));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be empty", nameof(postalCode));

        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        Country = country;
        PostalCode = postalCode;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AddressLine1;
        yield return AddressLine2 ?? string.Empty;
        yield return City;
        yield return State;
        yield return Country;
        yield return PostalCode;
    }

    public override string ToString()
    {
        var address = $"{AddressLine1}";

        if (!string.IsNullOrWhiteSpace(AddressLine2))
            address += $", {AddressLine2}";

        address += $", {City}, {State} {PostalCode}, {Country}";
        return address;
    }
}