using System.Diagnostics;
using Bogus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Configuration;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.Enums;
using Shopilent.Domain.Identity.Repositories.Write;
using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Seeding.Seeders;

public class UserSeeder : IDataSeeder
{
    private readonly IUserWriteRepository _userRepository;
    private readonly ILogger<UserSeeder> _logger;
    private readonly SeedingSettings _options;

    // Default password: "P@55word"
    private const string DefaultPasswordHash = "3HHQJ37fhE711b1+6VG7VOX89oqSvKKUxg4kqajFC1o/bVyCKCEuq2Snza/cH6W1";
    private const string DefaultAdminEmail = "admin@shopilent.com";
    private const int BatchSize = 500;

    public string SeederName => "User Seeder";
    public int Order => 1;

    public UserSeeder(
        IUserWriteRepository userRepository,
        ILogger<UserSeeder> logger,
        IOptions<SeedingSettings> options)
    {
        _userRepository = userRepository;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<SeedingResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var totalUsers = 0;

        try
        {
            _logger.LogInformation("Starting user seeding...");
            _logger.LogInformation("Target counts - Admins: {AdminCount}, Managers: {ManagerCount}, Customers: {CustomerCount}",
                _options.Counts.Admins, _options.Counts.Managers, _options.Counts.Customers);

            // Seed Admins
            _logger.LogInformation("Seeding {Count} admin users...", _options.Counts.Admins);
            var adminCount = await SeedUsersByRole(UserRole.Admin, _options.Counts.Admins, DefaultPasswordHash, cancellationToken);
            totalUsers += adminCount;
            _logger.LogInformation("Completed seeding {Count} admin users", adminCount);

            // Seed Managers
            _logger.LogInformation("Seeding {Count} manager users...", _options.Counts.Managers);
            var managerCount = await SeedUsersByRole(UserRole.Manager, _options.Counts.Managers, DefaultPasswordHash, cancellationToken);
            totalUsers += managerCount;
            _logger.LogInformation("Completed seeding {Count} manager users", managerCount);

            // Seed Customers
            _logger.LogInformation("Seeding {Count} customer users...", _options.Counts.Customers);
            var customerCount = await SeedUsersByRole(UserRole.Customer, _options.Counts.Customers, DefaultPasswordHash, cancellationToken);
            totalUsers += customerCount;
            _logger.LogInformation("Completed seeding {Count} customer users", customerCount);

            stopwatch.Stop();
            _logger.LogInformation("User seeding completed. Total users created: {TotalCount} in {Duration}ms",
                totalUsers, stopwatch.ElapsedMilliseconds);

            return SeedingResult.SuccessResult(SeederName, totalUsers, stopwatch.Elapsed,
                $"Seeded {adminCount} admins, {managerCount} managers, {customerCount} customers");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error occurred while seeding users");
            return SeedingResult.FailureResult(SeederName, ex);
        }
    }

    private async Task<int> SeedUsersByRole(UserRole role, int count, string passwordHash, CancellationToken cancellationToken)
    {
        if (count <= 0) return 0;

        var users = new List<User>();
        var faker = CreateFaker(role, passwordHash);

        for (int i = 0; i < count; i++)
        {
            var user = GenerateUser(faker, role, passwordHash);
            if (user != null)
            {
                users.Add(user);
            }

            // Batch insert for performance
            if (users.Count >= BatchSize || i == count - 1)
            {
                foreach (var u in users)
                {
                    await _userRepository.AddAsync(u, cancellationToken);
                }

                _logger.LogDebug("Inserted batch of {Count} {Role} users ({Current}/{Total})",
                    users.Count, role, i + 1, count);

                users.Clear();
            }
        }

        return count;
    }

    private Faker CreateFaker(UserRole role, string passwordHash)
    {
        return new Faker();
    }

    private User GenerateUser(Faker faker, UserRole role, string passwordHash)
    {
        try
        {
            // Generate email based on role
            var emailDomain = role == UserRole.Customer
                ? faker.Internet.DomainName()
                : "shopilent.com";

            var firstName = faker.Name.FirstName();
            var lastName = faker.Name.LastName();
            var emailPrefix = role switch
            {
                UserRole.Admin => $"admin.{firstName.ToLower()}.{lastName.ToLower()}",
                UserRole.Manager => $"manager.{firstName.ToLower()}.{lastName.ToLower()}",
                _ => $"{firstName.ToLower()}.{lastName.ToLower()}"
            };

            var emailValue = $"{emailPrefix}@{emailDomain}";
            var emailResult = Email.Create(emailValue);
            if (emailResult.IsFailure)
            {
                // Fallback to simpler email format
                emailValue = $"{faker.Internet.UserName()}@{emailDomain}";
                emailResult = Email.Create(emailValue);
                if (emailResult.IsFailure) return null;
            }

            var fullNameResult = FullName.Create(firstName, lastName, faker.Name.Suffix());
            if (fullNameResult.IsFailure) return null;

            var userResult = User.CreatePreVerified(
                emailResult.Value,
                passwordHash,
                fullNameResult.Value,
                role);

            if (userResult.IsFailure) return null;

            var user = userResult.Value;

            // Add phone number for some users (70% chance)
            if (faker.Random.Double() < 0.7)
            {
                var phoneValue = faker.Phone.PhoneNumber("##########");
                var phoneResult = PhoneNumber.Create(phoneValue);
                if (phoneResult.IsSuccess)
                {
                    user.UpdatePersonalInfo(fullNameResult.Value, phoneResult.Value);
                }
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate user for role {Role}", role);
            return null;
        }
    }

    public async Task<bool> EnsureDefaultAdminUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if default admin user already exists
            var existingAdmin = await _userRepository.GetByEmailAsync(DefaultAdminEmail, cancellationToken);

            if (existingAdmin == null)
            {
                _logger.LogInformation("Default admin user does not exist. Creating admin user...");

                var emailResult = Email.Create(DefaultAdminEmail);
                if (emailResult.IsFailure)
                {
                    _logger.LogError("Failed to create admin email: {Error}", emailResult.Error);
                    return false;
                }

                var fullNameResult = FullName.Create("Admin", "User", null);
                if (fullNameResult.IsFailure)
                {
                    _logger.LogError("Failed to create admin full name: {Error}", fullNameResult.Error);
                    return false;
                }

                var userResult = User.CreatePreVerified(
                    emailResult.Value,
                    DefaultPasswordHash,
                    fullNameResult.Value,
                    UserRole.Admin);

                if (userResult.IsFailure)
                {
                    _logger.LogError("Failed to create admin user: {Error}", userResult.Error);
                    return false;
                }

                await _userRepository.AddAsync(userResult.Value, cancellationToken);

                _logger.LogInformation("Default admin user created successfully. Email: {Email}, Password: P@55word",
                    DefaultAdminEmail);

                return true;
            }

            _logger.LogDebug("Default admin user already exists. Skipping creation.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while ensuring default admin user exists");
            return false;
        }
    }
}
