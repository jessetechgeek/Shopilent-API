using System.Reflection;
using Shopilent.Domain.Catalog;

namespace Shopilent.ArchitectureTests.DesignPatterns;

public class RepositoryPatternTests
{
    private static readonly Assembly DomainAssembly = typeof(Product).Assembly;

    [Fact]
    public void ReadRepositories_Should_EndWith_ReadRepository()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .And().HaveNameMatching(@".*Read.*Repository.*")
            .And().DoNotHaveNameMatching(@"^IReadRepository`?\d*$") // Exclude base interface
            .And().DoNotHaveNameMatching(@"^IAggregateReadRepository`?\d*$") // Exclude base interface
            .And().DoNotHaveNameMatching(@"^IEntityReadRepository`?\d*$") // Exclude base interface
            .Should()
            .HaveNameEndingWith("ReadRepository")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All concrete read repository interfaces should end with 'ReadRepository'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void WriteRepositories_Should_EndWith_WriteRepository()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .And().HaveNameMatching(@".*Write.*Repository.*")
            .And().DoNotHaveNameMatching(@"^IWriteRepository`?\d*$") // Exclude base interface
            .And().DoNotHaveNameMatching(@"^IAggregateWriteRepository`?\d*$") // Exclude base interface
            .And().DoNotHaveNameMatching(@"^IEntityWriteRepository`?\d*$") // Exclude base interface
            .Should()
            .HaveNameEndingWith("WriteRepository")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All concrete write repository interfaces should end with 'WriteRepository'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Repository_Interfaces_Should_Be_In_Domain_Layer()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .And().HaveNameEndingWith("Repository")
            .Should()
            .ResideInNamespaceMatching(@"Shopilent\.Domain\..*")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All repository interfaces should be in Domain layer. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Repository_Interfaces_Should_Be_Public()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .And().HaveNameEndingWith("Repository")
            .Should()
            .BePublic()
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All repository interfaces should be public. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Repository_Interfaces_Should_Be_In_Repositories_Namespace()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .And().HaveNameEndingWith("Repository")
            .Should()
            .ResideInNamespaceMatching(@".*\.Repositories(\.(Read|Write))?$")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All repository interfaces should be in Repositories namespace (or Repositories.Read/Write). Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Repository_Interfaces_Should_Start_With_I()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .And().HaveNameEndingWith("Repository")
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All repository interfaces should start with 'I'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Each_Aggregate_Should_Have_Both_Read_And_Write_Repository()
    {
        var aggregateRoots = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(object))
            .And().HaveNameEndingWith("AggregateRoot")
            .Or().HaveCustomAttribute(typeof(System.ComponentModel.DataAnnotations.KeyAttribute))
            .GetTypes()
            .Where(t => t.Name != "AggregateRoot")
            .ToList();

        var repositories = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .And().HaveNameEndingWith("Repository")
            .GetTypes()
            .ToList();

        var missingRepositories = new List<string>();

        foreach (var aggregate in aggregateRoots)
        {
            var aggregateName = aggregate.Name.Replace("AggregateRoot", "");

            var expectedReadRepository = $"I{aggregateName}ReadRepository";
            var expectedWriteRepository = $"I{aggregateName}WriteRepository";

            var hasReadRepository = repositories.Any(r => r.Name == expectedReadRepository);
            var hasWriteRepository = repositories.Any(r => r.Name == expectedWriteRepository);

            if (!hasReadRepository)
            {
                missingRepositories.Add($"Missing read repository: {expectedReadRepository} for {aggregate.Name}");
            }

            if (!hasWriteRepository)
            {
                missingRepositories.Add($"Missing write repository: {expectedWriteRepository} for {aggregate.Name}");
            }
        }

        missingRepositories.Should().BeEmpty(
            "Each aggregate should have both read and write repositories. Missing: {0}",
            string.Join(", ", missingRepositories));
    }

    [Fact]
    public void Repository_Methods_Should_Return_Tasks_For_Async_Operations()
    {
        var repositories = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .And().HaveNameEndingWith("Repository")
            .GetTypes();

        var synchronousMethods = new List<string>();

        foreach (var repository in repositories)
        {
            var methods = repository.GetMethods()
                .Where(m => m.IsPublic && !m.IsSpecialName)
                .ToList();

            foreach (var method in methods)
            {
                if (method.ReturnType != typeof(Task) &&
                    !method.ReturnType.IsGenericType ||
                    (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() != typeof(Task<>)))
                {
                    synchronousMethods.Add($"{repository.Name}.{method.Name}");
                }
            }
        }

        synchronousMethods.Should().BeEmpty(
            "All repository methods should return Task or Task<T> for async operations. Synchronous methods: {0}",
            string.Join(", ", synchronousMethods));
    }

    [Fact]
    public void Read_Repositories_Should_Not_Have_Mutating_Methods()
    {
        var readRepositories = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .And().HaveNameMatching(@".*ReadRepository")
            .GetTypes();

        var mutatingMethodNames = new[] { "Add", "Create", "Insert", "Update", "Delete", "Remove", "Save" };
        var violations = new List<string>();

        foreach (var repository in readRepositories)
        {
            var methods = repository.GetMethods()
                .Where(m => m.IsPublic && !m.IsSpecialName)
                .ToList();

            foreach (var method in methods)
            {
                if (mutatingMethodNames.Any(mutating =>
                        method.Name.StartsWith(mutating, StringComparison.OrdinalIgnoreCase)))
                {
                    violations.Add($"{repository.Name}.{method.Name}");
                }
            }
        }

        violations.Should().BeEmpty(
            "Read repositories should not contain mutating methods. Violations: {0}",
            string.Join(", ", violations));
    }

    [Fact]
    public void Write_Repositories_Should_Inherit_From_Base_Repository()
    {
        var writeRepositories = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .And().HaveNameMatching(@".*WriteRepository")
            .And().DoNotHaveNameMatching(@"^IWriteRepository`?\d*$") // Exclude base interface
            .And().DoNotHaveNameMatching(@"^IAggregateWriteRepository`?\d*$") // Exclude base interface
            .And().DoNotHaveNameMatching(@"^IEntityWriteRepository`?\d*$") // Exclude base interface
            .GetTypes();

        var repositoriesWithoutBaseInterface = new List<string>();

        foreach (var repository in writeRepositories)
        {
            // Check if repository inherits from IWriteRepository<T> or similar base interface
            var inheritsFromWriteRepository = repository.GetInterfaces()
                                                  .Any(i => i.IsGenericType &&
                                                            (i.GetGenericTypeDefinition().Name
                                                                 .Contains("IWriteRepository") ||
                                                             i.GetGenericTypeDefinition().Name
                                                                 .Contains("IAggregateWriteRepository") ||
                                                             i.GetGenericTypeDefinition().Name
                                                                 .Contains("IEntityWriteRepository"))) ||
                                              repository.GetInterfaces()
                                                  .Any(i => i.Name.Contains("IWriteRepository"));

            if (!inheritsFromWriteRepository)
            {
                repositoriesWithoutBaseInterface.Add(repository.Name);
            }
        }

        repositoriesWithoutBaseInterface.Should().BeEmpty(
            $"Write repositories should inherit from base repository interface (IWriteRepository<T> or similar). Missing inheritance: {string.Join(", ", repositoriesWithoutBaseInterface)}");
    }

    [Fact]
    public void Repository_Methods_Should_Accept_CancellationToken()
    {
        var repositories = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .And().HaveNameEndingWith("Repository")
            .GetTypes();

        var methodsWithoutCancellationToken = new List<string>();

        foreach (var repository in repositories)
        {
            var methods = repository.GetMethods()
                .Where(m => m.IsPublic && !m.IsSpecialName)
                .ToList();

            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                var hasCancellationToken = parameters.Any(p => p.ParameterType == typeof(CancellationToken));

                if (!hasCancellationToken)
                {
                    methodsWithoutCancellationToken.Add($"{repository.Name}.{method.Name}");
                }
            }
        }

        methodsWithoutCancellationToken.Should().BeEmpty(
            "All repository methods should accept CancellationToken parameter. Methods without CancellationToken: {0}",
            string.Join(", ", methodsWithoutCancellationToken));
    }
}
