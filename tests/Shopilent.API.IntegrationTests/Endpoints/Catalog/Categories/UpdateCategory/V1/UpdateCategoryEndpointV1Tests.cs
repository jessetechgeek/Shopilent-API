using System.Net;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Endpoints.Catalog.Categories.UpdateCategory.V1;
using Shopilent.Application.Features.Catalog.Commands.CreateCategory.V1;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.UpdateCategory.V1;

public class UpdateCategoryEndpointV1Tests : ApiIntegrationTestBase
{
    public UpdateCategoryEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task UpdateCategory_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a category to update
        var categoryId = await CreateTestCategoryAsync("Original Name", "original-name", "Original description");

        var updateRequest = new
        {
            Name = "Updated Category Name",
            Slug = "updated-category-name",
            Description = "Updated description for the category",
            IsActive = true
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, UpdateCategoryResponseV1>($"v1/categories/{categoryId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(categoryId);
        response.Data.Name.Should().Be("Updated Category Name");
        response.Data.Slug.Should().Be("updated-category-name");
        response.Data.Description.Should().Be("Updated description for the category");
        response.Data.IsActive.Should().BeTrue();
        response.Data.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        response.Message.Should().Be("Category updated successfully");
    }

    [Fact]
    public async Task UpdateCategory_WithValidData_ShouldUpdateInDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Database Test", "database-test", "Original database description");

        var updateRequest = new
        {
            Name = "Updated Database Test",
            Slug = "updated-database-test",
            Description = "Updated database description",
            IsActive = true
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, UpdateCategoryResponseV1>($"v1/categories/{categoryId}", updateRequest);

        // Assert
        AssertApiSuccess(response);

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var updatedCategory = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            updatedCategory.Should().NotBeNull();
            updatedCategory!.Name.Should().Be("Updated Database Test");
            updatedCategory.Slug.Value.Should().Be("updated-database-test");
            updatedCategory.Description.Should().Be("Updated database description");
            updatedCategory.IsActive.Should().BeTrue();
            updatedCategory.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        });
    }

    [Fact]
    public async Task UpdateCategory_WithPartialData_ShouldUpdateOnlyProvidedFields()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Partial Test", "partial-test", "Original description");

        // Update only name and description, keep original slug
        var updateRequest = new
        {
            Name = "Updated Partial Name",
            Slug = "partial-test", // Keep original slug
            Description = "Updated description only",
            IsActive = (bool?)null // Don't change active status
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, UpdateCategoryResponseV1>($"v1/categories/{categoryId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("Updated Partial Name");
        response.Data.Slug.Should().Be("partial-test");
        response.Data.Description.Should().Be("Updated description only");
        response.Data.IsActive.Should().BeTrue(); // Should maintain original active status
    }

    [Fact]
    public async Task UpdateCategory_WithUnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Unicode Test", "unicode-test", "Original description");

        var updateRequest = new
        {
            Name = "Café & Münchën Category™",
            Slug = "cafe-munchen-category",
            Description = "Ürünler kategórisi with émojis 🛍️",
            IsActive = true
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, UpdateCategoryResponseV1>($"v1/categories/{categoryId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("Café & Münchën Category™");
        response.Data.Slug.Should().Be("cafe-munchen-category");
        response.Data.Description.Should().Be("Ürünler kategórisi with émojis 🛍️");
    }

    [Fact]
    public async Task UpdateCategory_WithManagerRole_ShouldAllowUpdate()
    {
        // Arrange
        await CreateTestUserAsync("manager@test.com", "Manager", "User", UserRole.Manager);
        var accessToken = await AuthenticateAsync("manager@test.com", "Password123!");
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Manager Test", "manager-test", "Manager test description");

        var updateRequest = new
        {
            Name = "Updated by Manager",
            Slug = "updated-by-manager",
            Description = "Updated by manager role",
            IsActive = true
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, UpdateCategoryResponseV1>($"v1/categories/{categoryId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("Updated by Manager");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task UpdateCategory_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Validation Test", "validation-test", "Test description");

        var updateRequest = new
        {
            Name = "",
            Slug = "valid-slug",
            Description = "Valid description",
            IsActive = true
        };

        // Act
        var response = await PutAsync($"v1/categories/{categoryId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Category name is required");
    }

    [Fact]
    public async Task UpdateCategory_WithLongName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Long Name Test", "long-name-test", "Test description");

        var updateRequest = new
        {
            Name = new string('A', 101), // Exceeds 100 character limit
            Slug = "valid-slug",
            Description = "Valid description",
            IsActive = true
        };

        // Act
        var response = await PutAsync($"v1/categories/{categoryId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("must not exceed 100 characters");
    }

    [Fact]
    public async Task UpdateCategory_WithEmptySlug_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Slug Test", "slug-test", "Test description");

        var updateRequest = new
        {
            Name = "Valid Name",
            Slug = "",
            Description = "Valid description",
            IsActive = true
        };

        // Act
        var response = await PutAsync($"v1/categories/{categoryId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Category slug is required");
    }

    [Fact]
    public async Task UpdateCategory_WithInvalidSlugFormat_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Invalid Slug Test", "invalid-slug-test", "Test description");

        var updateRequest = new
        {
            Name = "Valid Name",
            Slug = "Invalid Slug With Spaces!", // Invalid format
            Description = "Valid description",
            IsActive = true
        };

        // Act
        var response = await PutAsync($"v1/categories/{categoryId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("slug must contain only lowercase letters, numbers, and hyphens");
    }

    [Fact]
    public async Task UpdateCategory_WithLongDescription_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Description Test", "description-test", "Test description");

        var updateRequest = new
        {
            Name = "Valid Name",
            Slug = "valid-slug",
            Description = new string('D', 501), // Exceeds 500 character limit
            IsActive = true
        };

        // Act
        var response = await PutAsync($"v1/categories/{categoryId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("must not exceed 500 characters");
    }

    [Theory]
    [InlineData("UPPERCASE-SLUG")]
    [InlineData("slug with spaces")]
    [InlineData("slug.with.dots")]
    [InlineData("slug_with_underscores")]
    public async Task UpdateCategory_WithInvalidSlugFormats_ShouldReturnValidationError(string invalidSlug)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Format Test", "format-test", "Test description");

        var updateRequest = new
        {
            Name = "Valid Name",
            Slug = invalidSlug,
            Description = "Valid description",
            IsActive = true
        };

        // Act
        var response = await PutAsync($"v1/categories/{categoryId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task UpdateCategory_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();

        var categoryId = Guid.NewGuid();
        var updateRequest = new
        {
            Name = "Test Name",
            Slug = "test-slug",
            Description = "Test description",
            IsActive = true
        };

        // Act
        var response = await PutAsync($"v1/categories/{categoryId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCategory_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = Guid.NewGuid();
        var updateRequest = new
        {
            Name = "Test Name",
            Slug = "test-slug",
            Description = "Test description",
            IsActive = true
        };

        // Act
        var response = await PutAsync($"v1/categories/{categoryId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task UpdateCategory_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var nonExistentId = Guid.NewGuid();
        var updateRequest = new
        {
            Name = "Test Name",
            Slug = "test-slug",
            Description = "Test description",
            IsActive = true
        };

        // Act
        var response = await PutAsync($"v1/categories/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateCategory_WithInvalidGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var updateRequest = new
        {
            Name = "Test Name",
            Slug = "test-slug",
            Description = "Test description",
            IsActive = true
        };

        // Act
        var response = await PutAsync("v1/categories/invalid-guid", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCategory_WithDuplicateSlug_ShouldReturnConflict()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create two categories
        var category1Id = await CreateTestCategoryAsync("Category 1", "category-1", "First category");
        var category2Id = await CreateTestCategoryAsync("Category 2", "category-2", "Second category");

        // Try to update category2 with category1's slug
        var updateRequest = new
        {
            Name = "Updated Category 2",
            Slug = "category-1", // This slug already exists
            Description = "Updated description",
            IsActive = true
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutAsync($"v1/categories/{category2Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("already exists", "duplicate", "slug");
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public async Task UpdateCategory_WithMaximumValidLengths_ShouldSucceed()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Boundary Test", "boundary-test", "Test description");

        var updateRequest = new
        {
            Name = new string('A', 100), // Exactly 100 characters
            Slug = new string('a', 150), // Exactly 150 characters
            Description = new string('D', 500), // Exactly 500 characters
            IsActive = true
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, UpdateCategoryResponseV1>($"v1/categories/{categoryId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be(new string('A', 100));
        response.Data.Slug.Should().Be(new string('a', 150));
        response.Data.Description.Should().Be(new string('D', 500));
    }

    [Fact]
    public async Task UpdateCategory_WithMinimumValidLengths_ShouldSucceed()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Minimum Test", "minimum-test", "Test description");

        var updateRequest = new
        {
            Name = "A", // Single character
            Slug = "a", // Single character
            Description = "", // Empty description (optional)
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, UpdateCategoryResponseV1>($"v1/categories/{categoryId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("A");
        response.Data.Slug.Should().Be("a");
        response.Data.Description.Should().Be("");
        response.Data.IsActive.Should().BeFalse();
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task UpdateCategory_ConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Concurrent Test", "concurrent-test", "Test description");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        var tasks = Enumerable.Range(1, 3)
            .Select(i => new
            {
                Name = $"Concurrent Update {i}",
                Slug = $"concurrent-update-{i}",
                Description = $"Concurrent update {i} description",
                IsActive = true
            })
            .Select(request => PutApiResponseAsync<object, UpdateCategoryResponseV1>($"v1/categories/{categoryId}", request))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        // At least one should succeed, others might fail due to concurrency
        responses.Should().Contain(response => response != null && response.Succeeded);

        // The category should have been updated by one of the requests
        await ExecuteDbContextAsync(async context =>
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            category.Should().NotBeNull();
            category!.Name.Should().StartWith("Concurrent Update");
        });
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateTestCategoryAsync(
        string name,
        string slug,
        string description,
        Guid? parentId = null)
    {
        using var scope = Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var createCommand = new CreateCategoryCommandV1
        {
            Name = name,
            Slug = slug,
            Description = description,
            ParentId = parentId
        };

        var result = await mediator.Send(createCommand);

        if (result.IsSuccess && result.Value != null)
        {
            return result.Value.Id;
        }

        throw new InvalidOperationException($"Failed to create test category: {name}");
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

    #endregion
}