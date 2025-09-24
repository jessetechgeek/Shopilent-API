using System.Net;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.Application.Features.Identity.Commands.Register.V1;
using Shopilent.Application.Features.Identity.Commands.ChangeUserRole.V1;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.GetUsersDatatable.V1;

public class GetUsersDatatableEndpointV1Tests : ApiIntegrationTestBase
{
    public GetUsersDatatableEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUsersDatatable_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.CreateValidRequest();

        // Act
        var response = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Draw.Should().Be(request.Draw);
        response.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(0);
        response.Data.RecordsFiltered.Should().BeGreaterThanOrEqualTo(0);
        response.Data.Data.Should().NotBeNull();
        response.Data.Error.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetUsersDatatable_WithTestUsers_ShouldReturnCorrectData()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        
        // Create additional test users
        await CreateTestUserAsync("manager@test.com", "Test", "Manager", UserRole.Manager);
        await CreateTestUserAsync("customer2@test.com", "John", "Doe", UserRole.Customer);
        
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.CreateValidRequest(length: 10);

        // Act
        var response = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().HaveCountGreaterThanOrEqualTo(4); // At least admin, customer, manager, customer2
        response.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(4);
        response.Data.RecordsFiltered.Should().BeGreaterThanOrEqualTo(4);

        // Verify data structure
        var firstUser = response.Data.Data.First();
        firstUser.Id.Should().NotBeEmpty();
        firstUser.Email.Should().NotBeNullOrEmpty();
        firstUser.FullName.Should().NotBeNullOrEmpty();
        firstUser.RoleName.Should().NotBeNullOrEmpty();
        firstUser.CreatedAt.Should().NotBe(default);
        firstUser.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task GetUsersDatatable_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateMultipleTestUsersAsync(5);
        
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        
        // First page
        var firstPageRequest = GetUsersDatatableTestDataV1.Pagination.CreateFirstPageRequest(pageSize: 3);
        
        // Act
        var firstPageResponse = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", firstPageRequest);

        // Assert
        AssertApiSuccess(firstPageResponse);
        firstPageResponse!.Data.Data.Should().HaveCount(3);
        firstPageResponse.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(6); // Admin + 5 test users
        
        // Second page
        var secondPageRequest = GetUsersDatatableTestDataV1.Pagination.CreateSecondPageRequest(pageSize: 3);
        var secondPageResponse = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", secondPageRequest);
        
        AssertApiSuccess(secondPageResponse);
        secondPageResponse!.Data.Data.Should().HaveCount(3);
        
        // Verify different users on different pages
        var firstPageIds = firstPageResponse.Data.Data.Select(u => u.Id).ToList();
        var secondPageIds = secondPageResponse.Data.Data.Select(u => u.Id).ToList();
        firstPageIds.Should().NotIntersectWith(secondPageIds);
    }

    [Fact]
    public async Task GetUsersDatatable_WithSearch_ShouldReturnFilteredResults()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateTestUserAsync("searchable@test.com", "Searchable", "User", UserRole.Customer);
        await CreateTestUserAsync("another@test.com", "Another", "Person", UserRole.Customer);
        
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.SearchScenarios.CreateEmailSearchRequest("searchable");

        // Act
        var response = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().HaveCount(1);
        response.Data.Data.First().Email.Should().Contain("searchable");
        response.Data.RecordsFiltered.Should().Be(1);
    }

    [Fact]
    public async Task GetUsersDatatable_WithSorting_ShouldReturnSortedResults()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateTestUserAsync("alpha@test.com", "Alpha", "User", UserRole.Customer);
        await CreateTestUserAsync("beta@test.com", "Beta", "User", UserRole.Customer);
        await CreateTestUserAsync("gamma@test.com", "Gamma", "User", UserRole.Customer);
        
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.SortingScenarios.CreateSortByEmailAscRequest();

        // Act
        var response = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().HaveCountGreaterThanOrEqualTo(4);
        
        var sortedEmails = response.Data.Data.Select(u => u.Email).ToList();
        sortedEmails.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetUsersDatatable_WithDescendingSortByCreatedAt_ShouldReturnNewestFirst()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var oldUserId = await CreateTestUserAsync("old@test.com", "Old", "User", UserRole.Customer);
        await Task.Delay(1000); // Ensure different timestamps
        var newUserId = await CreateTestUserAsync("new@test.com", "New", "User", UserRole.Customer);
        
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.SortingScenarios.CreateSortByCreatedAtRequest();

        // Act
        var response = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        
        var sortedDates = response.Data.Data.Select(u => u.CreatedAt).ToList();
        sortedDates.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetUsersDatatable_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = GetUsersDatatableTestDataV1.CreateValidRequest();

        // Act
        var response = await PostAsync("v1/users/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsersDatatable_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.CreateValidRequest();

        // Act
        var response = await PostAsync("v1/users/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsersDatatable_WithManagerRole_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateTestUserAsync("manager@test.com", "Manager", "User", UserRole.Manager);
        var accessToken = await AuthenticateAsync("manager@test.com", "Password123!");
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.CreateValidRequest();

        // Act
        var response = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUsersDatatable_WithZeroLength_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.Pagination.CreateZeroLengthRequest();

        // Act
        var response = await PostAsync("v1/users/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetUsersDatatable_WithNegativeValues_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.ValidationTests.CreateNegativeStartRequest();

        // Act
        var response = await PostAsync("v1/users/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUsersDatatable_WithExcessiveLength_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.ValidationTests.CreateExcessiveLengthRequest();

        // Act
        var response = await PostAsync("v1/users/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUsersDatatable_WithNoResultsSearch_ShouldReturnEmptyData()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.SearchScenarios.CreateNoResultsSearchRequest();

        // Act
        var response = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().BeEmpty();
        response.Data.RecordsFiltered.Should().Be(0);
        response.Data.RecordsTotal.Should().BeGreaterThan(0); // Total users still exist
    }

    [Fact]
    public async Task GetUsersDatatable_WithUnicodeSearch_ShouldReturnCorrectResults()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateTestUserAsync("unicode@test.com", "Müller", "Üser", UserRole.Customer);
        
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.SearchScenarios.CreateUnicodeSearchRequest();

        // Act
        var response = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().HaveCount(1);
        response.Data.Data.First().FullName.Should().Contain("Müller");
    }

    [Fact]
    public async Task GetUsersDatatable_WithComplexRequest_ShouldHandleAllParameters()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateTestUserAsync("complex1@test.com", "Test", "User", UserRole.Manager);
        await CreateTestUserAsync("complex2@test.com", "Another", "Manager", UserRole.Manager);
        
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.EdgeCases.CreateComplexRequest();

        // Act
        var response = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Draw.Should().Be(request.Draw);
        response.Data.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUsersDatatable_WithInvalidColumnSort_ShouldHandleGracefully()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.SortingScenarios.CreateInvalidColumnSortRequest();

        // Act
        var response = await PostAsync("v1/users/datatable", request);

        // Assert - Should either return BadRequest or handle gracefully with default sort
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUsersDatatable_ConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        
        var requests = Enumerable.Range(0, 5)
            .Select(i => GetUsersDatatableTestDataV1.CreateValidRequest(draw: i + 1))
            .ToList();

        // Act
        var tasks = requests.Select(request => 
            PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request)
        ).ToList();
        
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Should().HaveCount(5);
        
        // Verify each response has correct draw number
        for (int i = 0; i < responses.Length; i++)
        {
            responses[i]!.Data.Draw.Should().Be(i + 1);
        }
    }

    [Fact]
    public async Task GetUsersDatatable_WithEmptySearch_ShouldReturnAllUsers()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateTestUserAsync("empty@test.com", "Empty", "Search", UserRole.Customer);
        
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.SearchScenarios.CreateEmptySearchRequest();

        // Act
        var response = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.RecordsFiltered.Should().Be(response.Data.RecordsTotal);
        response.Data.Data.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetUsersDatatable_WithHighPageNumber_ShouldReturnEmptyOrLastPage()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.Pagination.CreateHighStartRequest();

        // Act
        var response = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().BeEmpty(); // No users at such high page number
        response.Data.RecordsTotal.Should().BeGreaterThan(0);
        response.Data.RecordsFiltered.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetUsersDatatable_DatabaseConsistency_ShouldMatchDatabaseCounts()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var testUser1Id = await CreateTestUserAsync("db1@test.com", "Database", "User1", UserRole.Customer);
        var testUser2Id = await CreateTestUserAsync("db2@test.com", "Database", "User2", UserRole.Manager);
        
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.CreateValidRequest(length: 100); // Get all users

        // Act
        var response = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request);

        // Assert
        AssertApiSuccess(response);
        
        // Verify against database
        await ExecuteDbContextAsync(async context =>
        {
            var totalUsersInDb = await context.Users.CountAsync();
            response!.Data.RecordsTotal.Should().Be(totalUsersInDb);
            response.Data.RecordsFiltered.Should().Be(totalUsersInDb);
            
            // Verify specific test users are included
            var testUserIds = new[] { testUser1Id, testUser2Id };
            var responseIds = response.Data.Data.Select(u => u.Id).ToList();
            responseIds.Should().Contain(testUserIds);
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    public async Task GetUsersDatatable_WithDifferentPageSizes_ShouldReturnCorrectCount(int pageSize)
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateMultipleTestUsersAsync(15); // Create enough users to test pagination
        
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.CreateValidRequest(length: pageSize);

        // Act
        var response = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        
        // Should return at most pageSize items, but could be less if not enough data
        response.Data.Data.Should().HaveCountLessOrEqualTo(pageSize);
        response.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(response.Data.Data.Count);
    }

    [Fact]
    public async Task GetUsersDatatable_ResponseTime_ShouldBeReasonable()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateMultipleTestUsersAsync(50); // Create a decent amount of data
        
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetUsersDatatableTestDataV1.CreateValidRequest();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await PostDataTableResponseAsync<UserDatatableDto>("v1/users/datatable", request);
        stopwatch.Stop();

        // Assert
        AssertApiSuccess(response);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10)); // Should be fast enough for UI
    }

    // Helper methods
    private async Task<Guid> CreateTestUserAsync(string email, string firstName, string lastName, UserRole role = UserRole.Customer)
    {
        using var scope = Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var registerCommand = new RegisterCommandV1
        {
            Email = email,
            Password = "Password123!",
            FirstName = firstName,
            LastName = lastName,
            Phone = $"+1555{new Random().Next(1000000, 9999999)}",
            IpAddress = "127.0.0.1",
            UserAgent = "Integration Test"
        };

        var result = await mediator.Send(registerCommand);

        if (result.IsSuccess && result.Value != null)
        {
            var userId = result.Value.User.Id;

            // Change role if not customer
            if (role != UserRole.Customer)
            {
                var changeRoleCommand = new ChangeUserRoleCommandV1
                {
                    UserId = userId,
                    NewRole = role
                };
                await mediator.Send(changeRoleCommand);
            }

            return userId;
        }

        throw new InvalidOperationException($"Failed to create test user: {email}");
    }

    private async Task CreateMultipleTestUsersAsync(int count)
    {
        var roles = new[] { UserRole.Customer, UserRole.Manager };
        var tasks = new List<Task>();

        for (int i = 0; i < count; i++)
        {
            var email = $"testuser{i}@example.com";
            var firstName = $"Test{i}";
            var lastName = $"User{i}";
            var role = roles[i % roles.Length];
            
            tasks.Add(CreateTestUserAsync(email, firstName, lastName, role));
        }

        await Task.WhenAll(tasks);
    }

    // Response DTO for this specific endpoint version
    public class UserDatatableDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int AddressCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}