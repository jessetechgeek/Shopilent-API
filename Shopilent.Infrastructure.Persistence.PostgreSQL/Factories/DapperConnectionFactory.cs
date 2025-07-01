using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Factories;

public class DapperConnectionFactory : IDapperConnectionFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DapperConnectionFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IDbConnection GetReadConnection()
    {
        return _serviceProvider.GetRequiredService<IDbConnection>();
    }
   
}