using Bogus;
using Shopilent.Domain.Shipping.Enums;

namespace Shopilent.API.IntegrationTests.Common.TestData;

/// <summary>
/// Unified test data factory for all Address endpoint operations.
/// Centralizes all address-related test data generation following the pattern established by AttributeTestDataV1.
/// </summary>
public static class AddressTestDataV1
{
    private static readonly Faker _faker = new();

    private static readonly string[] ValidStreetNames = { "Main Street", "Oak Avenue", "Park Boulevard", "Elm Drive", "Washington Street" };
    private static readonly string[] ValidCities = { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio" };
    private static readonly string[] ValidStates = { "California", "Texas", "New York", "Florida", "Illinois", "Pennsylvania", "Ohio" };
    private static readonly string[] ValidCountries = { "United States", "USA", "Canada", "United Kingdom", "Australia", "Germany", "France" };

    /// <summary>
    /// Core address creation methods for all test scenarios
    /// </summary>
    public static class Creation
    {
        /// <summary>
        /// Creates a valid address request with customizable parameters
        /// </summary>
        public static object CreateValidRequest(
            string? addressLine1 = null,
            string? addressLine2 = null,
            string? city = null,
            string? state = null,
            string? postalCode = null,
            string? country = null,
            string? phone = null,
            AddressType? addressType = null,
            bool? isDefault = null)
        {
            return new
            {
                AddressLine1 = addressLine1 ?? $"{_faker.Random.Int(100, 9999)} {_faker.Random.ArrayElement(ValidStreetNames)}",
                AddressLine2 = addressLine2 ?? (_faker.Random.Bool(0.3f) ? $"Apt {_faker.Random.Int(1, 999)}" : null),
                City = city ?? _faker.Random.ArrayElement(ValidCities),
                State = state ?? _faker.Random.ArrayElement(ValidStates),
                PostalCode = postalCode ?? _faker.Random.Int(10000, 99999).ToString(),
                Country = country ?? _faker.Random.ArrayElement(ValidCountries),
                Phone = phone ?? GenerateValidPhone(),
                AddressType = addressType ?? _faker.PickRandom<AddressType>(),
                IsDefault = isDefault ?? _faker.Random.Bool()
            };
        }

        /// <summary>
        /// Creates a shipping address request
        /// </summary>
        public static object CreateShippingAddressRequest(
            bool? isDefault = null)
        {
            return new
            {
                AddressLine1 = $"{_faker.Random.Int(100, 9999)} {_faker.Random.ArrayElement(ValidStreetNames)}",
                AddressLine2 = _faker.Random.Bool(0.3f) ? $"Suite {_faker.Random.Int(100, 999)}" : null,
                City = _faker.Random.ArrayElement(ValidCities),
                State = _faker.Random.ArrayElement(ValidStates),
                PostalCode = _faker.Random.Int(10000, 99999).ToString(),
                Country = _faker.Random.ArrayElement(ValidCountries),
                Phone = GenerateValidPhone(),
                AddressType = AddressType.Shipping,
                IsDefault = isDefault ?? false
            };
        }

        /// <summary>
        /// Creates a billing address request
        /// </summary>
        public static object CreateBillingAddressRequest(
            bool? isDefault = null)
        {
            return new
            {
                AddressLine1 = $"{_faker.Random.Int(100, 9999)} {_faker.Random.ArrayElement(ValidStreetNames)}",
                AddressLine2 = _faker.Random.Bool(0.3f) ? $"Apt {_faker.Random.Int(1, 999)}" : null,
                City = _faker.Random.ArrayElement(ValidCities),
                State = _faker.Random.ArrayElement(ValidStates),
                PostalCode = _faker.Random.Int(10000, 99999).ToString(),
                Country = _faker.Random.ArrayElement(ValidCountries),
                Phone = GenerateValidPhone(),
                AddressType = AddressType.Billing,
                IsDefault = isDefault ?? false
            };
        }

        /// <summary>
        /// Creates a 'Both' type address request
        /// </summary>
        public static object CreateBothAddressRequest(
            bool? isDefault = null)
        {
            return new
            {
                AddressLine1 = $"{_faker.Random.Int(100, 9999)} {_faker.Random.ArrayElement(ValidStreetNames)}",
                AddressLine2 = _faker.Random.Bool(0.3f) ? $"Unit {_faker.Random.Int(1, 500)}" : null,
                City = _faker.Random.ArrayElement(ValidCities),
                State = _faker.Random.ArrayElement(ValidStates),
                PostalCode = _faker.Random.Int(10000, 99999).ToString(),
                Country = _faker.Random.ArrayElement(ValidCountries),
                Phone = GenerateValidPhone(),
                AddressType = AddressType.Both,
                IsDefault = isDefault ?? true
            };
        }

        /// <summary>
        /// Creates an address without phone (optional field)
        /// </summary>
        public static object CreateAddressWithoutPhone()
        {
            return new
            {
                AddressLine1 = $"{_faker.Random.Int(100, 9999)} {_faker.Random.ArrayElement(ValidStreetNames)}",
                AddressLine2 = (string?)null,
                City = _faker.Random.ArrayElement(ValidCities),
                State = _faker.Random.ArrayElement(ValidStates),
                PostalCode = _faker.Random.Int(10000, 99999).ToString(),
                Country = _faker.Random.ArrayElement(ValidCountries),
                Phone = (string?)null,
                AddressType = AddressType.Shipping,
                IsDefault = false
            };
        }

        /// <summary>
        /// Creates an address without AddressLine2 (optional field)
        /// </summary>
        public static object CreateAddressWithoutLine2()
        {
            return new
            {
                AddressLine1 = $"{_faker.Random.Int(100, 9999)} {_faker.Random.ArrayElement(ValidStreetNames)}",
                AddressLine2 = (string?)null,
                City = _faker.Random.ArrayElement(ValidCities),
                State = _faker.Random.ArrayElement(ValidStates),
                PostalCode = _faker.Random.Int(10000, 99999).ToString(),
                Country = _faker.Random.ArrayElement(ValidCountries),
                Phone = GenerateValidPhone(),
                AddressType = AddressType.Billing,
                IsDefault = false
            };
        }

        /// <summary>
        /// Creates multiple addresses for bulk testing
        /// </summary>
        public static List<object> CreateMultipleAddresses(int count = 3)
        {
            var addresses = new List<object>();
            for (int i = 0; i < count; i++)
            {
                addresses.Add(CreateValidRequest(
                    addressLine1: $"{_faker.Random.Int(100, 9999)} Test Street {i + 1}",
                    city: _faker.Random.ArrayElement(ValidCities),
                    isDefault: i == 0
                ));
            }
            return addresses;
        }

        private static string GenerateValidPhone()
        {
            return _faker.Random.Bool(0.7f) ? _faker.Phone.PhoneNumber("+1##########") : _faker.Phone.PhoneNumber("##########");
        }
    }

    /// <summary>
    /// Validation test cases for various field validations
    /// </summary>
    public static class Validation
    {
        // AddressLine1 validation (required)
        public static object CreateRequestWithEmptyAddressLine1() => new
        {
            AddressLine1 = "",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithNullAddressLine1() => new
        {
            AddressLine1 = (string?)null,
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithWhitespaceAddressLine1() => new
        {
            AddressLine1 = "   ",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithLongAddressLine1() => new
        {
            AddressLine1 = new string('A', 256), // Exceeds 255 character limit
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        // AddressLine2 validation (optional)
        public static object CreateRequestWithLongAddressLine2() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = new string('B', 256), // Exceeds 255 character limit
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        // City validation (required)
        public static object CreateRequestWithEmptyCity() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = "",
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithNullCity() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = (string?)null,
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithLongCity() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = new string('C', 101), // Exceeds 100 character limit
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        // State validation (required)
        public static object CreateRequestWithEmptyState() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = "",
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithNullState() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = (string?)null,
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithLongState() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = new string('S', 101), // Exceeds 100 character limit
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        // PostalCode validation (required)
        public static object CreateRequestWithEmptyPostalCode() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = "",
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithNullPostalCode() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = (string?)null,
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithLongPostalCode() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = new string('1', 21), // Exceeds 20 character limit
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        // Country validation (required)
        public static object CreateRequestWithEmptyCountry() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = "",
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithNullCountry() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = (string?)null,
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithLongCountry() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = new string('C', 101), // Exceeds 100 character limit
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        // Phone validation (optional)
        public static object CreateRequestWithLongPhone() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = new string('1', 21), // Exceeds 20 character limit
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        // AddressType validation
        public static object CreateRequestWithInvalidAddressType() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = (AddressType)999, // Invalid enum value
            IsDefault = false
        };

        private static string GenerateValidPhone()
        {
            return _faker.Random.Bool(0.7f) ? _faker.Phone.PhoneNumber("+1##########") : _faker.Phone.PhoneNumber("##########");
        }
    }

    /// <summary>
    /// Boundary value testing for limits and edge values
    /// </summary>
    public static class BoundaryTests
    {
        public static object CreateRequestWithMaximumLengthAddressLine1() => new
        {
            AddressLine1 = new string('A', 255), // Exactly 255 characters
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithMaximumLengthAddressLine2() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = new string('B', 255), // Exactly 255 characters
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithMaximumLengthCity() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = new string('C', 100), // Exactly 100 characters
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithMaximumLengthState() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = new string('S', 100), // Exactly 100 characters
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithMaximumLengthPostalCode() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = new string('1', 20), // Exactly 20 characters
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithMaximumLengthCountry() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = new string('C', 100), // Exactly 100 characters
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithMaximumLengthPhone() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = new string('1', 20), // Exactly 20 characters
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithMinimumValidAddress() => new
        {
            AddressLine1 = "A", // Single character
            AddressLine2 = (string?)null,
            City = "B",
            State = "C",
            PostalCode = "1",
            Country = "D",
            Phone = (string?)null,
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        private static string GenerateValidPhone()
        {
            return _faker.Random.Bool(0.7f) ? _faker.Phone.PhoneNumber("+1##########") : _faker.Phone.PhoneNumber("##########");
        }
    }

    /// <summary>
    /// Edge cases for special scenarios
    /// </summary>
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodeCharacters() => new
        {
            AddressLine1 = "123 Café Münchën Street™",
            AddressLine2 = "Suite Ñoño 日本",
            City = "São Paulo",
            State = "Île-de-France",
            PostalCode = "75001",
            Country = "République Française",
            Phone = "+33123456789",
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithSpecialCharacters() => new
        {
            AddressLine1 = "123 Main St. #456",
            AddressLine2 = "Apt. 7B - Building C",
            City = "St. John's",
            State = "Prince Edward Island",
            PostalCode = "A1A-1A1",
            Country = "Canada",
            Phone = "+1-234-567-8900",
            AddressType = AddressType.Both,
            IsDefault = false
        };

        public static object CreateRequestWithNumericAddress() => new
        {
            AddressLine1 = "123456",
            AddressLine2 = "789",
            City = "12345",
            State = "67890",
            PostalCode = "11111",
            Country = "22222",
            Phone = "1234567890",
            AddressType = AddressType.Billing,
            IsDefault = false
        };

        public static object CreateRequestWithWhitespaceInFields() => new
        {
            AddressLine1 = "  123 Main Street  ",
            AddressLine2 = "  Apt 456  ",
            City = "  New York  ",
            State = "  New York  ",
            PostalCode = "  10001  ",
            Country = "  USA  ",
            Phone = "  1234567890  ",
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateInternationalAddress() => new
        {
            AddressLine1 = "1-2-3 Shibuya",
            AddressLine2 = "Shibuya-ku",
            City = "Tokyo",
            State = "Tokyo",
            PostalCode = "150-0002",
            Country = "Japan",
            Phone = "+81312345678",
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateUKAddress() => new
        {
            AddressLine1 = "10 Downing Street",
            AddressLine2 = (string?)null,
            City = "London",
            State = "England",
            PostalCode = "SW1A 2AA",
            Country = "United Kingdom",
            Phone = "+442079250918",
            AddressType = AddressType.Both,
            IsDefault = true
        };

        public static object CreateCanadianAddress() => new
        {
            AddressLine1 = "123 Maple Avenue",
            AddressLine2 = "Suite 456",
            City = "Toronto",
            State = "Ontario",
            PostalCode = "M5H 2N2",
            Country = "Canada",
            Phone = "+14165551234",
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithEmptyAddressLine2() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = "",
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = GenerateValidPhone(),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateRequestWithEmptyPhone() => new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = (string?)null,
            City = _faker.Random.ArrayElement(ValidCities),
            State = _faker.Random.ArrayElement(ValidStates),
            PostalCode = _faker.Random.Int(10000, 99999).ToString(),
            Country = _faker.Random.ArrayElement(ValidCountries),
            Phone = "",
            AddressType = AddressType.Billing,
            IsDefault = false
        };

        private static string GenerateValidPhone()
        {
            return _faker.Random.Bool(0.7f) ? _faker.Phone.PhoneNumber("+1##########") : _faker.Phone.PhoneNumber("##########");
        }
    }

    /// <summary>
    /// Address type specific test scenarios
    /// </summary>
    public static class TypeSpecific
    {
        public static object CreateShippingAddress() => Creation.CreateShippingAddressRequest();
        public static object CreateBillingAddress() => Creation.CreateBillingAddressRequest();
        public static object CreateBothAddress() => Creation.CreateBothAddressRequest();

        public static List<object> CreateAllAddressTypes()
        {
            return new List<object>
            {
                Creation.CreateShippingAddressRequest(false),
                Creation.CreateBillingAddressRequest(false),
                Creation.CreateBothAddressRequest(false)
            };
        }
    }

    /// <summary>
    /// Default address management scenarios
    /// </summary>
    public static class DefaultManagement
    {
        public static object CreateDefaultShippingAddress() => Creation.CreateShippingAddressRequest(isDefault: true);
        public static object CreateDefaultBillingAddress() => Creation.CreateBillingAddressRequest(isDefault: true);
        public static object CreateNonDefaultAddress() => Creation.CreateValidRequest(addressType: AddressType.Billing, isDefault: false);

        public static List<object> CreateMultipleDefaultAddresses(AddressType addressType, int count = 3)
        {
            var addresses = new List<object>();
            for (int i = 0; i < count; i++)
            {
                addresses.Add(Creation.CreateValidRequest(
                    addressLine1: $"Address {i + 1}, Main Street",
                    addressType: addressType,
                    isDefault: true // All marked as default for testing conflict resolution
                ));
            }
            return addresses;
        }
    }

    /// <summary>
    /// Security test scenarios
    /// </summary>
    public static class SecurityTests
    {
        public static object CreateSqlInjectionAttempt() => new
        {
            AddressLine1 = "'; DROP TABLE Addresses; --",
            AddressLine2 = "'; SELECT * FROM Users; --",
            City = "'; UPDATE Users SET Role = 'Admin'; --",
            State = "California'; --",
            PostalCode = "12345'; --",
            Country = "USA'; DROP TABLE Orders; --",
            Phone = "+1234567890'; --",
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateXssAttempt() => new
        {
            AddressLine1 = "<script>alert('xss')</script>",
            AddressLine2 = "<img src=x onerror=alert('xss')>",
            City = "javascript:alert('xss')",
            State = "<iframe src='malicious.com'></iframe>",
            PostalCode = "<svg onload=alert('xss')>",
            Country = "<body onload=alert('xss')>",
            Phone = "+1234567890",
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateCommandInjectionAttempt() => new
        {
            AddressLine1 = "123 Main St; rm -rf /",
            AddressLine2 = "Apt 456 | cat /etc/passwd",
            City = "New York && whoami",
            State = "NY $(cat /etc/shadow)",
            PostalCode = "10001 `ls -la`",
            Country = "USA; shutdown -h now",
            Phone = "+1234567890",
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateLongStringAttack() => new
        {
            AddressLine1 = new string('A', 10000),
            AddressLine2 = new string('B', 10000),
            City = new string('C', 10000),
            State = new string('S', 10000),
            PostalCode = new string('1', 10000),
            Country = new string('D', 10000),
            Phone = new string('9', 10000),
            AddressType = AddressType.Shipping,
            IsDefault = false
        };

        public static object CreateNullByteAttempt() => new
        {
            AddressLine1 = "123 Main St\0Admin",
            AddressLine2 = "Apt 456\0Root",
            City = "New York\0System",
            State = "NY\0Admin",
            PostalCode = "10001\0000",
            Country = "USA\0World",
            Phone = "+1234567890\0",
            AddressType = AddressType.Shipping,
            IsDefault = false
        };
    }

    /// <summary>
    /// Helper method for phone generation
    /// </summary>
    private static string GenerateValidPhone()
    {
        return _faker.Random.Bool(0.7f) ? _faker.Phone.PhoneNumber("+1##########") : _faker.Phone.PhoneNumber("##########");
    }
}
