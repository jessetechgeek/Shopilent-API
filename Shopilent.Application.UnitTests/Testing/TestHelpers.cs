// TestHelpers.cs in the Testing folder
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.Application.Abstractions.Behaviors;

namespace Shopilent.Application.UnitTests.Testing;

public static class TestHelpers
{
    public static IServiceCollection AddMediatRWithValidation(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            // Use a concrete type from the Application assembly instead of the static class
            cfg.RegisterServicesFromAssembly(typeof(Shopilent.Application.Abstractions.Messaging.ICommand<>).Assembly);
            
            // Add the validation pipeline behavior
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        // Register all validators from the assembly
        services.AddValidatorsFromAssembly(
            typeof(Shopilent.Application.Abstractions.Messaging.ICommand<>).Assembly, 
            includeInternalTypes: true);

        return services;
    }
}