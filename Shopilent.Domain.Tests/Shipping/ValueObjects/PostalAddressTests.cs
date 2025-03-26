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
        Assert.True(result.IsSuccess);
        var address = result.Value;
        Assert.Equal(addressLine1, address.AddressLine1);
        Assert.Equal(addressLine2, address.AddressLine2);
        Assert.Equal(city, address.City);
        Assert.Equal(state, address.State);
        Assert.Equal(country, address.Country);
        Assert.Equal(postalCode, address.PostalCode);
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
        Assert.True(result.IsSuccess);
        var address = result.Value;
        Assert.Equal(addressLine1, address.AddressLine1);
        Assert.Null(address.AddressLine2);
        Assert.Equal(city, address.City);
        Assert.Equal(state, address.State);
        Assert.Equal(country, address.Country);
        Assert.Equal(postalCode, address.PostalCode);
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
        Assert.True(result.IsFailure);
        Assert.Equal("Address.AddressLine1Required", result.Error.Code);
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
        Assert.True(result.IsFailure);
        Assert.Equal("Address.CityRequired", result.Error.Code);
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
        Assert.True(result.IsFailure);
        Assert.Equal("Address.StateRequired", result.Error.Code);
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
        Assert.True(result.IsFailure);
        Assert.Equal("Address.CountryRequired", result.Error.Code);
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
        Assert.True(result.IsFailure);
        Assert.Equal("Address.PostalCodeRequired", result.Error.Code);
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
        Assert.True(result.IsSuccess);
        var address = result.Value;
        var expected = "123 Main St, Apt 4B, Anytown, State 12345, Country";

        // Act
        var toStringResult = address.ToString();

        // Assert
        Assert.Equal(expected, toStringResult);
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
        Assert.True(result.IsSuccess);
        var address = result.Value;
        var expected = "123 Main St, Anytown, State 12345, Country";

        // Act
        var toStringResult = address.ToString();

        // Assert
        Assert.Equal(expected, toStringResult);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345", "Apt 4B");
        var address2Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345", "Apt 4B");
        
        Assert.True(address1Result.IsSuccess);
        Assert.True(address2Result.IsSuccess);
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;

        // Act & Assert
        Assert.True(address1.Equals(address2));
        Assert.True(address1 == address2);
        Assert.False(address1 != address2);
    }

    [Fact]
    public void Equals_WithDifferentAddressLine1_ShouldReturnFalse()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345");
        var address2Result = PostalAddress.Create("456 Oak Ave", "Anytown", "State", "Country", "12345");
        
        Assert.True(address1Result.IsSuccess);
        Assert.True(address2Result.IsSuccess);
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;

        // Act & Assert
        Assert.False(address1.Equals(address2));
        Assert.False(address1 == address2);
        Assert.True(address1 != address2);
    }

    [Fact]
    public void Equals_WithDifferentCity_ShouldReturnFalse()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345");
        var address2Result = PostalAddress.Create("123 Main St", "Othertown", "State", "Country", "12345");
        
        Assert.True(address1Result.IsSuccess);
        Assert.True(address2Result.IsSuccess);
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;

        // Act & Assert
        Assert.False(address1.Equals(address2));
        Assert.False(address1 == address2);
        Assert.True(address1 != address2);
    }

    [Fact]
    public void Equals_WithDifferentState_ShouldReturnFalse()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345");
        var address2Result = PostalAddress.Create("123 Main St", "Anytown", "OtherState", "Country", "12345");
        
        Assert.True(address1Result.IsSuccess);
        Assert.True(address2Result.IsSuccess);
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;

        // Act & Assert
        Assert.False(address1.Equals(address2));
        Assert.False(address1 == address2);
        Assert.True(address1 != address2);
    }

    [Fact]
    public void Equals_WithDifferentCountry_ShouldReturnFalse()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345");
        var address2Result = PostalAddress.Create("123 Main St", "Anytown", "State", "OtherCountry", "12345");
        
        Assert.True(address1Result.IsSuccess);
        Assert.True(address2Result.IsSuccess);
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;

        // Act & Assert
        Assert.False(address1.Equals(address2));
        Assert.False(address1 == address2);
        Assert.True(address1 != address2);
    }

    [Fact]
    public void Equals_WithDifferentPostalCode_ShouldReturnFalse()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345");
        var address2Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "67890");
        
        Assert.True(address1Result.IsSuccess);
        Assert.True(address2Result.IsSuccess);
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;

        // Act & Assert
        Assert.False(address1.Equals(address2));
        Assert.False(address1 == address2);
        Assert.True(address1 != address2);
    }

    [Fact]
    public void Equals_WithDifferentAddressLine2_ShouldReturnFalse()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345", "Apt 4B");
        var address2Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345", "Suite 100");
        
        Assert.True(address1Result.IsSuccess);
        Assert.True(address2Result.IsSuccess);
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;

        // Act & Assert
        Assert.False(address1.Equals(address2));
        Assert.False(address1 == address2);
        Assert.True(address1 != address2);
    }

    [Fact]
    public void Equals_WithNullAddressLine2_ShouldHandleComparisonCorrectly()
    {
        // Arrange
        var address1Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345");
        var address2Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345");
        var address3Result = PostalAddress.Create("123 Main St", "Anytown", "State", "Country", "12345", "Apt 4B");
        
        Assert.True(address1Result.IsSuccess);
        Assert.True(address2Result.IsSuccess);
        Assert.True(address3Result.IsSuccess);
        
        var address1 = address1Result.Value;
        var address2 = address2Result.Value;
        var address3 = address3Result.Value;

        // Act & Assert
        Assert.True(address1.Equals(address2));
        Assert.False(address1.Equals(address3));
    }
}