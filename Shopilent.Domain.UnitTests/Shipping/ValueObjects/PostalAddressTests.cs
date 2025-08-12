using Shopilent.Domain.Shipping.ValueObjects;

namespace Shopilent.Domain.Tests.Shipping.ValueObjects;

public class PostalAddressTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreatePostalAddress()
    {
        // Arrange
        var addressLine1 = "123 Main St";
        var city = "Anytown";
        var state = "State";
        var country = "Country";
        var postalCode = "12345";
        var addressLine2 = "Apt 4B";

        // Act
        var result = PostalAddress.Create(
            addressLine1,
            city,
            state,
            country,
            postalCode,
            addressLine2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var address = result.Value;
        address.AddressLine1.Should().Be(addressLine1);
        address.AddressLine2.Should().Be(addressLine2);
        address.City.Should().Be(city);
        address.State.Should().Be(state);
        address.Country.Should().Be(country);
        address.PostalCode.Should().Be(postalCode);
    }

    [Fact]
    public void Create_WithoutAddressLine2_ShouldCreatePostalAddress()
    {
        // Arrange
        var addressLine1 = "123 Main St";
        var city = "Anytown";
        var state = "State";
        var country = "Country";
        var postalCode = "12345";

        // Act
        var result = PostalAddress.Create(
            addressLine1,
            city,
            state,
            country,
            postalCode);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var address = result.Value;
        address.AddressLine1.Should().Be(addressLine1);
        address.AddressLine2.Should().BeNull();
        address.City.Should().Be(city);
        address.State.Should().Be(state);
        address.Country.Should().Be(country);
        address.PostalCode.Should().Be(postalCode);
    }

    [Fact]
    public void Create_WithEmptyAddressLine1_ShouldReturnFailure()
    {
        // Arrange
        var addressLine1 = string.Empty;
        var city = "Anytown";
        var state = "State";
        var country = "Country";
        var postalCode = "12345";

        // Act
        var result = PostalAddress.Create(
            addressLine1,
            city,
            state,
            country,
            postalCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Address.AddressLine1Required");
    }

    [Fact]
    public void Create_WithEmptyCity_ShouldReturnFailure()
    {
        // Arrange
        var addressLine1 = "123 Main St";
        var city = string.Empty;
        var state = "State";
        var country = "Country";
        var postalCode = "12345";

        // Act
        var result = PostalAddress.Create(
            addressLine1,
            city,
            state,
            country,
            postalCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Address.CityRequired");
    }

    [Fact]
    public void Create_WithEmptyState_ShouldReturnFailure()
    {
        // Arrange
        var addressLine1 = "123 Main St";
        var city = "Anytown";
        var state = string.Empty;
        var country = "Country";
        var postalCode = "12345";

        // Act
        var result = PostalAddress.Create(
            addressLine1,
            city,
            state,
            country,
            postalCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Address.StateRequired");
    }

    [Fact]
    public void Create_WithEmptyCountry_ShouldReturnFailure()
    {
        // Arrange
        var addressLine1 = "123 Main St";
        var city = "Anytown";
        var state = "State";
        var country = string.Empty;
        var postalCode = "12345";

        // Act
        var result = PostalAddress.Create(
            addressLine1,
            city,
            state,
            country,
            postalCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Address.CountryRequired");
    }

    [Fact]
    public void Create_WithEmptyPostalCode_ShouldReturnFailure()
    {
        // Arrange
        var addressLine1 = "123 Main St";
        var city = "Anytown";
        var state = "State";
        var country = "Country";
        var postalCode = string.Empty;

        // Act
        var result = PostalAddress.Create(
            addressLine1,
            city,
            state,
            country,
            postalCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Address.PostalCodeRequired");
    }

    [Fact]
    public void ToString_WithAddressLine2_ShouldIncludeAllParts()
    {
        // Arrange
        var addressLine1 = "123 Main St";
        var city = "Anytown";
        var state = "State";
        var country = "Country";
        var postalCode = "12345";
        var addressLine2 = "Apt 4B";

        var result = PostalAddress.Create(addressLine1, city, state, country, postalCode, addressLine2);
        result.IsSuccess.Should().BeTrue();
        var address = result.Value;
        var expected = "123 Main St, Apt 4B, Anytown, State 12345, Country";

        // Act
        var toStringResult = address.ToString();

        // Assert
        toStringResult.Should().Be(expected);
    }

    [Fact]
    public void ToString_WithoutAddressLine2_ShouldExcludeIt()
    {
        // Arrange
        var addressLine1 = "123 Main St";
        var city = "Anytown";
        var state = "State";
        var country = "Country";
        var postalCode = "12345";

        var result = PostalAddress.Create(addressLine1, city, state, country, postalCode);
        result.IsSuccess.Should().BeTrue();
        var address = result.Value;
        var expected = "123 Main St, Anytown, State 12345, Country";

        // Act
        var toStringResult = address.ToString();

        // Assert
        toStringResult.Should().Be(expected);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345", "Apt 4B");
        var address2Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345", "Apt 4B");
        
        address1Result.IsSuccess.Should().BeTrue();
        address2Result.IsSuccess.Should().BeTrue();
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;

        // Act & Assert
        address1.Equals(address2).Should().BeTrue();
        (address1 == address2).Should().BeTrue();
        (address1 != address2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentAddressLine1_ShouldReturnFalse()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345");
        var address2Result = PostalAddress.Create("456 Oak Ave", "Anytown", "State", "Country", "12345");
        
        address1Result.IsSuccess.Should().BeTrue();
        address2Result.IsSuccess.Should().BeTrue();
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;

        // Act & Assert
        address1.Equals(address2).Should().BeFalse();
        (address1 == address2).Should().BeFalse();
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCity_ShouldReturnFalse()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345");
        var address2Result = PostalAddress.Create("123 Main St", "Othertown", "State", "Country", "12345");
        
        address1Result.IsSuccess.Should().BeTrue();
        address2Result.IsSuccess.Should().BeTrue();
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;

        // Act & Assert
        address1.Equals(address2).Should().BeFalse();
        (address1 == address2).Should().BeFalse();
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentState_ShouldReturnFalse()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345");
        var address2Result = PostalAddress.Create("123 Main St", "Anytown", "OtherState", "Country", "12345");
        
        address1Result.IsSuccess.Should().BeTrue();
        address2Result.IsSuccess.Should().BeTrue();
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;

        // Act & Assert
        address1.Equals(address2).Should().BeFalse();
        (address1 == address2).Should().BeFalse();
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCountry_ShouldReturnFalse()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345");
        var address2Result = PostalAddress.Create("123 Main St", "Anytown", "State", "OtherCountry", "12345");
        
        address1Result.IsSuccess.Should().BeTrue();
        address2Result.IsSuccess.Should().BeTrue();
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;

        // Act & Assert
        address1.Equals(address2).Should().BeFalse();
        (address1 == address2).Should().BeFalse();
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentPostalCode_ShouldReturnFalse()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345");
        var address2Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "67890");
        
        address1Result.IsSuccess.Should().BeTrue();
        address2Result.IsSuccess.Should().BeTrue();
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;

        // Act & Assert
        address1.Equals(address2).Should().BeFalse();
        (address1 == address2).Should().BeFalse();
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentAddressLine2_ShouldReturnFalse()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345", "Apt 4B");
        var address2Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345", "Suite 100");
        
        address1Result.IsSuccess.Should().BeTrue();
        address2Result.IsSuccess.Should().BeTrue();
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;

        // Act & Assert
        address1.Equals(address2).Should().BeFalse();
        (address1 == address2).Should().BeFalse();
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNullAddressLine2_ShouldHandleComparisonCorrectly()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345");
        var address2Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345");
        var address3Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345", "Apt 4B");
        
        address1Result.IsSuccess.Should().BeTrue();
        address2Result.IsSuccess.Should().BeTrue();
        address3Result.IsSuccess.Should().BeTrue();
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;
        var address3 = address3Result.Value;

        // Act & Assert
        address1.Equals(address2).Should().BeTrue();
        address1.Equals(address3).Should().BeFalse();
    }
}