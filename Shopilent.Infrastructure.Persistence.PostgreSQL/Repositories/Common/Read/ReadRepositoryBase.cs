using System.Data;
using System.Text;
using Dapper;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Common.Models;
using Shopilent.Domain.Common.Repositories.Read;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Extensions;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Read;

public abstract class ReadRepositoryBase<TEntity, TDto> : IReadRepository<TDto>
    where TEntity : class
    where TDto : class
{
    protected readonly IDbConnection Connection;
    protected readonly ILogger Logger;

    protected ReadRepositoryBase(IDapperConnectionFactory connectionFactory, ILogger logger)
    {
        Connection = connectionFactory?.GetReadConnection() ??
                     throw new ArgumentNullException(nameof(connectionFactory));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public abstract Task<TDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public abstract Task<IReadOnlyList<TDto>> ListAllAsync(CancellationToken cancellationToken = default);

    public virtual async Task<PaginatedResult<TDto>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        string sortColumn = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        // Get the table name using pluralized entity name
        string tableName = GetTableName();

        // Validate that the sort column exists if provided
        if (!string.IsNullOrEmpty(sortColumn))
        {
            sortColumn = sortColumn.ToSnakeCase();
        }
        else
        {
            // Default to "id" or another logical default column
            sortColumn = "id";
        }

        // Build the queries
        var sqlCount = $"SELECT COUNT(*) FROM {tableName}";

        var columnMappings = GetColumnMappings();
        var selectColumns = string.Join(", ", columnMappings.Select(m => $"{m.DbColumn} AS {m.DtoProperty}"));

        var sqlItems = $@"
            SELECT {selectColumns} FROM {tableName}
            ORDER BY {sortColumn} {(sortDescending ? "DESC" : "ASC")}
            LIMIT @PageSize OFFSET @Offset";

        var parameters = new
        {
            PageSize = pageSize,
            Offset = (pageNumber - 1) * pageSize
        };

        // Execute the queries
        var totalCount = await Connection.ExecuteScalarAsync<int>(sqlCount);
        var items = await Connection.QueryAsync<TDto>(sqlItems, parameters);

        return new PaginatedResult<TDto>(items.AsList(), totalCount, pageNumber, pageSize);
    }

    public virtual async Task<DataTableResult<TDto>> GetDataTableAsync(
        DataTableRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return new DataTableResult<TDto>(0, "Invalid request");

            // Get the table name using pluralized entity name
            string tableName = GetTableName();

            // Get column mappings
            var columnMappings = GetColumnMappings();
            var selectColumns = string.Join(", ", columnMappings.Select(m => $"{m.DbColumn} AS {m.DtoProperty}"));

            // Build the base SELECT query
            var selectSql = $"SELECT {selectColumns} FROM {tableName}";
            var countSql = $"SELECT COUNT(*) FROM {tableName}";

            // Build WHERE clause for global search
            var whereClause = new StringBuilder();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                whereClause.Append(" WHERE (");

                var searchableColumns = request.Columns
                    .Where(c => c.Searchable)
                    .Select(c => GetDbColumnName(c.Data))
                    .ToList();

                for (int i = 0; i < searchableColumns.Count; i++)
                {
                    if (i > 0) whereClause.Append(" OR ");
                    var columnName = searchableColumns[i];
                    whereClause.Append($"{columnName}::text ILIKE @SearchValue");
                }

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
                            var dbColumn = GetDbColumnName(column.Data);
                            orderByClause.Append($"{dbColumn} {(order.IsDescending ? "DESC" : "ASC")}");
                        }
                        else
                        {
                            orderByClause.Append("id ASC");
                        }
                    }
                    else
                    {
                        orderByClause.Append("id ASC");
                    }
                }
            }
            else
            {
                orderByClause.Append("id ASC");
            }

            // Add pagination
            var paginationClause = $" LIMIT @Length OFFSET @Start";
            parameters.Add("Length", request.Length);
            parameters.Add("Start", request.Start);

            // Build the final queries
            var finalCountSql = countSql + whereClause.ToString();
            var finalSelectSql = selectSql + whereClause.ToString() + orderByClause.ToString() + paginationClause;

            // Execute the queries
            var totalCount = await Connection.ExecuteScalarAsync<int>(countSql);
            var filteredCount = whereClause.Length > 0
                ? await Connection.ExecuteScalarAsync<int>(finalCountSql, parameters)
                : totalCount;

            var data = await Connection.QueryAsync<TDto>(finalSelectSql, parameters);

            return new DataTableResult<TDto>(request.Draw, totalCount, filteredCount, data.AsList());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing DataTable query");
            return new DataTableResult<TDto>(request.Draw, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the database table name for the entity.
    /// By default, converts the entity name to snake_case and pluralizes it.
    /// </summary>
    protected virtual string GetTableName()
    {
        // Convert entity name (e.g., "ProductVariant") to snake_case and pluralize
        // This assumes table names follow standard PostgreSQL naming conventions
        string entityName = typeof(TEntity).Name;
        if (entityName == "Category")
            return "categories";
        return entityName.ToSnakeCase() + "s";
    }

    /// <summary>
    /// Gets the database column name for a DTO property.
    /// By default, converts the property name to snake_case.
    /// </summary>
    protected virtual string GetDbColumnName(string propertyName)
    {
        return propertyName.ToSnakeCase();
    }

    /// <summary>
    /// Defines mappings between database columns and DTO properties.
    /// Override this method in derived repositories to provide specific mappings.
    /// </summary>
    protected virtual IEnumerable<(string DbColumn, string DtoProperty)> GetColumnMappings()
    {
        // Default implementation: map all properties one-to-one by name conversion
        var properties = typeof(TDto).GetProperties();
        foreach (var property in properties)
        {
            yield return (GetDbColumnName(property.Name), property.Name);
        }
    }
}