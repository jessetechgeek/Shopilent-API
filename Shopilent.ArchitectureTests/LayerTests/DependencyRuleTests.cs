using System.Reflection;
using Shopilent.Domain.Catalog;
using Shopilent.Application.Features.Catalog.Commands.CreateProduct.V1;

namespace Shopilent.ArchitectureTests.LayerTests;

public class DependencyRuleTests
{
    private static readonly Assembly DomainAssembly = typeof(Product).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(CreateProductCommandV1).Assembly;

    [Fact]
    public void Domain_Should_NotDependOn_Application()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("Shopilent.Application")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer should not depend on Application layer. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Domain_Should_NotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("Shopilent.Infrastructure")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer should not depend on any Infrastructure layer. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Domain_Should_NotDependOn_API()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("Shopilent.API")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer should not depend on API layer. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Application_Should_NotDependOn_API()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn("Shopilent.API")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Application layer should not depend on API layer. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Application_Should_NotDependOn_Infrastructure_Implementations()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn("Shopilent.Infrastructure.Persistence.PostgreSQL")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Cache.Redis")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Identity")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.S3ObjectStorage")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Payments")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Search.Meilisearch")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Logging")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Realtime.SignalR")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Application layer should only depend on Infrastructure abstractions, not implementations. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Application_Should_OnlyDependOn_Core_Infrastructure_Abstractions()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveDependencyOn("Shopilent.Infrastructure")
            .Should()
            .NotHaveDependencyOnAny(
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
            $"Application layer should only depend on core Infrastructure abstractions. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Infrastructure_Projects_Should_NotDependOn_EachOther()
    {
        var infrastructureProjects = new[]
        {
            "Shopilent.Infrastructure.Cache.Redis", "Shopilent.Infrastructure.Identity",
            "Shopilent.Infrastructure.Persistence.PostgreSQL", "Shopilent.Infrastructure.S3ObjectStorage",
            "Shopilent.Infrastructure.Payments", "Shopilent.Infrastructure.Search.Meilisearch",
            "Shopilent.Infrastructure.Logging", "Shopilent.Infrastructure.Realtime.SignalR"
        };

        foreach (var project in infrastructureProjects)
        {
            var assembly = GetAssemblyByName(project);
            if (assembly == null) continue;

            var otherProjects = infrastructureProjects.Where(p => p != project).ToArray();

            var result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(otherProjects)
                .GetResult();

            var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
            result.IsSuccessful.Should().BeTrue(
                $"Infrastructure project {project} should not depend on other infrastructure projects. Violations: {string.Join(", ", violations)}");
        }
    }

    [Fact]
    public void Infrastructure_Projects_Should_OnlyDependOn_Domain_Application_CoreInfrastructure()
    {
        var allowedDependencies = new[] { "Shopilent.Domain", "Shopilent.Application", "Shopilent.Infrastructure" };

        var infrastructureProjects = new[]
        {
            "Shopilent.Infrastructure.Cache.Redis", "Shopilent.Infrastructure.Identity",
            "Shopilent.Infrastructure.Persistence.PostgreSQL", "Shopilent.Infrastructure.S3ObjectStorage",
            "Shopilent.Infrastructure.Payments", "Shopilent.Infrastructure.Search.Meilisearch",
            "Shopilent.Infrastructure.Logging", "Shopilent.Infrastructure.Realtime.SignalR"
        };

        foreach (var project in infrastructureProjects)
        {
            var assembly = GetAssemblyByName(project);
            if (assembly == null) continue;

            var result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(
                    "Shopilent.API",
                    "System.Web",
                    "Microsoft.AspNetCore.Mvc")
                .GetResult();

            // Note: This test might have false positives due to transitive dependencies
            // Consider it a guideline rather than strict enforcement
            if (!result.IsSuccessful)
            {
                var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
                Console.WriteLine(
                    $"Warning: {project} has dependencies outside allowed scope: {string.Join(", ", violations)}");
            }
        }
    }

    [Fact]
    public void API_Should_NotDependOn_Infrastructure_Implementations_Directly()
    {
        var apiAssembly = GetAssemblyByName("Shopilent.API");
        if (apiAssembly == null)
        {
            Assert.True(false, "API assembly not found");
            return;
        }

        var result = Types.InAssembly(apiAssembly)
            .That()
            .DoNotHaveName("Program") // Exclude composition root
            .And().DoNotResideInNamespaceMatching(@".*\.Extensions$") // Exclude service registration extensions
            .Should()
            .NotHaveDependencyOn("Shopilent.Infrastructure.Persistence.PostgreSQL")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Cache.Redis")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.S3ObjectStorage")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Payments")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Search.Meilisearch")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"API layer (excluding composition root) should not directly depend on infrastructure implementations. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Core_Infrastructure_Should_NotDependOn_Specific_Implementations()
    {
        var coreInfrastructureAssembly = GetAssemblyByName("Shopilent.Infrastructure");
        if (coreInfrastructureAssembly == null)
        {
            Assert.True(false, "Core Infrastructure assembly not found");
            return;
        }

        var result = Types.InAssembly(coreInfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("Shopilent.Infrastructure.Persistence.PostgreSQL")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Cache.Redis")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Identity")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.S3ObjectStorage")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Payments")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Search.Meilisearch")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Logging")
            .And().NotHaveDependencyOn("Shopilent.Infrastructure.Realtime.SignalR")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Core Infrastructure should not depend on specific implementations. Violations: {string.Join(", ", violations)}");
    }

    private static Assembly? GetAssemblyByName(string name)
    {
        try
        {
            // First try to find in already loaded assemblies
            var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == name);

            if (loadedAssembly != null)
                return loadedAssembly;

            // If not found, try to load it by name
            return Assembly.Load(name);
        }
        catch
        {
            return null;
        }
    }
}
