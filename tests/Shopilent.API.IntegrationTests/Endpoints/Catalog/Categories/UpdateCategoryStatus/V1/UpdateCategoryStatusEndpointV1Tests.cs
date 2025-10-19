using System.Net;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Application.Features.Catalog.Commands.CreateCategory.V1;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.UpdateCategoryStatus.V1;

public class UpdateCategoryStatusEndpointV1Tests : ApiIntegrationTestBase
{
    public UpdateCategoryStatusEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task UpdateCategoryStatus_ActivateCategory_ShouldSetToActive()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Status Test Category", "status-test-category", "Category for status testing");

        var updateRequest = new
        {
            IsActive = true
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/categories/{categoryId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Category status updated to active");
        response.Message.Should().Be("Category status updated successfully");
    }

    [Fact]
    public async Task UpdateCategoryStatus_DeactivateCategory_ShouldSetToInactive()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Deactivate Test", "deactivate-test", "Category for deactivation testing");

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/categories/{categoryId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Category status updated to inactive");
        response.Message.Should().Be("Category status updated successfully");
    }

    [Fact]
    public async Task UpdateCategoryStatus_UpdateInDatabase_ShouldPersistChanges()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Database Test", "database-test", "Category for database testing");

        // First deactivate
        var deactivateRequest = new { IsActive = false };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act - Deactivate
        var deactivateResponse = await PutApiResponseAsync<object, string>($"v1/categories/{categoryId}/status", deactivateRequest);

        // Assert deactivation
        AssertApiSuccess(deactivateResponse);

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            category.Should().NotBeNull();
            category!.IsActive.Should().BeFalse();
        });

        // Act - Reactivate
        var activateRequest = new { IsActive = true };
        var activateResponse = await PutApiResponseAsync<object, string>($"v1/categories/{categoryId}/status", activateRequest);

        // Assert reactivation
        AssertApiSuccess(activateResponse);

        // Verify in database again
        await ExecuteDbContextAsync(async context =>
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            category.Should().NotBeNull();
            category!.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public async Task UpdateCategoryStatus_WithManagerRole_ShouldAllowStatusUpdate()
    {
        // Arrange
        await CreateTestUserAsync("manager@test.com", "Manager", "User", UserRole.Manager);
        var accessToken = await AuthenticateAsync("manager@test.com", "Password123!");
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Manager Test", "manager-test", "Category for manager testing");

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/categories/{categoryId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Category status updated to inactive");
    }

    [Fact]
    public async Task UpdateCategoryStatus_ToggleMultipleTimes_ShouldHandleCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Toggle Test", "toggle-test", "Category for toggle testing");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act - Deactivate
        var deactivateRequest = new { IsActive = false };
        var deactivateResponse = await PutApiResponseAsync<object, string>($"v1/categories/{categoryId}/status", deactivateRequest);

        // Act - Reactivate
        var activateRequest = new { IsActive = true };
        var activateResponse = await PutApiResponseAsync<object, string>($"v1/categories/{categoryId}/status", activateRequest);

        // Act - Deactivate again
        var deactivateAgainResponse = await PutApiResponseAsync<object, string>($"v1/categories/{categoryId}/status", deactivateRequest);

        // Assert
        AssertApiSuccess(deactivateResponse);
        AssertApiSuccess(activateResponse);
        AssertApiSuccess(deactivateAgainResponse);

        deactivateResponse!.Data.Should().Be("Category status updated to inactive");
        activateResponse!.Data.Should().Be("Category status updated to active");
        deactivateAgainResponse!.Data.Should().Be("Category status updated to inactive");

        // Verify final state in database
        await ExecuteDbContextAsync(async context =>
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            category!.IsActive.Should().BeFalse();
        });
    }

    [Fact]
    public async Task UpdateCategoryStatus_SameStatus_ShouldStillReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Same Status Test", "same-status-test", "Category for same status testing");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Category starts as active, try to activate again
        var updateRequest = new { IsActive = true };

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/categories/{categoryId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Category status updated to active");

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            category!.IsActive.Should().BeTrue();
        });
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task UpdateCategoryStatus_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();

        var categoryId = Guid.NewGuid();
        var updateRequest = new
        {
            IsActive = false
        };

        // Act
        var response = await PutAsync($"v1/categories/{categoryId}/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCategoryStatus_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = Guid.NewGuid();
        var updateRequest = new
        {
            IsActive = false
        };

        // Act
        var response = await PutAsync($"v1/categories/{categoryId}/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task UpdateCategoryStatus_WithNonExistentCategory_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var nonExistentCategoryId = Guid.NewGuid();
        var updateRequest = new
        {
            IsActive = false
        };

        // Act
        var response = await PutAsync($"v1/categories/{nonExistentCategoryId}/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateCategoryStatus_WithInvalidGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var updateRequest = new
        {
            IsActive = false
        };

        // Act
        var response = await PutAsync("v1/categories/invalid-guid/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    public async Task UpdateCategoryStatus_WithChildCategories_ShouldUpdateOnlyTargetCategory()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create parent and child categories
        var parentId = await CreateTestCategoryAsync("Parent Category", "parent-category", "Parent category");
        var childId = await CreateTestCategoryAsync("Child Category", "child-category", "Child category", parentId);

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act - Deactivate parent
        var response = await PutApiResponseAsync<object, string>($"v1/categories/{parentId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);

        // Verify only parent is deactivated, child remains active
        await ExecuteDbContextAsync(async context =>
        {
            var parent = await context.Categories.FirstOrDefaultAsync(c => c.Id == parentId);
            var child = await context.Categories.FirstOrDefaultAsync(c => c.Id == childId);

            parent!.IsActive.Should().BeFalse();
            child!.IsActive.Should().BeTrue(); // Child should remain active
        });
    }

    [Fact]
    public async Task UpdateCategoryStatus_DeactivateRootCategory_ShouldSucceed()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var rootCategoryId = await CreateTestCategoryAsync("Root Category", "root-category", "Root category without parent");

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/categories/{rootCategoryId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Category status updated to inactive");

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == rootCategoryId);
            category!.IsActive.Should().BeFalse();
        });
    }

    #endregion

    #region Data Validation Tests

    [Theory]
    [InlineData(true, "active")]
    [InlineData(false, "inactive")]
    public async Task UpdateCategoryStatus_WithBooleanValues_ShouldReturnCorrectMessage(bool isActive, string expectedStatus)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync($"Status {expectedStatus}", $"status-{expectedStatus}", $"Category for {expectedStatus} testing");

        var updateRequest = new
        {
            IsActive = isActive
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/categories/{categoryId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be($"Category status updated to {expectedStatus}");

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            category!.IsActive.Should().Be(isActive);
        });
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task UpdateCategoryStatus_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple categories first to avoid concurrency conflicts
        var categoryIds = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var categoryId = await CreateTestCategoryAsync($"Concurrent Test {i}", $"concurrent-test-{i}", $"Category {i} for concurrency testing");
            categoryIds.Add(categoryId);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Create concurrent update tasks for different categories
        var tasks = categoryIds.Select((id, index) =>
        {
            var updateRequest = new { IsActive = index % 2 == 0 }; // Alternate between true/false
            return PutApiResponseAsync<object, string>($"v1/categories/{id}/status", updateRequest);
        }).ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));

        // Verify final state is consistent for all categories
        await ExecuteDbContextAsync(async context =>
        {
            var categories = await context.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync();

            categories.Should().HaveCount(5);
            categories.Should().AllSatisfy(category => category.Should().NotBeNull());
        });
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task UpdateCategoryStatus_ResponseTime_ShouldBeReasonable()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var categoryId = await CreateTestCategoryAsync("Performance Test", "performance-test", "Category for performance testing");

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/categories/{categoryId}/status", updateRequest);
        stopwatch.Stop();

        // Assert
        AssertApiSuccess(response);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2)); // Should be very fast for simple status update
    }

    [Fact]
    public async Task UpdateCategoryStatus_MultipleCategories_ShouldHandleIndependently()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var category1Id = await CreateTestCategoryAsync("Category 1", "category-1", "First category");
        var category2Id = await CreateTestCategoryAsync("Category 2", "category-2", "Second category");
        var category3Id = await CreateTestCategoryAsync("Category 3", "category-3", "Third category");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act - Update different categories to different statuses
        var response1 = await PutApiResponseAsync<object, string>($"v1/categories/{category1Id}/status", new { IsActive = false });
        var response2 = await PutApiResponseAsync<object, string>($"v1/categories/{category2Id}/status", new { IsActive = true });
        var response3 = await PutApiResponseAsync<object, string>($"v1/categories/{category3Id}/status", new { IsActive = false });

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);
        AssertApiSuccess(response3);

        // Verify each category has correct status
        await ExecuteDbContextAsync(async context =>
        {
            var categories = await context.Categories
                .Where(c => new[] { category1Id, category2Id, category3Id }.Contains(c.Id))
                .ToListAsync();

            var category1 = categories.First(c => c.Id == category1Id);
            var category2 = categories.First(c => c.Id == category2Id);
            var category3 = categories.First(c => c.Id == category3Id);

            category1.IsActive.Should().BeFalse();
            category2.IsActive.Should().BeTrue();
            category3.IsActive.Should().BeFalse();
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
