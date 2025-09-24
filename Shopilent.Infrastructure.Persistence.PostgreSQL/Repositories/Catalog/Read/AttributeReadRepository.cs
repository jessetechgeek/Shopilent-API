using System.Text;
using Dapper;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Catalog.Enums;
using Shopilent.Domain.Catalog.Repositories.Read;
using Shopilent.Domain.Common.Models;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Read;
using Attribute = Shopilent.Domain.Catalog.Attribute;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Catalog.Read;

public class AttributeReadRepository : AggregateReadRepositoryBase<Attribute, AttributeDto>, IAttributeReadRepository
{
    public AttributeReadRepository(IDapperConnectionFactory connectionFactory, ILogger<AttributeReadRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    public override async Task<AttributeDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
        SELECT 
            id AS Id,
            name AS Name,
            display_name AS DisplayName,
            type AS Type,
            configuration AS Configuration,
            filterable AS Filterable,
            searchable AS Searchable,
            is_variant AS IsVariant,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM attributes
        WHERE id = @Id";

        return await Connection.QueryFirstOrDefaultAsync<AttributeDto>(sql, new { Id = id });
    }

    public override async Task<IReadOnlyList<AttributeDto>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
        SELECT 
            id AS Id,
            name AS Name,
            display_name AS DisplayName,
            type AS Type,
            configuration AS Configuration,
            filterable AS Filterable,
            searchable AS Searchable,
            is_variant AS IsVariant,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM attributes
        ORDER BY name";

        var attributeDtos = await Connection.QueryAsync<AttributeDto>(sql);
        return attributeDtos.ToList();
    }

    public async Task<AttributeDto> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                name AS Name,
                display_name AS DisplayName,
                type AS Type,
                configuration AS Configuration,
                filterable AS Filterable,
                searchable AS Searchable,
                is_variant AS IsVariant,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM attributes
            WHERE name = @Name";

        return await Connection.QueryFirstOrDefaultAsync<AttributeDto>(sql, new { Name = name });
    }

    public async Task<IReadOnlyList<AttributeDto>> GetVariantAttributesAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                name AS Name,
                display_name AS DisplayName,
                type AS Type,
                configuration AS Configuration,
                filterable AS Filterable,
                searchable AS Searchable,
                is_variant AS IsVariant,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM attributes
            WHERE is_variant = true
            ORDER BY name";

        var attributeDtos = await Connection.QueryAsync<AttributeDto>(sql);
        return attributeDtos.ToList();
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        string sql;
        object parameters;

        if (excludeId.HasValue)
        {
            sql = @"
                SELECT COUNT(1) FROM attributes
                WHERE name = @Name AND id != @ExcludeId";
            parameters = new { Name = name, ExcludeId = excludeId.Value };
        }
        else
        {
            sql = @"
                SELECT COUNT(1) FROM attributes
                WHERE name = @Name";
            parameters = new { Name = name };
        }

        int count = await Connection.ExecuteScalarAsync<int>(sql, parameters);
        return count > 0;
    }

    public override async Task<DataTableResult<AttributeDto>> GetDataTableAsync(
        DataTableRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return new DataTableResult<AttributeDto>(0, "Invalid request");

            // Base query using JsonDictionaryTypeHandler
            var selectSql = new StringBuilder(@"
            SELECT 
                id AS Id,
                name AS Name,
                display_name AS DisplayName,
                type AS Type,
                configuration AS Configuration,
                filterable AS Filterable,
                searchable AS Searchable,
                is_variant AS IsVariant,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM attributes");

            // Base count query
            const string countSql = "SELECT COUNT(*) FROM attributes";

            // Where clause for filtering
            var whereClause = new StringBuilder();
            var parameters = new DynamicParameters();

            // Apply global search if provided
            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                whereClause.Append(" WHERE (");
                whereClause.Append("name ILIKE @SearchValue OR ");
                whereClause.Append("display_name ILIKE @SearchValue OR ");
                whereClause.Append("type ILIKE @SearchValue");
                whereClause.Append(")");
                parameters.Add("SearchValue", $"%{request.Search.Value}%");
            }

            // Build ORDER BY clause
            var orderByClause = new StringBuilder(" ORDER BY ");

            if (request.Order != null && request.Order.Any())
            {
                for (int i = 0; i < request.Order.Count; i++)
                {
                    if (i > 0) orderByClause.Append(", ");

                    var order = request.Order[i];
                    if (order.Column < request.Columns.Count)
                    {
                        var column = request.Columns[order.Column];
                        if (column.Orderable)
                        {
                            // Map column names to database columns
                            var dbColumn = column.Data.ToLower() switch
                            {
                                "name" => "name",
                                "displayname" => "display_name",
                                "type" => "type",
                                "filterable" => "filterable",
                                "searchable" => "searchable",
                                "isvariant" => "is_variant",
                                "createdat" => "created_at",
                                _ => "name" // Default
                            };

                            orderByClause.Append($"{dbColumn} {(order.IsDescending ? "DESC" : "ASC")}");
                        }
                        else
                        {
                            orderByClause.Append("name ASC");
                        }
                    }
                    else
                    {
                        orderByClause.Append("name ASC");
                    }
                }
            }
            else
            {
                orderByClause.Append("name ASC");
            }

            // Pagination
            var paginationClause = " LIMIT @Length OFFSET @Start";
            parameters.Add("Length", request.Length);
            parameters.Add("Start", request.Start);

            // Build final queries
            var finalCountSql = countSql + whereClause.ToString();
            var finalSelectSql = selectSql.ToString() + whereClause.ToString() + orderByClause.ToString() +
                                 paginationClause;

            // Execute queries
            var totalCount = await Connection.ExecuteScalarAsync<int>(countSql);
            var filteredCount = whereClause.Length > 0
                ? await Connection.ExecuteScalarAsync<int>(finalCountSql, parameters)
                : totalCount;

            // Direct mapping using JsonDictionaryTypeHandler
            var attributeDtos = await Connection.QueryAsync<AttributeDto>(finalSelectSql, parameters);

            return new DataTableResult<AttributeDto>(request.Draw, totalCount, filteredCount, attributeDtos.AsList());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing AttributeDto DataTable query");
            return new DataTableResult<AttributeDto>(request.Draw, $"Error: {ex.Message}");
        }
    }
}