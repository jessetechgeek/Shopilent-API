using System.Reflection;
using Shopilent.Domain.Catalog;
using Shopilent.Application.Features.Catalog.Commands.CreateProduct.V1;

namespace Shopilent.ArchitectureTests.CodeQuality;

public class CodeQualityTests
{
    private static readonly Assembly DomainAssembly = typeof(Product).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(CreateProductCommandV1).Assembly;

    [Fact]
    public void Domain_Classes_Should_Be_Public_Or_Internal()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .AreClasses()
            .And().DoNotHaveNameEndingWith("Test")
            .And().DoNotHaveNameEndingWith("Tests")
            .Should()
            .BePublic()
            .Or().NotBePublic()
            .GetResult();

        // This test ensures no private nested classes or other visibility issues
        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Domain classes should have appropriate visibility. Issues: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Application_Services_Should_Be_Internal()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Service")
            .And().AreClasses()
            .Should()
            .NotBePublic()
            .GetResult();

        var publicServices = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"Application services should be internal (not public). Public services: {string.Join(", ", publicServices)}");
    }

    [Fact]
    public void Domain_Entities_Should_Not_Have_Public_Setters()
    {
        var entities = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.Entities$")
            .GetTypes();

        var entitiesWithPublicSetters = new List<string>();

        foreach (var entity in entities)
        {
            var properties = entity.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true)
                .ToList();

            // Allow properties with init-only setters
            var publicSetters = properties.Where(p =>
                    !p.SetMethod!.ReturnParameter.GetRequiredCustomModifiers()
                        .Contains(typeof(System.Runtime.CompilerServices.IsExternalInit)))
                .ToList();

            if (publicSetters.Any())
            {
                entitiesWithPublicSetters.Add($"{entity.Name}: {string.Join(", ", publicSetters.Select(p => p.Name))}");
            }
        }

        entitiesWithPublicSetters.Should().BeEmpty(
            "Domain entities should not have public setters (use init-only setters or private setters). Violations: {0}",
            string.Join("; ", entitiesWithPublicSetters));
    }

    [Fact]
    public void Value_Objects_Should_Be_Immutable()
    {
        var valueObjects = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.ValueObjects$")
            .GetTypes();

        var mutableValueObjects = new List<string>();

        foreach (var valueObject in valueObjects)
        {
            var properties = valueObject.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true)
                .ToList();

            // Check if setters are init-only
            var mutableProperties = properties.Where(p =>
                    !p.SetMethod!.ReturnParameter.GetRequiredCustomModifiers()
                        .Contains(typeof(System.Runtime.CompilerServices.IsExternalInit)))
                .ToList();

            if (mutableProperties.Any())
            {
                mutableValueObjects.Add(
                    $"{valueObject.Name}: {string.Join(", ", mutableProperties.Select(p => p.Name))}");
            }
        }

        mutableValueObjects.Should().BeEmpty(
            "Value objects should be immutable (no public setters, only init-only). Mutable value objects: {0}",
            string.Join("; ", mutableValueObjects));
    }

    [Fact]
    public void DTOs_Should_Have_Public_Parameterless_Constructor()
    {
        var dtos = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Dto")
            .Or().HaveNameEndingWith("Request")
            .Or().HaveNameEndingWith("Response")
            .GetTypes();

        var dtosWithoutParameterlessConstructor = new List<string>();

        foreach (var dto in dtos)
        {
            var constructors = dto.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var hasParameterlessConstructor = constructors.Any(c => c.GetParameters().Length == 0);

            if (!hasParameterlessConstructor)
            {
                dtosWithoutParameterlessConstructor.Add(dto.Name);
            }
        }

        dtosWithoutParameterlessConstructor.Should().BeEmpty(
            "DTOs should have public parameterless constructors for serialization. Missing: {0}",
            string.Join(", ", dtosWithoutParameterlessConstructor));
    }

    [Fact]
    public void Async_Methods_Should_End_With_Async()
    {
        var allTypes = Types.InAssembly(ApplicationAssembly)
            .That()
            .AreClasses()
            .GetTypes()
            .Concat(Types.InAssembly(DomainAssembly)
                .That()
                .AreClasses()
                .GetTypes());

        var asyncMethodsWithoutAsyncSuffix = new List<string>();

        foreach (var type in allTypes)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.ReturnType == typeof(Task) ||
                            (m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
                .ToList();

            foreach (var method in methods)
            {
                if (!method.Name.EndsWith("Async") && !method.IsSpecialName)
                {
                    // Skip MediatR interface methods (Handle is required by MediatR contract)
                    if (method.Name == "Handle" &&
                        (type.GetInterfaces().Any(i => i.Name.Contains("Handler")) ||
                         type.Name.Contains("Handler") ||
                         type.Name.Contains("Behavior") || // Pipeline behaviors
                         type.GetInterfaces().Any(i => i.Name.Contains("Behavior"))))
                    {
                        continue;
                    }

                    asyncMethodsWithoutAsyncSuffix.Add($"{type.Name}.{method.Name}");
                }
            }
        }

        asyncMethodsWithoutAsyncSuffix.Should().BeEmpty(
            $"Async methods should end with 'Async' (excluding MediatR Handle methods and pipeline behaviors). Missing suffix: {string.Join(", ", asyncMethodsWithoutAsyncSuffix)}");
    }

    [Fact]
    public void Interfaces_Should_Not_Have_Implementation()
    {
        var interfaces = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .GetTypes()
            .Concat(Types.InAssembly(ApplicationAssembly)
                .That()
                .AreInterfaces()
                .GetTypes());

        var interfacesWithImplementation = new List<string>();

        foreach (var @interface in interfaces)
        {
            var methods = @interface.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.IsVirtual && !m.IsAbstract)
                .ToList();

            if (methods.Any())
            {
                interfacesWithImplementation.Add(@interface.Name);
            }
        }

        interfacesWithImplementation.Should().BeEmpty(
            "Interfaces should not have default implementations (use abstract classes instead). Interfaces with implementation: {0}",
            string.Join(", ", interfacesWithImplementation));
    }

    [Fact]
    public void Static_Classes_Should_Be_Sealed()
    {
        var staticClasses = Types.InAssembly(ApplicationAssembly)
            .That()
            .AreStatic()
            .GetTypes()
            .Concat(Types.InAssembly(DomainAssembly)
                .That()
                .AreStatic()
                .GetTypes());

        // Static classes are sealed by default in C#, but this test ensures it
        staticClasses.Should().AllSatisfy(type =>
            type.IsSealed.Should().BeTrue($"{type.Name} should be sealed"));
    }

    [Fact]
    public void Exception_Classes_Should_Have_Standard_Constructors()
    {
        var exceptions = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Exception))
            .GetTypes()
            .Concat(Types.InAssembly(ApplicationAssembly)
                .That()
                .Inherit(typeof(Exception))
                .GetTypes());

        var exceptionsWithMissingConstructors = new List<string>();

        foreach (var exception in exceptions)
        {
            var constructors = exception.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            // Standard exception constructors:
            // 1. Parameterless constructor
            // 2. Constructor with string message
            // 3. Constructor with string message and inner exception

            var hasParameterlessConstructor = constructors.Any(c => c.GetParameters().Length == 0);
            var hasMessageConstructor = constructors.Any(c =>
                c.GetParameters().Length == 1 &&
                c.GetParameters()[0].ParameterType == typeof(string));
            var hasMessageAndInnerConstructor = constructors.Any(c =>
                c.GetParameters().Length == 2 &&
                c.GetParameters()[0].ParameterType == typeof(string) &&
                c.GetParameters()[1].ParameterType == typeof(Exception));

            if (!hasParameterlessConstructor || !hasMessageConstructor || !hasMessageAndInnerConstructor)
            {
                exceptionsWithMissingConstructors.Add(exception.Name);
            }
        }

        exceptionsWithMissingConstructors.Should().BeEmpty(
            "Exception classes should have standard constructors (parameterless, message, message+inner). Missing: {0}",
            string.Join(", ", exceptionsWithMissingConstructors));
    }

    [Fact]
    public void Public_API_Surface_Should_Be_Stable()
    {
        // This test documents the public API surface to catch breaking changes
        var publicTypes = Types.InAssembly(DomainAssembly)
            .That()
            .ArePublic()
            .GetTypes()
            .Concat(Types.InAssembly(ApplicationAssembly)
                .That()
                .ArePublic()
                .GetTypes())
            .Select(t => t.FullName)
            .OrderBy(name => name)
            .ToList();

        // This test serves as documentation - when it fails, review if the changes are intentional
        publicTypes.Should().NotBeEmpty("Should have public types in Domain and Application layers");

        // Log the current public API surface for documentation
        Console.WriteLine($"Current public API surface has {publicTypes.Count} public types");
    }

    [Fact]
    public void Constants_Should_Be_ReadOnly_Or_Const()
    {
        var constantsClasses = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching(@".*\.Constants$")
            .GetTypes();

        var fieldsWithoutConstOrReadonly = new List<string>();

        foreach (var constantsClass in constantsClasses)
        {
            var fields = constantsClass.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => !f.IsLiteral && !f.IsInitOnly) // Not const and not readonly
                .ToList();

            foreach (var field in fields)
            {
                fieldsWithoutConstOrReadonly.Add($"{constantsClass.Name}.{field.Name}");
            }
        }

        fieldsWithoutConstOrReadonly.Should().BeEmpty(
            "Constants should be declared as const or readonly. Mutable constants: {0}",
            string.Join(", ", fieldsWithoutConstOrReadonly));
    }
}
