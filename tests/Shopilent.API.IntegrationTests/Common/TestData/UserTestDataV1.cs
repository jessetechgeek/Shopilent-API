using Bogus;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.IntegrationTests.Common.TestData;

/// <summary>
/// Unified test data factory for all Users endpoint operations.
/// Centralizes all user-related test data generation following the pattern established by AttributeTestDataV1.
/// </summary>
public static class UserTestDataV1
{
    private static readonly Faker _faker = new();

    // Valid names that comply with validation pattern ^[a-zA-Z\s\-'\.]+$
    private static readonly string[] ValidFirstNames = { "John", "Jane", "Michael", "Sarah", "David", "Emily", "Robert", "Lisa", "Mary", "James" };
    private static readonly string[] ValidLastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Wilson", "Moore" };
    private static readonly string[] ValidMiddleNames = { "James", "Marie", "Ann", "Lee", "Rose", "Grace", "Paul", "Lynn", "Thomas", "Elizabeth" };

    /// <summary>
    /// Core user creation and management methods
    /// </summary>
    public static class Creation
    {
        /// <summary>
        /// Creates a valid user creation request with customizable parameters
        /// </summary>
        public static object CreateValidUserData(
            string? email = null,
            string? firstName = null,
            string? lastName = null,
            string? middleName = null,
            string? phone = null,
            UserRole role = UserRole.Customer,
            bool isActive = true,
            bool emailVerified = true)
        {
            return new
            {
                Email = email ?? _faker.Internet.Email(),
                Password = "Password123!",
                FirstName = firstName ?? _faker.Random.ArrayElement(ValidFirstNames),
                LastName = lastName ?? _faker.Random.ArrayElement(ValidLastNames),
                MiddleName = middleName ?? (_faker.Random.Bool(0.3f) ? _faker.Random.ArrayElement(ValidMiddleNames) : null),
                Phone = phone ?? GenerateValidPhone(),
                Role = role,
                IsActive = isActive,
                EmailVerified = emailVerified
            };
        }

        /// <summary>
        /// Creates a valid profile update request
        /// </summary>
        public static object CreateValidProfileUpdateRequest(
            string? firstName = null,
            string? lastName = null,
            string? middleName = null,
            string? phone = null)
        {
            return new
            {
                FirstName = firstName ?? _faker.Random.ArrayElement(ValidFirstNames),
                LastName = lastName ?? _faker.Random.ArrayElement(ValidLastNames),
                MiddleName = middleName ?? (_faker.Random.Bool(0.3f) ? _faker.Random.ArrayElement(ValidMiddleNames) : null),
                Phone = phone ?? GenerateValidPhone()
            };
        }

        /// <summary>
        /// Creates a valid user update request
        /// </summary>
        public static object CreateValidUserUpdateRequest(
            string? firstName = null,
            string? lastName = null,
            string? middleName = null,
            string? phone = null)
        {
            return new
            {
                FirstName = firstName ?? _faker.Random.ArrayElement(ValidFirstNames),
                LastName = lastName ?? _faker.Random.ArrayElement(ValidLastNames),
                MiddleName = middleName,
                Phone = phone
            };
        }

        /// <summary>
        /// Creates predictable user IDs for testing
        /// </summary>
        public static Guid CreateExistingAdminUserId() => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static Guid CreateExistingCustomerUserId() => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static Guid CreateValidUserId() => Guid.NewGuid();

        public static string GenerateValidPhone()
        {
            return _faker.Random.Bool(0.7f) ? _faker.Phone.PhoneNumber("+1##########") : "1234567890";
        }
    }

    /// <summary>
    /// Password-related test data
    /// </summary>
    public static class PasswordScenarios
    {
        /// <summary>
        /// Creates a valid password change request
        /// </summary>
        public static object CreateValidChangePasswordRequest(
            string? currentPassword = null,
            string? newPassword = null,
            string? confirmPassword = null)
        {
            var validNewPassword = newPassword ?? GenerateValidPassword();
            return new
            {
                CurrentPassword = currentPassword ?? "Customer123!",
                NewPassword = validNewPassword,
                ConfirmPassword = confirmPassword ?? validNewPassword
            };
        }

        public static object CreatePasswordMismatchRequest() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        public static object CreateSamePasswordRequest() => new
        {
            CurrentPassword = "SamePassword123!",
            NewPassword = "SamePassword123!",
            ConfirmPassword = "SamePassword123!"
        };

        public static string GenerateValidPassword()
        {
            var upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            var lowerChars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            var digits = "0123456789".ToCharArray();
            var special = "!@#$%^&*()_+-=[]{}|;:,.<>?".ToCharArray();

            var password = "";
            password += _faker.Random.ArrayElement(upperChars); // At least one uppercase
            password += _faker.Random.ArrayElement(lowerChars); // At least one lowercase
            password += _faker.Random.ArrayElement(digits);     // At least one digit
            password += _faker.Random.ArrayElement(special);    // At least one special char

            // Fill remaining length with random valid characters
            var allChars = upperChars.Concat(lowerChars).Concat(digits).Concat(special).ToArray();
            var remainingLength = _faker.Random.Int(4, 8); // Make it 8-12 chars total
            for (int i = 0; i < remainingLength; i++)
            {
                password += _faker.Random.ArrayElement(allChars);
            }

            // Shuffle the password to avoid predictable patterns
            return new string(password.OrderBy(x => _faker.Random.Int()).ToArray());
        }

        // Additional password request methods
        public static object CreateRequestWithEmptyCurrentPassword() => new
        {
            CurrentPassword = "",
            NewPassword = GenerateValidPassword(),
            ConfirmPassword = GenerateValidPassword()
        };

        public static object CreateRequestWithNullCurrentPassword() => new
        {
            CurrentPassword = (string?)null,
            NewPassword = GenerateValidPassword(),
            ConfirmPassword = GenerateValidPassword()
        };

        public static object CreateRequestWithEmptyNewPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "",
            ConfirmPassword = ""
        };

        public static object CreateRequestWithNullNewPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = (string?)null,
            ConfirmPassword = (string?)null
        };

        public static object CreateRequestWithEmptyConfirmPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = GenerateValidPassword(),
            ConfirmPassword = ""
        };

        public static object CreateRequestWithNullConfirmPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = GenerateValidPassword(),
            ConfirmPassword = (string?)null
        };

        public static object CreateRequestWithMismatchedPasswords() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        public static object CreateRequestWithSameCurrentAndNewPassword() => new
        {
            CurrentPassword = "SamePassword123!",
            NewPassword = "SamePassword123!",
            ConfirmPassword = "SamePassword123!"
        };

        public static object CreateRequestWithShortNewPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "Short1!",
            ConfirmPassword = "Short1!"
        };

        public static object CreateRequestWithWeakNewPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "weakpassword",
            ConfirmPassword = "weakpassword"
        };

        public static object CreateValidRequest() => CreateValidChangePasswordRequest();

        /// <summary>
        /// Password strength specific tests
        /// </summary>
        public static class PasswordStrengthTests
        {
            public static object CreateRequestWithNoUppercaseNewPassword() => new
            {
                CurrentPassword = "Customer123!",
                NewPassword = "newpassword123!",
                ConfirmPassword = "newpassword123!"
            };

            public static object CreateRequestWithNoLowercaseNewPassword() => new
            {
                CurrentPassword = "Customer123!",
                NewPassword = "NEWPASSWORD123!",
                ConfirmPassword = "NEWPASSWORD123!"
            };

            public static object CreateRequestWithNoNumberNewPassword() => new
            {
                CurrentPassword = "Customer123!",
                NewPassword = "NewPassword!",
                ConfirmPassword = "NewPassword!"
            };

            public static object CreateRequestWithNoSpecialCharNewPassword() => new
            {
                CurrentPassword = "Customer123!",
                NewPassword = "NewPassword123",
                ConfirmPassword = "NewPassword123"
            };
        }

        /// <summary>
        /// Boundary tests for password validation
        /// </summary>
        public static class BoundaryTests
        {
            public static object CreateRequestWithMinimumValidPassword() => new
            {
                CurrentPassword = "Customer123!",
                NewPassword = "MinVal1!",  // 8 characters (minimum)
                ConfirmPassword = "MinVal1!"
            };

            public static object CreateRequestWithSevenCharacterPassword() => new
            {
                CurrentPassword = "Customer123!",
                NewPassword = "Short1!",  // 7 characters (below minimum)
                ConfirmPassword = "Short1!"
            };

            public static object CreateRequestWithVeryLongPassword() => new
            {
                CurrentPassword = "Customer123!",
                NewPassword = TestHelpers.GeneratePasswordWithLength(128),
                ConfirmPassword = TestHelpers.GeneratePasswordWithLength(128)
            };

            public static object CreateRequestWithExtremelyLongPassword() => new
            {
                CurrentPassword = "Customer123!",
                NewPassword = new string('A', 1000) + "1!",  // Extremely long password
                ConfirmPassword = new string('A', 1000) + "1!"
            };
        }

        /// <summary>
        /// Edge cases for password testing
        /// </summary>
        public static class EdgeCases
        {
            public static object CreateRequestWithUnicodeCharacters() => new
            {
                CurrentPassword = "Customer123!",
                NewPassword = "Pássw0rd123!ñ",
                ConfirmPassword = "Pássw0rd123!ñ"
            };

            public static object CreateRequestWithWhitespaceInPasswords() => new
            {
                CurrentPassword = "  Customer123!  ",
                NewPassword = "  NewPassword123!  ",
                ConfirmPassword = "  NewPassword123!  "
            };

            public static object CreateRequestWithOnlyWhitespace() => new
            {
                CurrentPassword = "Customer123!",
                NewPassword = "   ",
                ConfirmPassword = "   "
            };

            public static object CreateRequestWithTabsAndNewlines() => new
            {
                CurrentPassword = "Customer123!",
                NewPassword = "NewPassword\t123!\n",
                ConfirmPassword = "NewPassword\t123!\n"
            };
        }

        /// <summary>
        /// Security tests for password operations
        /// </summary>
        public static class SecurityTests
        {
            public static object CreateSqlInjectionAttempt() => new
            {
                CurrentPassword = "'; DROP TABLE Users; --",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            public static object CreateXssAttempt() => new
            {
                CurrentPassword = "<script>alert('xss')</script>",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            public static object CreateCommandInjectionAttempt() => new
            {
                CurrentPassword = "Customer123!; rm -rf /",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            public static object CreateLongPasswordAttack() => new
            {
                CurrentPassword = "Customer123!",
                NewPassword = new string('A', 10000),
                ConfirmPassword = new string('A', 10000)
            };

            public static object CreateNullByteAttempt() => new
            {
                CurrentPassword = "Customer123!\0Admin",
                NewPassword = "NewPassword123!\0",
                ConfirmPassword = "NewPassword123!\0"
            };
        }
    }

    /// <summary>
    /// Role management test data
    /// </summary>
    public static class RoleScenarios
    {
        public static object CreateValidRoleChangeRequest(UserRole? role = null) => new
        {
            Role = role ?? _faker.Random.Enum<UserRole>()
        };

        public static object CreateAdminRoleRequest() => new { Role = UserRole.Admin };
        public static object CreateManagerRoleRequest() => new { Role = UserRole.Manager };
        public static object CreateCustomerRoleRequest() => new { Role = UserRole.Customer };
        public static object CreateInvalidRoleRequest() => new { Role = 999 }; // Invalid enum value

        public static UserRole GetRandomValidRole() => _faker.Random.Enum<UserRole>();
        public static UserRole GetDifferentRole(UserRole currentRole)
        {
            var allRoles = Enum.GetValues<UserRole>();
            var availableRoles = allRoles.Where(r => r != currentRole).ToArray();
            return _faker.Random.ArrayElement(availableRoles);
        }

        /// <summary>
        /// Edge cases for role testing
        /// </summary>
        public static class EdgeCases
        {
            public static object CreateRequestWithStringRole() => new { Role = "InvalidString" };
            public static object CreateRequestWithNegativeRole() => new { Role = -1 };
            public static object CreateRequestWithLargeRole() => new { Role = int.MaxValue };
        }

        /// <summary>
        /// Security tests for role operations
        /// </summary>
        public static class SecurityTests
        {
            public static object CreateSqlInjectionAttempt() => new { Role = "'; DROP TABLE Users; --" };
            public static object CreateXssAttempt() => new { Role = "<script>alert('xss')</script>" };
            public static object CreateCommandInjectionAttempt() => new { Role = "$(rm -rf /)" };
        }
    }

    /// <summary>
    /// Status management test data
    /// </summary>
    public static class StatusScenarios
    {
        public static object CreateValidStatusRequest(bool? isActive = null) => new
        {
            IsActive = isActive ?? _faker.Random.Bool()
        };

        public static object CreateActivateRequest() => new { IsActive = true };
        public static object CreateDeactivateRequest() => new { IsActive = false };
        public static object CreateValidRequest(bool? isActive = null) => CreateValidStatusRequest(isActive);

        public static bool GetOppositeBool(bool currentStatus) => !currentStatus;

        /// <summary>
        /// Edge cases for status testing
        /// </summary>
        public static class EdgeCases
        {
            public static string CreateMalformedJsonRequest() => "{ \"IsActive\": }"; // Missing value
        }

        /// <summary>
        /// Concurrency tests for status operations
        /// </summary>
        public static class ConcurrencyTests
        {
            public static List<object> CreateConcurrencyTestRequests()
            {
                return new List<object>
                {
                    CreateActivateRequest(),   // First: Activate
                    CreateDeactivateRequest(), // Second: Deactivate
                    CreateActivateRequest()    // Third: Activate again
                };
            }
        }

        /// <summary>
        /// Boundary tests for status operations
        /// </summary>
        public static class BoundaryTests
        {
            public static readonly object[] ValidBooleanValues =
            {
                new { IsActive = true },
                new { IsActive = false }
            };

            public static readonly object[] InvalidBooleanValues =
            {
                new { IsActive = "true" },      // String representation should fail
                new { IsActive = "false" },     // String representation should fail
                new { IsActive = 1 },           // Numeric representation should fail
                new { IsActive = 0 },           // Numeric representation should fail
                new { IsActive = "maybe" },     // Invalid string
                new { IsActive = "" }           // Empty string
            };
        }

        /// <summary>
        /// Security tests for status operations
        /// </summary>
        public static class SecurityTests
        {
            public static object CreateSqlInjectionAttempt() => new { IsActive = "'; DROP TABLE Users; --" };
            public static object CreateXssAttempt() => new { IsActive = "<script>alert('xss')</script>" };
            public static object CreateCommandInjectionAttempt() => new { IsActive = "$(rm -rf /)" };
            public static object CreateLdapInjectionAttempt() => new { IsActive = "admin)(|(password=*))" };
            public static object CreateNoSqlInjectionAttempt() => new { IsActive = "'; return db.users.drop(); //" };
            public static object CreatePathTraversalAttempt() => new { IsActive = "../../etc/passwd" };
            public static object CreateUnicodeAttempt() => new { IsActive = "𝕿𝖊𝖘𝖙 🔥 𝔘𝔫𝔦𝔠𝔬𝔡𝔢" };
        }
    }

    /// <summary>
    /// Validation test cases for various field validations
    /// </summary>
    public static class Validation
    {
        // FirstName validation
        public static object CreateRequestWithEmptyFirstName() => new
        {
            FirstName = "",
            LastName = _faker.Random.ArrayElement(ValidLastNames),
            MiddleName = _faker.Random.ArrayElement(ValidMiddleNames),
            Phone = TestHelpers.GenerateValidPhone()
        };

        public static object CreateRequestWithNullFirstName() => new
        {
            FirstName = (string?)null,
            LastName = _faker.Random.ArrayElement(ValidLastNames),
            MiddleName = _faker.Random.ArrayElement(ValidMiddleNames),
            Phone = TestHelpers.GenerateValidPhone()
        };

        // LastName validation
        public static object CreateRequestWithEmptyLastName() => new
        {
            FirstName = _faker.Random.ArrayElement(ValidFirstNames),
            LastName = "",
            MiddleName = _faker.Random.ArrayElement(ValidMiddleNames),
            Phone = TestHelpers.GenerateValidPhone()
        };

        public static object CreateRequestWithNullLastName() => new
        {
            FirstName = _faker.Random.ArrayElement(ValidFirstNames),
            LastName = (string?)null,
            MiddleName = _faker.Random.ArrayElement(ValidMiddleNames),
            Phone = TestHelpers.GenerateValidPhone()
        };

        // MiddleName validation (optional field)
        public static object CreateRequestWithNullMiddleName() => new
        {
            FirstName = _faker.Random.ArrayElement(ValidFirstNames),
            LastName = _faker.Random.ArrayElement(ValidLastNames),
            MiddleName = (string?)null,
            Phone = TestHelpers.GenerateValidPhone()
        };

        public static object CreateRequestWithEmptyMiddleName() => new
        {
            FirstName = _faker.Random.ArrayElement(ValidFirstNames),
            LastName = _faker.Random.ArrayElement(ValidLastNames),
            MiddleName = "",
            Phone = TestHelpers.GenerateValidPhone()
        };

        // Phone validation
        public static object CreateRequestWithNullPhone() => new
        {
            FirstName = _faker.Random.ArrayElement(ValidFirstNames),
            LastName = _faker.Random.ArrayElement(ValidLastNames),
            MiddleName = _faker.Random.ArrayElement(ValidMiddleNames),
            Phone = (string?)null
        };

        public static object CreateRequestWithEmptyPhone() => new
        {
            FirstName = _faker.Random.ArrayElement(ValidFirstNames),
            LastName = _faker.Random.ArrayElement(ValidLastNames),
            MiddleName = _faker.Random.ArrayElement(ValidMiddleNames),
            Phone = ""
        };

        public static object CreateRequestWithInvalidPhone() => new
        {
            FirstName = _faker.Random.ArrayElement(ValidFirstNames),
            LastName = _faker.Random.ArrayElement(ValidLastNames),
            MiddleName = _faker.Random.ArrayElement(ValidMiddleNames),
            Phone = "invalid-phone"
        };

        public static object CreateRequestWithValidInternationalPhone() => new
        {
            FirstName = _faker.Random.ArrayElement(ValidFirstNames),
            LastName = _faker.Random.ArrayElement(ValidLastNames),
            MiddleName = _faker.Random.ArrayElement(ValidMiddleNames),
            Phone = "+1234567890123"
        };

        public static object CreateRequestWithInvalidCharactersInName() => new
        {
            FirstName = "John123@",
            LastName = "Doe456#",
            MiddleName = "Test$%",
            Phone = "1234567890"
        };

        // Password validation
        public static object CreatePasswordRequestWithEmptyCurrentPassword() => new
        {
            CurrentPassword = "",
            NewPassword = PasswordScenarios.GenerateValidPassword(),
            ConfirmPassword = PasswordScenarios.GenerateValidPassword()
        };

        public static object CreatePasswordRequestWithNullCurrentPassword() => new
        {
            CurrentPassword = (string?)null,
            NewPassword = PasswordScenarios.GenerateValidPassword(),
            ConfirmPassword = PasswordScenarios.GenerateValidPassword()
        };

        public static object CreatePasswordRequestWithShortNewPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "Short1!",
            ConfirmPassword = "Short1!"
        };

        public static object CreatePasswordRequestWithWeakNewPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "weakpassword",
            ConfirmPassword = "weakpassword"
        };

        // Role validation
        public static object CreateNullRoleRequest() => new { Role = (UserRole?)null };

        // Status validation
        public static object CreateNullStatusRequest() => new { IsActive = (bool?)null };

        public static string GenerateValidPhone()
        {
            return _faker.Random.Bool(0.7f) ? _faker.Phone.PhoneNumber("+1##########") : "1234567890";
        }
    }

    /// <summary>
    /// Boundary value testing for limits and edge values
    /// </summary>
    public static class BoundaryTests
    {
        public static object CreateRequestWithMaximumLengthNames() => new
        {
            FirstName = new string('A', 50), // Maximum valid length
            LastName = new string('B', 50),
            MiddleName = new string('C', 50),
            Phone = "+1234567890"
        };

        public static object CreateRequestWithTooLongNames() => new
        {
            FirstName = new string('A', 51), // Exceeds maximum
            LastName = new string('B', 51),
            MiddleName = new string('C', 51),
            Phone = "+1234567890"
        };

        public static object CreateRequestWithSingleCharacterNames() => new
        {
            FirstName = "A",
            LastName = "B",
            MiddleName = "C",
            Phone = "+1234567890"
        };

        public static object CreateRequestWithSingleCharacterName() => new
        {
            FirstName = "A",
            LastName = "B",
            MiddleName = "C",
            Phone = "+1234567890"
        };

        public static object CreateRequestWithLongNames() => new
        {
            FirstName = new string('A', 101), // Exceeds maximum length
            LastName = new string('B', 101),
            MiddleName = new string('C', 101),
            Phone = "+1234567890"
        };

        public static object CreateRequestWithMinimumValidPhone() => new
        {
            FirstName = _faker.Random.ArrayElement(ValidFirstNames),
            LastName = _faker.Random.ArrayElement(ValidLastNames),
            MiddleName = _faker.Random.ArrayElement(ValidMiddleNames),
            Phone = "+1234567" // Minimum valid: 7+ total characters
        };

        public static object CreateRequestWithMaximumValidPhone() => new
        {
            FirstName = _faker.Random.ArrayElement(ValidFirstNames),
            LastName = _faker.Random.ArrayElement(ValidLastNames),
            MiddleName = _faker.Random.ArrayElement(ValidMiddleNames),
            Phone = "+123456789012345" // Maximum valid: 15 digits total
        };

        // Password boundary tests
        public static object CreatePasswordRequestWithMinimumValidPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "MinVal1!",  // 8 characters (minimum)
            ConfirmPassword = "MinVal1!"
        };

        public static object CreatePasswordRequestWithSevenCharacterPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "Short1!",  // 7 characters (below minimum)
            ConfirmPassword = "Short1!"
        };
    }

    /// <summary>
    /// Edge cases for special scenarios
    /// </summary>
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodeCharacters() => new
        {
            FirstName = "José",
            LastName = "García-Martínez",
            MiddleName = "María",
            Phone = "+34123456789"
        };

        public static object CreateRequestWithSpecialCharacters() => new
        {
            FirstName = "John-Paul",
            LastName = "O'Connor",
            MiddleName = "De'Angelo",
            Phone = "+1234567890"
        };

        // UpdateUser specific methods (different expectations than UpdateProfile)
        public static object CreateUpdateUserRequestWithSpecialCharacters() => new
        {
            FirstName = "Mary-Jane",
            LastName = "O'Connor",
            MiddleName = "D'Angelo",
            Phone = "+1234567890"
        };

        public static object CreateUpdateUserRequestWithUnicodeCharacters() => new
        {
            FirstName = "Café",
            LastName = "Münchën",
            MiddleName = "José-María",
            Phone = "+34123456789"
        };

        public static object CreateRequestWithWhitespaceNames() => new
        {
            FirstName = "  John  ",
            LastName = "  Doe  ",
            MiddleName = "  Middle  ",
            Phone = "+1234567890"
        };

        public static object CreateRequestWithInvalidCharactersInName() => new
        {
            FirstName = "John123@",
            LastName = "Doe456#",
            MiddleName = "Test$%",
            Phone = "+1234567890"
        };

        public static object CreateRequestWithValidSpecialCharacters() => new
        {
            FirstName = "Mary-Jane",
            LastName = "O'Connor",
            MiddleName = "Ann-Marie",
            Phone = "+1234567890"
        };

        public static object CreateRequestWithValidDotsAndSpaces() => new
        {
            FirstName = "John Jr.",
            LastName = "Van Der Berg",
            MiddleName = "De La Cruz",
            Phone = "+1234567890"
        };

        public static object CreateRequestWithEmojis() => new
        {
            FirstName = "John😊",
            LastName = "Doe🎉",
            MiddleName = "Test👍",
            Phone = "+1234567890"
        };

        // Status edge cases
        public static object CreateStatusRequestWithStringTrue() => new { IsActive = "true" };
        public static object CreateStatusRequestWithStringFalse() => new { IsActive = "false" };
        public static object CreateStatusRequestWithNumericTrue() => new { IsActive = 1 };
        public static object CreateStatusRequestWithNumericFalse() => new { IsActive = 0 };

        // Role edge cases
        public static object CreateRoleRequestWithStringRole() => new { Role = "InvalidString" };
        public static object CreateRoleRequestWithNegativeRole() => new { Role = -1 };
        public static object CreateRoleRequestWithLargeRole() => new { Role = int.MaxValue };

        public static object CreateRequestWithWhitespaceInNames() => new
        {
            FirstName = " John ",
            LastName = " Doe ",
            MiddleName = " Middle ",
            Phone = "+1234567890"
        };

        public static object CreateRequestWithOnlyWhitespace() => new
        {
            FirstName = "   ",
            LastName = "   ",
            MiddleName = "   ",
            Phone = "+1234567890"
        };

        public static object CreateRequestWithTabsAndNewlines() => new
        {
            FirstName = "John\tTab",
            LastName = "Doe\nNewline",
            MiddleName = "Test\r\nCRLF",
            Phone = "+1234567890"
        };

        public static object CreateRequestWithNumericNames() => new
        {
            FirstName = "123",
            LastName = "456",
            MiddleName = "789",
            Phone = "+1234567890"
        };

        // NonExistent GUIDs
        public static Guid CreateNonExistentUserId() => Guid.Parse("99999999-9999-9999-9999-999999999999");
        public static Guid CreateInvalidUserId() => Guid.Empty;
        public static string CreateMalformedUserId() => "not-a-guid";
    }

    /// <summary>
    /// Security test scenarios
    /// </summary>
    public static class SecurityTests
    {
        public static object CreateSqlInjectionAttempt() => new
        {
            FirstName = "'; DROP TABLE Users; --",
            LastName = "'; SELECT * FROM Users; --",
            MiddleName = "'; UPDATE Users SET Role = 'Admin'; --",
            Phone = "+1234567890"
        };

        public static object CreateXssAttempt() => new
        {
            FirstName = "<script>alert('xss')</script>",
            LastName = "<img src=x onerror=alert('xss')>",
            MiddleName = "javascript:alert('xss')",
            Phone = "+1234567890"
        };

        public static object CreateCommandInjectionAttempt() => new
        {
            FirstName = "John; rm -rf /",
            LastName = "Doe | cat /etc/passwd",
            MiddleName = "Test && whoami",
            Phone = "+1234567890"
        };

        public static object CreateLongStringAttack() => new
        {
            FirstName = new string('A', 10000),
            LastName = new string('B', 10000),
            MiddleName = new string('C', 10000),
            Phone = new string('1', 50)
        };

        public static object CreateNullByteAttempt() => new
        {
            FirstName = "John\0Admin",
            LastName = "Doe\0Root",
            MiddleName = "Test\0System",
            Phone = "+1234567890"
        };

        // Password security tests
        public static object CreatePasswordSqlInjectionAttempt() => new
        {
            CurrentPassword = "'; DROP TABLE Users; --",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        public static object CreatePasswordXssAttempt() => new
        {
            CurrentPassword = "<script>alert('xss')</script>",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Status security tests
        public static object CreateStatusSqlInjectionAttempt() => new { IsActive = "'; DROP TABLE Users; --" };
        public static object CreateStatusXssAttempt() => new { IsActive = "<script>alert('xss')</script>" };

        // Role security tests
        public static object CreateRoleSqlInjectionAttempt() => new { Role = "'; DROP TABLE Users; --" };
        public static object CreateRoleXssAttempt() => new { Role = "<script>alert('xss')</script>" };
    }

    /// <summary>
    /// Test helper methods and GUIDs
    /// </summary>
    public static class TestHelpers
    {
        public static Guid CreateDeletedUserId() => Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        public static Guid CreateMinimumValidGuid() => new Guid("00000000-0000-0000-0000-000000000001");
        public static Guid CreateMaximumValidGuid() => new Guid("ffffffff-ffff-ffff-ffff-fffffffffffe");

        public static string GetRoleName(UserRole role) => role switch
        {
            UserRole.Admin => "Admin",
            UserRole.Manager => "Manager",
            UserRole.Customer => "Customer",
            _ => "Unknown"
        };

        public static string GetStatusMessage(bool isActive) => isActive ? "active" : "inactive";

        public static string GenerateValidPhone()
        {
            return _faker.Random.Bool(0.7f) ? _faker.Phone.PhoneNumber("+1##########") : "1234567890";
        }

        public static string GeneratePasswordWithLength(int length)
        {
            if (length < 8) return "Short1!";

            // Ensure we meet all requirements and then fill to desired length
            var basePassword = "Aa1!";
            var remainingLength = length - basePassword.Length;
            var filler = string.Join("", Enumerable.Repeat("a", remainingLength));
            return basePassword + filler;
        }
    }
}
