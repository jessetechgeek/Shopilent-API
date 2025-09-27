using System.Net;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.Application.Features.Catalog.Commands.CreateAttribute.V1;
using Shopilent.Application.Features.Catalog.Queries.GetAttributesDatatable.V1;
using Shopilent.Domain.Catalog.Enums;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Attributes.GetAttributesDatatable.V1;

public class GetAttributesDatatableEndpointV1Tests : ApiIntegrationTestBase
{
    public GetAttributesDatatableEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAttributesDatatable_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.CreateValidRequest();

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

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
    public async Task GetAttributesDatatable_WithTestAttributes_ShouldReturnCorrectData()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();

        // Create test attributes
        await CreateTestAttributeAsync("color", "Color", AttributeType.Text, true, true, false);
        await CreateTestAttributeAsync("size", "Size", AttributeType.Text, true, true, true);
        await CreateTestAttributeAsync("weight", "Weight", AttributeType.Number, false, true, false);

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.CreateValidRequest(length: 10);

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().HaveCountGreaterThanOrEqualTo(3); // At least the 3 test attributes
        response.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(3);
        response.Data.RecordsFiltered.Should().BeGreaterThanOrEqualTo(3);

        // Verify data structure
        var firstAttribute = response.Data.Data.First();
        firstAttribute.Id.Should().NotBeEmpty();
        firstAttribute.Name.Should().NotBeNullOrEmpty();
        firstAttribute.DisplayName.Should().NotBeNullOrEmpty();
        firstAttribute.Type.Should().NotBeNullOrEmpty();
        firstAttribute.CreatedAt.Should().NotBe(default);
        firstAttribute.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task GetAttributesDatatable_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateMultipleTestAttributesAsync(8);

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // First page
        var firstPageRequest = GetAttributesDatatableTestDataV1.Pagination.CreateFirstPageRequest(pageSize: 3);

        // Act
        var firstPageResponse = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", firstPageRequest);

        // Assert
        AssertApiSuccess(firstPageResponse);
        firstPageResponse!.Data.Data.Should().HaveCount(3);
        firstPageResponse.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(8);

        // Second page
        var secondPageRequest = GetAttributesDatatableTestDataV1.Pagination.CreateSecondPageRequest(pageSize: 3);
        var secondPageResponse = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", secondPageRequest);

        AssertApiSuccess(secondPageResponse);
        secondPageResponse!.Data.Data.Should().HaveCount(3);

        // Verify different attributes on different pages
        var firstPageIds = firstPageResponse.Data.Data.Select(a => a.Id).ToList();
        var secondPageIds = secondPageResponse.Data.Data.Select(a => a.Id).ToList();
        firstPageIds.Should().NotIntersectWith(secondPageIds);
    }

    [Fact]
    public async Task GetAttributesDatatable_WithSearch_ShouldReturnFilteredResults()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateTestAttributeAsync("searchable_color", "Searchable Color", AttributeType.Text, true, true, false);
        await CreateTestAttributeAsync("another_size", "Another Size", AttributeType.Text, true, true, true);

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.SearchScenarios.CreateNameSearchRequest("searchable");

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().HaveCount(1);
        response.Data.Data.First().Name.Should().Contain("searchable");
        response.Data.RecordsFiltered.Should().Be(1);
    }

    [Fact]
    public async Task GetAttributesDatatable_WithSorting_ShouldReturnSortedResults()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateTestAttributeAsync("alpha_attr", "Alpha Attribute", AttributeType.Text, true, true, false);
        await CreateTestAttributeAsync("beta_attr", "Beta Attribute", AttributeType.Text, true, true, false);
        await CreateTestAttributeAsync("gamma_attr", "Gamma Attribute", AttributeType.Text, true, true, false);

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.SortingScenarios.CreateSortByNameAscRequest();

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().HaveCountGreaterThanOrEqualTo(3);

        var sortedNames = response.Data.Data.Select(a => a.Name).ToList();
        sortedNames.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetAttributesDatatable_WithDescendingSortByCreatedAt_ShouldReturnNewestFirst()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var oldAttributeId = await CreateTestAttributeAsync("old_attr", "Old Attribute", AttributeType.Text, true, true, false);
        await Task.Delay(1000); // Ensure different timestamps
        var newAttributeId = await CreateTestAttributeAsync("new_attr", "New Attribute", AttributeType.Text, true, true, false);

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.SortingScenarios.CreateSortByCreatedAtRequest();

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();

        var sortedDates = response.Data.Data.Select(a => a.CreatedAt).ToList();
        sortedDates.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetAttributesDatatable_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = GetAttributesDatatableTestDataV1.CreateValidRequest();

        // Act
        var response = await PostAsync("v1/attributes/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAttributesDatatable_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.CreateValidRequest();

        // Act
        var response = await PostAsync("v1/attributes/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAttributesDatatable_WithManagerRole_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateTestUserAsync("manager@test.com", "Manager", "User", UserRole.Manager);
        var accessToken = await AuthenticateAsync("manager@test.com", "Password123!");
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.CreateValidRequest();

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAttributesDatatable_WithZeroLength_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.Pagination.CreateZeroLengthRequest();

        // Act
        var response = await PostAsync("v1/attributes/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Length must be greater than 0");
    }

    [Fact]
    public async Task GetAttributesDatatable_WithNegativeValues_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.ValidationTests.CreateNegativeStartRequest();

        // Act
        var response = await PostAsync("v1/attributes/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Start must be greater than or equal to 0");
    }

    [Fact]
    public async Task GetAttributesDatatable_WithExcessiveLength_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.ValidationTests.CreateExcessiveLengthRequest();

        // Act
        var response = await PostAsync("v1/attributes/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Length must be less than or equal to 1000");
    }

    [Fact]
    public async Task GetAttributesDatatable_WithNoResultsSearch_ShouldReturnEmptyData()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.SearchScenarios.CreateNoResultsSearchRequest();

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().BeEmpty();
        response.Data.RecordsFiltered.Should().Be(0);
        response.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(0); // Total attributes might exist
    }

    [Fact]
    public async Task GetAttributesDatatable_WithUnicodeSearch_ShouldReturnCorrectResults()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateTestAttributeAsync("groesse", "Größe", AttributeType.Text, true, true, false);

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.SearchScenarios.CreateUnicodeSearchRequest();

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().HaveCount(1);
        response.Data.Data.First().DisplayName.Should().Contain("Größe");
    }

    [Fact]
    public async Task GetAttributesDatatable_WithComplexRequest_ShouldHandleAllParameters()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateTestAttributeAsync("complex1", "Complex Attribute 1", AttributeType.Text, true, true, false);
        await CreateTestAttributeAsync("complex2", "Complex Attribute 2", AttributeType.Number, false, true, true);

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.EdgeCases.CreateComplexRequest();

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Draw.Should().Be(request.Draw);
        response.Data.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAttributesDatatable_WithInvalidColumnSort_ShouldHandleGracefully()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.SortingScenarios.CreateInvalidColumnSortRequest();

        // Act
        var response = await PostAsync("v1/attributes/datatable", request);

        // Assert - Should either return BadRequest or handle gracefully with default sort
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAttributesDatatable_ConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var requests = Enumerable.Range(0, 5)
            .Select(i => GetAttributesDatatableTestDataV1.CreateValidRequest(draw: i + 1))
            .ToList();

        // Act
        var tasks = requests.Select(request =>
            PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request)
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
    public async Task GetAttributesDatatable_WithEmptySearch_ShouldReturnAllAttributes()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateTestAttributeAsync("empty_test", "Empty Test", AttributeType.Text, true, true, false);

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.SearchScenarios.CreateEmptySearchRequest();

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.RecordsFiltered.Should().Be(response.Data.RecordsTotal);
        response.Data.Data.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetAttributesDatatable_WithHighPageNumber_ShouldReturnEmptyOrLastPage()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.Pagination.CreateHighStartRequest();

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().BeEmpty(); // No attributes at such high page number
        response.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(0);
        response.Data.RecordsFiltered.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetAttributesDatatable_DatabaseConsistency_ShouldMatchDatabaseCounts()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var testAttribute1Id = await CreateTestAttributeAsync("db_test1", "Database Test 1", AttributeType.Text, true, true, false);
        var testAttribute2Id = await CreateTestAttributeAsync("db_test2", "Database Test 2", AttributeType.Number, false, true, true);

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.CreateValidRequest(length: 100); // Get all attributes

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

        // Assert
        AssertApiSuccess(response);

        // Verify against database
        await ExecuteDbContextAsync(async context =>
        {
            var totalAttributesInDb = await context.Attributes.CountAsync();
            response!.Data.RecordsTotal.Should().Be(totalAttributesInDb);
            response.Data.RecordsFiltered.Should().Be(totalAttributesInDb);

            // Verify specific test attributes are included
            var testAttributeIds = new[] { testAttribute1Id, testAttribute2Id };
            var responseIds = response.Data.Data.Select(a => a.Id).ToList();
            responseIds.Should().Contain(testAttributeIds);
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    public async Task GetAttributesDatatable_WithDifferentPageSizes_ShouldReturnCorrectCount(int pageSize)
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateMultipleTestAttributesAsync(15); // Create enough attributes to test pagination

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.CreateValidRequest(length: pageSize);

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();

        // Should return at most pageSize items, but could be less if not enough data
        response.Data.Data.Should().HaveCountLessOrEqualTo(pageSize);
        response.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(response.Data.Data.Count);
    }

    [Fact]
    public async Task GetAttributesDatatable_ResponseTime_ShouldBeReasonable()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateMultipleTestAttributesAsync(50); // Create a decent amount of data

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.CreateValidRequest();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);
        stopwatch.Stop();

        // Assert
        AssertApiSuccess(response);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10)); // Should be fast enough for UI
    }

    [Fact]
    public async Task GetAttributesDatatable_WithFilterableAttributes_ShouldShowCorrectFlags()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateTestAttributeAsync("filterable_attr", "Filterable Attribute", AttributeType.Text, true, false, false);
        await CreateTestAttributeAsync("non_filterable_attr", "Non-Filterable Attribute", AttributeType.Text, false, true, false);

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.CreateValidRequest(length: 10);

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();

        var filterableAttr = response.Data.Data.FirstOrDefault(a => a.Name == "filterable_attr");
        var nonFilterableAttr = response.Data.Data.FirstOrDefault(a => a.Name == "non_filterable_attr");

        filterableAttr?.Filterable.Should().BeTrue();
        filterableAttr?.Searchable.Should().BeFalse();

        nonFilterableAttr?.Filterable.Should().BeFalse();
        nonFilterableAttr?.Searchable.Should().BeTrue();
    }

    [Fact]
    public async Task GetAttributesDatatable_WithVariantAttributes_ShouldShowCorrectFlags()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await CreateTestAttributeAsync("variant_attr", "Variant Attribute", AttributeType.Text, true, true, true);
        await CreateTestAttributeAsync("product_attr", "Product Attribute", AttributeType.Text, true, true, false);

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetAttributesDatatableTestDataV1.CreateValidRequest(length: 10);

        // Act
        var response = await PostDataTableResponseAsync<AttributeDatatableDto>("v1/attributes/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();

        var variantAttr = response.Data.Data.FirstOrDefault(a => a.Name == "variant_attr");
        var productAttr = response.Data.Data.FirstOrDefault(a => a.Name == "product_attr");

        variantAttr?.IsVariant.Should().BeTrue();
        productAttr?.IsVariant.Should().BeFalse();
    }

    // Helper methods
    private async Task<Guid> CreateTestAttributeAsync(
        string name,
        string displayName,
        AttributeType type,
        bool filterable = true,
        bool searchable = true,
        bool isVariant = false)
    {
        using var scope = Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var createCommand = new CreateAttributeCommandV1
        {
            Name = name,
            DisplayName = displayName,
            Type = type,
            Filterable = filterable,
            Searchable = searchable,
            IsVariant = isVariant,
            Configuration = new Dictionary<string, object>()
        };

        var result = await mediator.Send(createCommand);

        if (result.IsSuccess && result.Value != null)
        {
            return result.Value.Id;
        }

        throw new InvalidOperationException($"Failed to create test attribute: {name}");
    }

    private async Task CreateMultipleTestAttributesAsync(int count)
    {
        var types = new[] { AttributeType.Text, AttributeType.Number, AttributeType.Boolean };
        var tasks = new List<Task>();

        for (int i = 0; i < count; i++)
        {
            var name = $"test_attr_{i}";
            var displayName = $"Test Attribute {i}";
            var type = types[i % types.Length];
            var filterable = i % 2 == 0;
            var searchable = i % 3 == 0;
            var isVariant = i % 4 == 0;

            tasks.Add(CreateTestAttributeAsync(name, displayName, type, filterable, searchable, isVariant));
        }

        await Task.WhenAll(tasks);
    }

    private async Task<Guid> CreateTestUserAsync(string email, string firstName, string lastName, UserRole role = UserRole.Customer)
    {
        using var scope = Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var registerCommand = new Shopilent.Application.Features.Identity.Commands.Register.V1.RegisterCommandV1
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
                var changeRoleCommand = new Shopilent.Application.Features.Identity.Commands.ChangeUserRole.V1.ChangeUserRoleCommandV1
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
}