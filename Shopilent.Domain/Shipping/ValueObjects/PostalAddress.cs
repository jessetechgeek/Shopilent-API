using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Shipping.Errors;

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

    private PostalAddress(
        string addressLine1,
        string city,
        string state,
        string country,
        string postalCode,
        string addressLine2 = null)
    {
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        Country = country;
        PostalCode = postalCode;
    }

    public static Result<PostalAddress> Create(
        string addressLine1,
        string city,
        string state,
        string country,
        string postalCode,
        string addressLine2 = null)
    {
        if (string.IsNullOrWhiteSpace(addressLine1))
            return Result.Failure<PostalAddress>(AddressErrors.AddressLine1Required);

        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<PostalAddress>(AddressErrors.CityRequired);

        if (string.IsNullOrWhiteSpace(state))
            return Result.Failure<PostalAddress>(AddressErrors.StateRequired);

        if (string.IsNullOrWhiteSpace(country))
            return Result.Failure<PostalAddress>(AddressErrors.CountryRequired);

        if (string.IsNullOrWhiteSpace(postalCode))
            return Result.Failure<PostalAddress>(AddressErrors.PostalCodeRequired);

        return Result.Success(new PostalAddress(addressLine1, city, state, country, postalCode, addressLine2));
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