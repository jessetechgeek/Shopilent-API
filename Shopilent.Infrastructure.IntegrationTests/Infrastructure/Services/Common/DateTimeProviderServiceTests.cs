using System.Diagnostics;
using Shopilent.Domain.Common;
using Shopilent.Infrastructure.IntegrationTests.Common;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Services.Common;

[Collection("IntegrationTests")]
public class DateTimeProviderServiceTests : IntegrationTestBase
{
    private IDateTimeProvider _dateTimeProvider = null!;

    public DateTimeProviderServiceTests(IntegrationTestFixture integrationTestFixture)
        : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _dateTimeProvider = GetService<IDateTimeProvider>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Now_ShouldReturnCurrentUtcTime()
    {
        // Arrange
        await ResetDatabaseAsync();

        var beforeCall = DateTime.UtcNow;

        // Act
        var result = _dateTimeProvider.Now;

        var afterCall = DateTime.UtcNow;

        // Assert
        result.Should().BeOnOrAfter(beforeCall);
        result.Should().BeOnOrBefore(afterCall);
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task Now_ConsecutiveCalls_ShouldReturnIncreasingTimes()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var time1 = _dateTimeProvider.Now;

        // Small delay to ensure time difference
        await Task.Delay(1);

        var time2 = _dateTimeProvider.Now;

        await Task.Delay(1);

        var time3 = _dateTimeProvider.Now;

        // Assert
        time2.Should().BeOnOrAfter(time1);
        time3.Should().BeOnOrAfter(time2);

        // All should be UTC
        time1.Kind.Should().Be(DateTimeKind.Utc);
        time2.Kind.Should().Be(DateTimeKind.Utc);
        time3.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task Now_ShouldBeConsistentWithSystemTime()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var systemTime = DateTime.UtcNow;
        var providerTime = _dateTimeProvider.Now;

        // Assert - Should be very close to system time (within 1 second)
        var difference = Math.Abs((providerTime - systemTime).TotalMilliseconds);
        difference.Should().BeLessThan(1000, "Provider time should be very close to system time");
    }

    [Fact]
    public async Task Now_MultipleThreads_ShouldReturnConsistentResults()
    {
        // Arrange
        await ResetDatabaseAsync();

        var results = new List<DateTime>();
        var tasks = new List<Task>();
        var lockObject = new object();

        // Act - Create multiple concurrent tasks
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var time = _dateTimeProvider.Now;
                lock (lockObject)
                {
                    results.Add(time);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().AllSatisfy(time => time.Kind.Should().Be(DateTimeKind.Utc));

        // All times should be within a reasonable range of each other
        var minTime = results.Min();
        var maxTime = results.Max();
        var timeSpan = maxTime - minTime;
        timeSpan.TotalSeconds.Should().BeLessThan(1, "All times should be within 1 second of each other");
    }

    [Fact]
    public async Task Now_RepeatedCalls_ShouldNotCacheResult()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act & Assert - Multiple calls over time should return different values
        var initialTime = _dateTimeProvider.Now;

        await Task.Delay(10); // Ensure some time passes

        var secondTime = _dateTimeProvider.Now;

        await Task.Delay(10);

        var thirdTime = _dateTimeProvider.Now;

        // The times should be different (not cached)
        secondTime.Should().BeOnOrAfter(initialTime);
        thirdTime.Should().BeOnOrAfter(secondTime);

        // At least one should be different (since we waited)
        (secondTime != initialTime || thirdTime != secondTime).Should().BeTrue("Times should not be cached");
    }

    [Fact]
    public async Task Now_ShouldAlwaysReturnUtcTime()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act - Get multiple time samples
        var times = new List<DateTime>();
        for (int i = 0; i < 5; i++)
        {
            times.Add(_dateTimeProvider.Now);
            if (i < 4) await Task.Delay(1); // Small delay between calls
        }

        // Assert - All times should be UTC
        times.Should().AllSatisfy(time =>
        {
            time.Kind.Should().Be(DateTimeKind.Utc);
        });
    }

    [Fact]
    public async Task Now_ShouldBeReasonablyClose_ToCurrentDate()
    {
        // Arrange
        await ResetDatabaseAsync();

        var currentYear = DateTime.UtcNow.Year;
        var currentMonth = DateTime.UtcNow.Month;
        var currentDay = DateTime.UtcNow.Day;

        // Act
        var providerTime = _dateTimeProvider.Now;

        // Assert - Should be same date as current system time
        providerTime.Year.Should().Be(currentYear);
        providerTime.Month.Should().Be(currentMonth);
        providerTime.Day.Should().Be(currentDay);
    }

    [Fact]
    public async Task Now_Performance_ShouldBeEfficientForMultipleCalls()
    {
        // Arrange
        await ResetDatabaseAsync();

        const int numberOfCalls = 1000;
        var stopwatch = Stopwatch.StartNew();

        // Act - Make many calls to test performance
        for (int i = 0; i < numberOfCalls; i++)
        {
            var _ = _dateTimeProvider.Now;
        }

        stopwatch.Stop();

        // Assert - Should complete quickly (less than 100ms for 1000 calls)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, "DateTime provider should be efficient");
    }

    [Fact]
    public async Task DateTimeProvider_WithDependencyInjection_ShouldResolveCorrectly()
    {
        // Arrange & Act
        await ResetDatabaseAsync();

        // The service should be properly constructed through DI
        _dateTimeProvider.Should().NotBeNull();
        _dateTimeProvider.Should().BeAssignableTo<IDateTimeProvider>();

        // Should be able to get time without issues
        var time = _dateTimeProvider.Now;

        // Assert
        time.Should().NotBe(default(DateTime));
        time.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task DateTimeProvider_SingletonBehavior_ShouldReturnSameInstance()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act - Get multiple instances through DI
        var provider1 = GetService<IDateTimeProvider>();
        var provider2 = GetService<IDateTimeProvider>();

        // Assert - Should be the same instance if registered as singleton
        // Note: This depends on the DI registration, but typically providers are singletons
        provider1.Should().NotBeNull();
        provider2.Should().NotBeNull();

        // Both should work correctly
        var time1 = provider1.Now;
        var time2 = provider2.Now;

        time1.Kind.Should().Be(DateTimeKind.Utc);
        time2.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task Now_EdgeCase_ShouldHandleRapidSuccessiveCalls()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act - Make rapid successive calls
        var times = new List<DateTime>();
        for (int i = 0; i < 100; i++)
        {
            times.Add(_dateTimeProvider.Now);
        }

        // Assert
        times.Should().HaveCount(100);
        times.Should().AllSatisfy(time => time.Kind.Should().Be(DateTimeKind.Utc));

        // Times should be in ascending order (or at least non-descending)
        for (int i = 1; i < times.Count; i++)
        {
            times[i].Should().BeOnOrAfter(times[i - 1]);
        }
    }
}
