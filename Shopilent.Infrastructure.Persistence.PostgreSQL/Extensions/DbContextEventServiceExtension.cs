using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.Application.Abstractions.Events;
using Shopilent.Application.Abstractions.Outbox;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Extensions;

public class DbContextEventServiceExtension : IDbContextOptionsExtension
{
    public IDomainEventService EventService { get; }

    public DbContextEventServiceExtension(IDomainEventService eventService)
    {
        EventService = eventService;
    }

    public void ApplyServices(IServiceCollection services) { }

    public void Validate(IDbContextOptions options) { }

    public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

    private class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ExtensionInfo(DbContextEventServiceExtension extension) 
            : base(extension) { }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "EventServiceExtension";

        public override int GetServiceProviderHashCode() => 0;
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return other is ExtensionInfo;
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) { }
    }
}

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseEventService(this DbContextOptionsBuilder optionsBuilder, IDomainEventService eventService)
    {
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(new DbContextEventServiceExtension(eventService));
        
        return optionsBuilder;
    }
}