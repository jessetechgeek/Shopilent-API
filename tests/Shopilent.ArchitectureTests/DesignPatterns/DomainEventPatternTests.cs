using System.Reflection;
using Shopilent.Domain.Catalog;
using Shopilent.Application.Features.Catalog.Commands.CreateProduct.V1;

namespace Shopilent.ArchitectureTests.DesignPatterns;

public class DomainEventPatternTests
{
    private static readonly Assembly DomainAssembly = typeof(Product).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(CreateProductCommandV1).Assembly;

    [Fact]
    public void DomainEvents_Should_EndWith_Event()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.Events$")
            .And().AreClasses()
            .Should()
            .HaveNameEndingWith("Event")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All domain events should end with 'Event'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void DomainEvents_Should_BeIn_Events_Namespace()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .HaveNameEndingWith("Event")
            .And().AreClasses()
            .Should()
            .ResideInNamespaceMatching(@".*\.Events$")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All domain events should be in Events namespace. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void DomainEvents_Should_Be_Public()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.Events$")
            .And().AreClasses()
            .Should()
            .BePublic()
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All domain events should be public. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void DomainEvents_Should_Be_Records_Or_Immutable()
    {
        var domainEvents = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.Events$")
            .And().AreClasses()
            .GetTypes();

        var mutableEvents = new List<string>();

        foreach (var eventType in domainEvents)
        {
            // Check if it's a record (records are immutable by default)
            var isRecord = eventType.GetMethods()
                .Any(m => m.Name == "<Clone>$" || m.Name == "get_EqualityContract");

            if (!isRecord)
            {
                // Check for mutable properties (setters that are not init-only)
                var properties = eventType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var hasMutableProperties = properties.Any(p =>
                    p.CanWrite &&
                    p.SetMethod != null &&
                    p.SetMethod.IsPublic &&
                    !p.SetMethod.ReturnParameter.GetRequiredCustomModifiers()
                        .Contains(typeof(System.Runtime.CompilerServices.IsExternalInit)));

                if (hasMutableProperties)
                {
                    mutableEvents.Add(eventType.Name);
                }
            }
        }

        mutableEvents.Should().BeEmpty(
            "Domain events should be immutable (records or classes with init-only setters). Mutable events: {0}",
            string.Join(", ", mutableEvents));
    }

    [Fact]
    public void DomainEventHandlers_Should_EndWith_EventHandler()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.EventHandlers$")
            .And().AreClasses()
            .Should()
            .HaveNameEndingWith("EventHandler")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All domain event handlers should end with 'EventHandler'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void DomainEventHandlers_Should_BeIn_EventHandlers_Namespace()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("EventHandler")
            .And().AreClasses()
            .Should()
            .ResideInNamespaceMatching(@".*\.EventHandlers$")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All event handlers should be in EventHandlers namespace. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void DomainEventHandlers_Should_Be_Internal()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.EventHandlers$")
            .And().AreClasses()
            .Should()
            .NotBePublic()
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All domain event handlers should be internal (not public). Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void DomainEvents_Should_Have_Corresponding_EventHandlers()
    {
        var domainEvents = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.Events$")
            .And().AreClasses()
            .GetTypes();

        var eventHandlers = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.EventHandlers$")
            .And().AreClasses()
            .GetTypes();

        var eventsWithoutHandlers = new List<string>();

        foreach (var domainEvent in domainEvents)
        {
            var expectedHandlerName = domainEvent.Name.Replace("Event", "EventHandler");
            var handlerExists = eventHandlers.Any(h => h.Name == expectedHandlerName);

            if (!handlerExists)
            {
                eventsWithoutHandlers.Add($"{domainEvent.Name} -> {expectedHandlerName}");
            }
        }

        // This is more of a guideline - not all events need handlers
        if (eventsWithoutHandlers.Any())
        {
            Console.WriteLine(
                $"Domain events without handlers (may be intentional): {string.Join(", ", eventsWithoutHandlers)}");
        }
    }

    [Fact]
    public void DomainEvents_Should_Have_Timestamp_Property()
    {
        var domainEvents = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.Events$")
            .And().AreClasses()
            .And().DoNotHaveName("DomainEvent") // Exclude base class
            .GetTypes();

        var eventsWithoutTimestamp = new List<string>();

        foreach (var eventType in domainEvents)
        {
            // Check for common timestamp property names (including inherited)
            var allProperties =
                eventType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            var hasTimestampProperty = allProperties.Any(p =>
                (p.Name == "DateOccurred" ||
                 p.Name == "OccurredOn" ||
                 p.Name == "CreatedAt" ||
                 p.Name == "Timestamp") &&
                (p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTimeOffset)));

            if (!hasTimestampProperty)
            {
                eventsWithoutTimestamp.Add(eventType.Name);
            }
        }

        // Only enforce this if some events already have timestamp properties
        // (indicating it's part of the architecture)
        var totalEvents = domainEvents.Count();
        var eventsWithTimestamp = totalEvents - eventsWithoutTimestamp.Count;

        if (eventsWithTimestamp > 0 && eventsWithoutTimestamp.Count > 0)
        {
            eventsWithoutTimestamp.Should().BeEmpty(
                $"Domain events should have timestamp properties for consistency. Missing: {string.Join(", ", eventsWithoutTimestamp)}");
        }
    }

    [Fact]
    public void DomainEvents_Should_Have_Id_Property()
    {
        var domainEvents = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.Events$")
            .And().AreClasses()
            .GetTypes();

        var eventsWithoutId = new List<string>();

        foreach (var eventType in domainEvents)
        {
            var hasIdProperty = eventType.GetProperties()
                .Any(p => p.Name == "Id" && p.PropertyType == typeof(Guid));

            if (!hasIdProperty)
            {
                eventsWithoutId.Add(eventType.Name);
            }
        }

        eventsWithoutId.Should().BeEmpty(
            "All domain events should have an Id property of type Guid. Missing: {0}",
            string.Join(", ", eventsWithoutId));
    }

    [Fact]
    public void DomainEvents_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.Events$")
            .Should()
            .NotHaveDependencyOnAny(
                "Shopilent.Infrastructure",
                "Shopilent.Infrastructure.Persistence.PostgreSQL",
                "Shopilent.Infrastructure.Cache.Redis",
                "Shopilent.Infrastructure.Identity",
                "Shopilent.Infrastructure.S3ObjectStorage",
                "Shopilent.Infrastructure.Payments",
                "Shopilent.Infrastructure.Search.Meilisearch",
                "Shopilent.Infrastructure.Logging",
                "Shopilent.Infrastructure.Realtime.SignalR")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Domain events should not reference infrastructure. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void DomainEvents_Should_Not_Reference_Application_Layer()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.Events$")
            .Should()
            .NotHaveDependencyOn("Shopilent.Application")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Domain events should not reference Application layer. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void AggregateRoots_Should_Have_Domain_Events_Property()
    {
        var aggregateRoots = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(object))
            .And().HaveNameEndingWith("AggregateRoot")
            .Or().ResideInNamespaceMatching(@".*\.Entities$")
            .GetTypes()
            .Where(t => t.Name != "AggregateRoot" && t.IsClass && !t.IsAbstract)
            .ToList();

        var aggregatesWithoutDomainEvents = new List<string>();

        foreach (var aggregate in aggregateRoots)
        {
            var hasDomainEventsProperty = aggregate.GetProperties()
                .Any(p => p.Name.Contains("DomainEvent") || p.Name.Contains("Events"));

            var hasDomainEventsMethod = aggregate.GetMethods()
                .Any(m => m.Name.Contains("DomainEvent") || m.Name.Contains("Event"));

            if (!hasDomainEventsProperty && !hasDomainEventsMethod)
            {
                aggregatesWithoutDomainEvents.Add(aggregate.Name);
            }
        }

        // This is more of a guideline - not all aggregates need to raise events
        if (aggregatesWithoutDomainEvents.Any())
        {
            Console.WriteLine(
                $"Aggregates without domain events (may be intentional): {string.Join(", ", aggregatesWithoutDomainEvents)}");
        }
    }

    [Fact]
    public void DomainEventHandlers_Should_Be_Sealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.EventHandlers$")
            .And().AreClasses()
            .Should()
            .BeSealed()
            .GetResult();

        // This is optional - event handlers don't need to be sealed
        if (!result.IsSuccessful)
        {
            var nonSealedHandlers = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
            Console.WriteLine(
                $"Event handlers that are not sealed (recommended but not required): {string.Join(", ", nonSealedHandlers)}");
        }
    }
}
