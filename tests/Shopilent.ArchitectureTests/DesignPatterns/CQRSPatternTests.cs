using System.Reflection;
using Shopilent.Application.Features.Catalog.Commands.CreateProduct.V1;
using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.ArchitectureTests.DesignPatterns;

public class CQRSPatternTests
{
    private static readonly Assembly ApplicationAssembly = typeof(CreateProductCommandV1).Assembly;

    [Fact]
    public void Commands_Should_EndWith_Command()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Or().ImplementInterface(typeof(ICommand<>))
            .Should()
            .HaveNameMatching(@".*CommandV\d+$")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All command types should end with 'CommandV{{version}}'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Queries_Should_EndWith_Query()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .And().DoNotHaveNameMatching(@"^ICachedQuery`?\d*$") // Exclude ICachedQuery interface
            .And().AreNotInterfaces() // Exclude interface definitions
            .And().DoNotResideInNamespaceMatching(@".*\.Abstractions\..*") // Exclude abstractions
            .Should()
            .HaveNameMatching(@".*QueryV\d+$")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All query types should end with 'QueryV{{version}}'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void CommandHandlers_Should_EndWith_CommandHandler()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or().ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .HaveNameMatching(@".*CommandHandlerV\d+$")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All command handler types should end with 'CommandHandlerV{{version}}'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void QueryHandlers_Should_EndWith_QueryHandler()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .HaveNameMatching(@".*QueryHandlerV\d+$")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All query handler types should end with 'QueryHandlerV{{version}}'. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Commands_Should_BeIn_Commands_Namespace()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Or().ImplementInterface(typeof(ICommand<>))
            .And().AreNotInterfaces() // Exclude interface definitions
            .And().DoNotResideInNamespaceMatching(@".*\.Abstractions\..*") // Exclude abstractions
            .Should()
            .ResideInNamespaceMatching(@".*\.Commands\..*")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All concrete commands should be in Commands namespace. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Queries_Should_BeIn_Queries_Namespace()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .And().AreNotInterfaces() // Exclude interface definitions like ICachedQuery
            .And().DoNotResideInNamespaceMatching(@".*\.Abstractions\..*") // Exclude abstractions
            .Should()
            .ResideInNamespaceMatching(@".*\.Queries\..*")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All concrete queries should be in Queries namespace. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void CommandHandlers_Should_BeIn_Commands_Namespace()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or().ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .ResideInNamespaceMatching(@".*\.Commands\..*")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All command handlers should be in Commands namespace. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void QueryHandlers_Should_BeIn_Queries_Namespace()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .ResideInNamespaceMatching(@".*\.Queries\..*")
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All query handlers should be in Queries namespace. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Commands_Should_Be_Public()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Or().ImplementInterface(typeof(ICommand<>))
            .Should()
            .BePublic()
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All commands should be public. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Queries_Should_Be_Public()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .Should()
            .BePublic()
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All queries should be public. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void CommandHandlers_Should_Be_Internal()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or().ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All command handlers should be internal (not public). Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void QueryHandlers_Should_Be_Internal()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult();

        var violations = result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"All query handlers should be internal (not public). Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Commands_Should_Have_Corresponding_CommandHandler()
    {
        var commands = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Or().ImplementInterface(typeof(ICommand<>))
            .And().AreNotInterfaces() // Exclude interface definitions
            .And().DoNotResideInNamespaceMatching(@".*\.Abstractions\..*") // Exclude abstractions
            .GetTypes();

        var commandHandlers = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or().ImplementInterface(typeof(ICommandHandler<,>))
            .GetTypes();

        var missingHandlers = new List<string>();

        foreach (var command in commands)
        {
            var expectedHandlerName = command.Name.Replace("Command", "CommandHandler");
            var handlerExists = commandHandlers.Any(h => h.Name == expectedHandlerName);

            if (!handlerExists)
            {
                missingHandlers.Add($"{command.Name} -> {expectedHandlerName}");
            }
        }

        missingHandlers.Should().BeEmpty(
            "Every command should have a corresponding command handler. Missing: {0}",
            string.Join(", ", missingHandlers));
    }

    [Fact]
    public void Queries_Should_Have_Corresponding_QueryHandler()
    {
        var queries = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .And().DoNotHaveNameMatching(@"^ICachedQuery`?\d*$") // Exclude ICachedQuery interface
            .And().AreNotInterfaces() // Exclude interface definitions
            .And().DoNotResideInNamespaceMatching(@".*\.Abstractions\..*") // Exclude abstractions
            .GetTypes();

        var queryHandlers = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .GetTypes();

        var missingHandlers = new List<string>();

        foreach (var query in queries)
        {
            var expectedHandlerName = query.Name.Replace("Query", "QueryHandler");
            var handlerExists = queryHandlers.Any(h => h.Name == expectedHandlerName);

            if (!handlerExists)
            {
                missingHandlers.Add($"{query.Name} -> {expectedHandlerName}");
            }
        }

        missingHandlers.Should().BeEmpty(
            "Every query should have a corresponding query handler. Missing: {0}",
            string.Join(", ", missingHandlers));
    }
}
