using System.Reflection;
using Shopilent.Domain.Catalog;
using Shopilent.Application.Features.Catalog.Commands.CreateProduct.V1;

namespace Shopilent.ArchitectureTests.NamingConventions;

public class NamingConventionTests
{
    private static readonly Assembly DomainAssembly = typeof(Product).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(CreateProductCommandV1).Assembly;

    [Fact]
    public void Interfaces_Should_StartWith_I()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All interfaces in Domain should start with 'I'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Application_Interfaces_Should_StartWith_I()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All interfaces in Application should start with 'I'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Entities_Should_BeIn_Entities_Namespace()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .AreClasses()
            .And().DoNotHaveNameEndingWith("Test")
            .And().DoNotHaveNameEndingWith("Tests")
            .And().ResideInNamespaceMatching(@".*\.Entities$")
            .Should()
            .ResideInNamespaceMatching(@".*\.Entities$")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Entity classes should be in Entities namespace. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void ValueObjects_Should_BeIn_ValueObjects_Namespace()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.ValueObjects$")
            .Should()
            .BeClasses()
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Types in ValueObjects namespace should be classes. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Services_Should_EndWith_Service()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.Services$")
            .And().AreClasses()
            .Should()
            .HaveNameEndingWith("Service")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Service classes should end with 'Service'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Validators_Should_EndWith_Validator()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Validator")
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Validator classes should end with 'Validator'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void RequestDTOs_Should_EndWith_Request()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Request")
            .Should()
            .BeClasses()
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Request DTOs should be classes and end with 'Request'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void ResponseDTOs_Should_EndWith_Response()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Response")
            .Should()
            .BeClasses()
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Response DTOs should be classes and end with 'Response'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void DTOs_Should_EndWith_Dto()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Dto")
            .Should()
            .BeClasses()
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"DTO classes should end with 'Dto'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Exceptions_Should_EndWith_Exception()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Exception))
            .Should()
            .HaveNameEndingWith("Exception")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Exception classes should end with 'Exception'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Application_Exceptions_Should_EndWith_Exception()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(Exception))
            .Should()
            .HaveNameEndingWith("Exception")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Application exception classes should end with 'Exception'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Specifications_Should_EndWith_Specification()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.Specifications$")
            .And().DoNotHaveNameMatching(@"^Specification`?\d*$") // Exclude base Specification class
            .And().DoNotHaveNameMatching(@"^AndSpecification`?\d*$") // Exclude base AndSpecification class
            .And().DoNotHaveNameMatching(@"^OrSpecification`?\d*$") // Exclude base OrSpecification class
            .And().DoNotHaveNameMatching(@"^NotSpecification`?\d*$") // Exclude base NotSpecification class
            .Should()
            .HaveNameEndingWith("Specification")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Concrete specification classes should end with 'Specification'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Constants_Should_Be_In_Constants_Namespace()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.Constants$")
            .Should()
            .BeClasses()
            .And().BeSealed()
            .And().BePublic()
            .GetResult();

        // Constants classes should be sealed and public
        if (!result.IsSuccessful)
        {
            var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
            Console.WriteLine($"Constants classes should be sealed and public: {string.Join(", ", violations)}");
        }
    }

    [Fact]
    public void Extension_Classes_Should_Be_Static()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Extensions")
            .Should()
            .BeStatic()
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Extension classes should be static. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Domain_Extension_Classes_Should_Be_Static()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .HaveNameEndingWith("Extensions")
            .Should()
            .BeStatic()
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Domain extension classes should be static. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Abstract_Classes_Should_Be_Abstract()
    {
        var abstractClasses = Types.InAssembly(DomainAssembly)
            .That()
            .HaveNameStartingWith("Abstract")
            .Or().HaveNameStartingWith("Base")
            .GetTypes();

        var nonAbstractClasses = abstractClasses
            .Where(t => !t.IsAbstract)
            .Select(t => t.Name)
            .ToList();

        nonAbstractClasses.Should().BeEmpty(
            "Classes with 'Abstract' or 'Base' prefix should be abstract. Non-abstract classes: {0}",
            string.Join(", ", nonAbstractClasses));
    }

    [Fact]
    public void Application_Abstract_Classes_Should_Be_Abstract()
    {
        var abstractClasses = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameStartingWith("Abstract")
            .Or().HaveNameStartingWith("Base")
            .GetTypes();

        var nonAbstractClasses = abstractClasses
            .Where(t => !t.IsAbstract)
            .Select(t => t.Name)
            .ToList();

        nonAbstractClasses.Should().BeEmpty(
            "Application classes with 'Abstract' or 'Base' prefix should be abstract. Non-abstract classes: {0}",
            string.Join(", ", nonAbstractClasses));
    }

    [Fact]
    public void Versioned_Types_Should_Have_Version_In_Name()
    {
        var versionedTypes = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.V\d+$")
            .GetTypes()
            .Where(t => t.Name.EndsWith("Command") ||
                        t.Name.EndsWith("Query") ||
                        t.Name.EndsWith("CommandHandler") ||
                        t.Name.EndsWith("QueryHandler") ||
                        t.Name.EndsWith("Request") ||
                        t.Name.EndsWith("Response")) // Only check main operations
            .ToList();

        var typesWithoutVersionInName = versionedTypes
            .Where(t => !System.Text.RegularExpressions.Regex.IsMatch(t.Name, @"V\d+$"))
            .Select(t => t.Name)
            .ToList();

        typesWithoutVersionInName.Should().BeEmpty(
            $"Main operation types in versioned namespaces should have version suffix in their name. Missing version suffix: {string.Join(", ", typesWithoutVersionInName)}");
    }
}
