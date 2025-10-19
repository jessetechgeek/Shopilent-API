using System.Data;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;

public interface IDapperConnectionFactory
{
    IDbConnection GetReadConnection();
}