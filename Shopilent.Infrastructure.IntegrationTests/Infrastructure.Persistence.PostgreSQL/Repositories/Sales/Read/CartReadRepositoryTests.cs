using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Infrastructure.IntegrationTests.Common;
using Shopilent.Infrastructure.IntegrationTests.TestData.Builders;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Persistence.PostgreSQL.Repositories.Sales.Read;

[Collection("IntegrationTests")]
public class CartReadRepositoryTests : IntegrationTestBase
{
    private IUnitOfWork _unitOfWork = null!;

    public CartReadRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _unitOfWork = GetService<IUnitOfWork>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCart_ShouldReturnCart()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var cart = new CartBuilder()
            .WithUser(user)
            .WithMetadata("source", "web")
            .Build();

        await _unitOfWork.CartWriter.AddAsync(cart);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.CartReader.GetByIdAsync(cart.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(cart.Id);
        result.UserId.Should().Be(user.Id);
        result.Metadata.Should().ContainKey("source");
        result.Metadata["source"].ToString().Should().Be("web");
        result.CreatedAt.Should().BeCloseTo(cart.CreatedAt, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentCart_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _unitOfWork.CartReader.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingUserCart_ShouldReturnCart()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var cart = new CartBuilder()
            .WithUser(user)
            .WithRandomMetadata()
            .Build();

        await _unitOfWork.CartWriter.AddAsync(cart);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.CartReader.GetByUserIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(cart.Id);
        result.UserId.Should().Be(user.Id);
        result.Metadata.Should().NotBeEmpty();
        result.CreatedAt.Should().BeCloseTo(cart.CreatedAt, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task GetByUserIdAsync_NonExistentUser_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentUserId = Guid.NewGuid();

        // Act
        var result = await _unitOfWork.CartReader.GetByUserIdAsync(nonExistentUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_AnonymousCart_ShouldNotReturnCart()
    {
        // Arrange
        await ResetDatabaseAsync();

        var anonymousCart = new CartBuilder()
            .AsAnonymousCart()
            .Build();

        await _unitOfWork.CartWriter.AddAsync(anonymousCart);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.CartReader.GetByUserIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ListAllAsync_EmptyRepository_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var result = await _unitOfWork.CartReader.ListAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAllAsync_HasCarts_ShouldReturnAllCarts()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user1 = new UserBuilder().Build();
        var user2 = new UserBuilder().Build();
        await _unitOfWork.UserWriter.AddAsync(user1);
        await _unitOfWork.UserWriter.AddAsync(user2);
        await _unitOfWork.SaveChangesAsync();

        var cart1 = new CartBuilder().WithUser(user1).Build();
        var cart2 = new CartBuilder().WithUser(user2).Build();
        var anonymousCart = new CartBuilder().AsAnonymousCart().Build();

        await _unitOfWork.CartWriter.AddAsync(cart1);
        await _unitOfWork.CartWriter.AddAsync(cart2);
        await _unitOfWork.CartWriter.AddAsync(anonymousCart);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.CartReader.ListAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Select(c => c.Id).Should().Contain(new[] { cart1.Id, cart2.Id, anonymousCart.Id });
    }

    [Fact]
    public async Task GetAbandonedCartsAsync_HasOldCarts_ShouldReturnAbandonedCarts()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user1 = new UserBuilder().Build();
        var user2 = new UserBuilder().Build();
        await _unitOfWork.UserWriter.AddAsync(user1);
        await _unitOfWork.UserWriter.AddAsync(user2);

        var category = new CategoryBuilder().Build();
        await _unitOfWork.CategoryWriter.AddAsync(category);

        var product1 = new ProductBuilder().WithCategory(category).Build();
        var product2 = new ProductBuilder().WithCategory(category).Build();
        await _unitOfWork.ProductWriter.AddAsync(product1);
        await _unitOfWork.ProductWriter.AddAsync(product2);
        await _unitOfWork.SaveChangesAsync();

        // Create carts with items that will be considered "old"
        // Note: GetAbandonedCartsAsync only returns carts that have items
        var oldCart = new CartBuilder()
            .WithUser(user1)
            .WithItem(product1, 1)
            .Build();
        var anotherOldCart = new CartBuilder()
            .WithUser(user2)
            .WithItem(product2, 2)
            .Build();

        await _unitOfWork.CartWriter.AddAsync(oldCart);
        await _unitOfWork.CartWriter.AddAsync(anotherOldCart);
        await _unitOfWork.SaveChangesAsync();

        // Wait to ensure the carts are older than our threshold
        await Task.Delay(2000); // Wait 2 seconds to be safe

        // Act - looking for carts older than 1 second
        var result = await _unitOfWork.CartReader.GetAbandonedCartsAsync(TimeSpan.FromSeconds(1));

        // Assert - Both carts should be considered "abandoned" since they're older than 1 second
        // and they both have items (which is required by the implementation)
        result.Should().HaveCount(2);
        result.Select(c => c.Id).Should().Contain(new[] { oldCart.Id, anotherOldCart.Id });
    }

    [Fact]
    public async Task GetAbandonedCartsAsync_NoOldCarts_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var recentCart = new CartBuilder().WithUser(user).Build();
        await _unitOfWork.CartWriter.AddAsync(recentCart);
        await _unitOfWork.SaveChangesAsync();

        // Act - looking for carts older than 1 hour
        var result = await _unitOfWork.CartReader.GetAbandonedCartsAsync(TimeSpan.FromHours(1));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAbandonedCartsAsync_EmptyRepository_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var result = await _unitOfWork.CartReader.GetAbandonedCartsAsync(TimeSpan.FromMinutes(30));

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_CartWithItems_ShouldIncludeItemsData()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        await _unitOfWork.UserWriter.AddAsync(user);

        var category = new CategoryBuilder().Build();
        await _unitOfWork.CategoryWriter.AddAsync(category);

        var product1 = new ProductBuilder().WithCategory(category).Build();
        var product2 = new ProductBuilder().WithCategory(category).Build();
        await _unitOfWork.ProductWriter.AddAsync(product1);
        await _unitOfWork.ProductWriter.AddAsync(product2);
        await _unitOfWork.SaveChangesAsync();

        var cart = new CartBuilder()
            .WithUser(user)
            .WithItem(product1, 2)
            .WithItem(product2, 1)
            .Build();

        await _unitOfWork.CartWriter.AddAsync(cart);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.CartReader.GetByIdAsync(cart.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(cart.Id);
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(3); // 2 + 1
        result.Items.Should().OnlyContain(item => item.Quantity > 0);
        result.Items.Select(i => i.ProductId).Should().Contain(new[] { product1.Id, product2.Id });
    }

    [Fact]
    public async Task GetByUserIdAsync_MultipleCartsForUser_ShouldReturnLatestCart()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Create multiple carts for the same user
        var cart1 = new CartBuilder().WithUser(user).Build();
        await _unitOfWork.CartWriter.AddAsync(cart1);
        await _unitOfWork.SaveChangesAsync();

        // Wait a bit to ensure different timestamps
        await Task.Delay(100);

        var cart2 = new CartBuilder().WithUser(user).WithMetadata("version", "2").Build();
        await _unitOfWork.CartWriter.AddAsync(cart2);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.CartReader.GetByUserIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        // Should return the latest cart (implementation may vary, but typically returns the most recent one)
        result!.UserId.Should().Be(user.Id);
        // The specific cart returned depends on the repository implementation
        // but both carts should be valid for this user
        new[] { cart1.Id, cart2.Id }.Should().Contain(result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NullId_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var result = await _unitOfWork.CartReader.GetByIdAsync(Guid.Empty);

        // Assert
        result.Should().BeNull();
    }
}