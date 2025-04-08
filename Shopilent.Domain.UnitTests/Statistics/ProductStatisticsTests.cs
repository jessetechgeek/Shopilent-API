using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Domain.Statistics;

namespace Shopilent.Domain.Tests.Statistics;

public class ProductStatisticsTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateProductStatistics()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var viewCount = 100;
        var orderCount = 10;
        var quantitySold = 15;
        var revenueResult = Money.FromDollars(750);
        Assert.True(revenueResult.IsSuccess);
        var revenue = revenueResult.Value;

        // Act
        var productStatistics = ProductStatistics.Create(
            productId,
            productName,
            viewCount,
            orderCount,
            quantitySold,
            revenue);

        // Assert
        Assert.Equal(productId, productStatistics.ProductId);
        Assert.Equal(productName, productStatistics.ProductName);
        Assert.Equal(viewCount, productStatistics.ViewCount);
        Assert.Equal(orderCount, productStatistics.OrderCount);
        Assert.Equal(quantitySold, productStatistics.QuantitySold);
        Assert.Equal(revenue, productStatistics.Revenue);
        Assert.True(productStatistics.LastUpdated <= DateTime.UtcNow);
        Assert.True(productStatistics.LastUpdated > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void IncrementViews_ShouldIncrementViewCount()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var initialViewCount = 100;
        var orderCount = 10;
        var quantitySold = 15;
        var revenueResult = Money.FromDollars(750);
        Assert.True(revenueResult.IsSuccess);
        var revenue = revenueResult.Value;

        var productStatistics = ProductStatistics.Create(
            productId,
            productName,
            initialViewCount,
            orderCount,
            quantitySold,
            revenue);

        // Act
        var updatedStatistics = productStatistics.IncrementViews();

        // Assert
        Assert.Equal(initialViewCount + 1, updatedStatistics.ViewCount);
        Assert.Equal(productId, updatedStatistics.ProductId);
        Assert.Equal(productName, updatedStatistics.ProductName);
        Assert.Equal(orderCount, updatedStatistics.OrderCount);
        Assert.Equal(quantitySold, updatedStatistics.QuantitySold);
        Assert.Equal(revenue, updatedStatistics.Revenue);
    }

    [Fact]
    public void AddSale_ShouldIncrementOrderCountAndQuantity()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var viewCount = 100;
        var initialOrderCount = 10;
        var initialQuantitySold = 15;
        var initialRevenueResult = Money.FromDollars(750);
        Assert.True(initialRevenueResult.IsSuccess);
        var initialRevenue = initialRevenueResult.Value;

        var productStatistics = ProductStatistics.Create(
            productId,
            productName,
            viewCount,
            initialOrderCount,
            initialQuantitySold,
            initialRevenue);

        var saleQuantity = 2;
        var saleAmountResult = Money.FromDollars(100);
        Assert.True(saleAmountResult.IsSuccess);
        var saleAmount = saleAmountResult.Value;

        // Act
        var updatedStatistics = productStatistics.AddSale(saleQuantity, saleAmount);

        // Assert
        Assert.Equal(initialOrderCount + 1, updatedStatistics.OrderCount);
        Assert.Equal(initialQuantitySold + saleQuantity, updatedStatistics.QuantitySold);
        Assert.Equal(initialRevenue.Amount + saleAmount.Amount, updatedStatistics.Revenue.Amount);
        Assert.Equal(productId, updatedStatistics.ProductId);
        Assert.Equal(productName, updatedStatistics.ProductName);
        Assert.Equal(viewCount, updatedStatistics.ViewCount);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var viewCount = 100;
        var orderCount = 10;
        var quantitySold = 15;
        var revenueResult = Money.FromDollars(750);
        Assert.True(revenueResult.IsSuccess);
        var revenue = revenueResult.Value;

        var statistics1 = ProductStatistics.Create(
            productId,
            productName,
            viewCount,
            orderCount,
            quantitySold,
            revenue);

        var statistics2 = ProductStatistics.Create(
            productId,
            productName,
            viewCount,
            orderCount,
            quantitySold,
            revenue);

        // Act & Assert
        Assert.Equal(statistics1, statistics2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var viewCount = 100;
        var orderCount = 10;
        var quantitySold = 15;
        var revenueResult = Money.FromDollars(750);
        Assert.True(revenueResult.IsSuccess);
        var revenue = revenueResult.Value;

        var statistics1 = ProductStatistics.Create(
            productId,
            productName,
            viewCount,
            orderCount,
            quantitySold,
            revenue);

        var statistics2 = ProductStatistics.Create(
            productId,
            productName,
            viewCount + 5, // Different view count
            orderCount,
            quantitySold,
            revenue);

        // Act & Assert
        Assert.NotEqual(statistics1, statistics2);
    }
}