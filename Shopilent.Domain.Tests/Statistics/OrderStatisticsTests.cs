using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Domain.Statistics;

namespace Shopilent.Domain.Tests.Statistics;

public class OrderStatisticsTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateOrderStatistics()
    {
        // Arrange
        var period = new DateTime(2023, 1, 1);
        var orderCount = 10;
        var totalRevenueResult = Money.FromDollars(1000);
        Assert.True(totalRevenueResult.IsSuccess);
        var totalRevenue = totalRevenueResult.Value;
        var newCustomerCount = 6;
        var returnCustomerCount = 4;

        // Act
        var orderStatistics = OrderStatistics.Create(
            period,
            orderCount,
            totalRevenue,
            newCustomerCount,
            returnCustomerCount);

        // Assert
        Assert.Equal(period, orderStatistics.Period);
        Assert.Equal(orderCount, orderStatistics.OrderCount);
        Assert.Equal(totalRevenue, orderStatistics.TotalRevenue);
        Assert.Equal(newCustomerCount, orderStatistics.NewCustomerCount);
        Assert.Equal(returnCustomerCount, orderStatistics.ReturnCustomerCount);
        
        // Check calculated properties
        var expectedAvg = 100m; // 1000 / 10
        Assert.Equal(expectedAvg, orderStatistics.AverageOrderValue.Amount);
        Assert.Equal(totalRevenue.Currency, orderStatistics.AverageOrderValue.Currency);
        
        var expectedRate = 40m; // (4 / 10) * 100
        Assert.Equal(expectedRate, orderStatistics.ReturnCustomerRate);
    }

    [Fact]
    public void Create_WithZeroOrders_ShouldHandleAverageAndRateCorrectly()
    {
        // Arrange
        var period = new DateTime(2023, 1, 1);
        var orderCount = 0;
        var totalRevenueResult = Money.FromDollars(0);
        Assert.True(totalRevenueResult.IsSuccess);
        var totalRevenue = totalRevenueResult.Value;
        var newCustomerCount = 0;
        var returnCustomerCount = 0;

        // Act
        var orderStatistics = OrderStatistics.Create(
            period,
            orderCount,
            totalRevenue,
            newCustomerCount,
            returnCustomerCount);

        // Assert
        Assert.Equal(period, orderStatistics.Period);
        Assert.Equal(orderCount, orderStatistics.OrderCount);
        Assert.Equal(totalRevenue, orderStatistics.TotalRevenue);
        
        // Check edge cases with zero orders
        Assert.Equal(0m, orderStatistics.AverageOrderValue.Amount);
        Assert.Equal(totalRevenue.Currency, orderStatistics.AverageOrderValue.Currency);
        Assert.Equal(0m, orderStatistics.ReturnCustomerRate);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var period = new DateTime(2023, 1, 1);
        var orderCount = 10;
        var totalRevenueResult = Money.FromDollars(1000);
        Assert.True(totalRevenueResult.IsSuccess);
        var totalRevenue = totalRevenueResult.Value;
        var newCustomerCount = 6;
        var returnCustomerCount = 4;

        var statistics1 = OrderStatistics.Create(
            period,
            orderCount,
            totalRevenue,
            newCustomerCount,
            returnCustomerCount);

        var statistics2 = OrderStatistics.Create(
            period,
            orderCount,
            totalRevenue,
            newCustomerCount,
            returnCustomerCount);

        // Act & Assert
        Assert.Equal(statistics1, statistics2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var period = new DateTime(2023, 1, 1);
        var totalRevenueResult = Money.FromDollars(1000);
        Assert.True(totalRevenueResult.IsSuccess);
        var totalRevenue = totalRevenueResult.Value;

        var statistics1 = OrderStatistics.Create(
            period,
            10,
            totalRevenue,
            6,
            4);

        var statistics2 = OrderStatistics.Create(
            period,
            15, // Different order count
            totalRevenue,
            6,
            4);

        // Act & Assert
        Assert.NotEqual(statistics1, statistics2);
    }
}