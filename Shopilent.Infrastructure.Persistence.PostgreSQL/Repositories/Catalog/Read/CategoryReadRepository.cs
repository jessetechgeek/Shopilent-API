using System.Text;
using Dapper;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Catalog.Repositories.Read;
using Shopilent.Domain.Common.Models;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Read;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Catalog.Read;

public class CategoryReadRepository : AggregateReadRepositoryBase<Category, CategoryDto>, ICategoryReadRepository
{
    public CategoryReadRepository(IDapperConnectionFactory connectionFactory, ILogger<CategoryReadRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    public override async Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                id AS Id,
                name AS Name,
                description AS Description,
                parent_id AS ParentId,
                slug AS Slug,
                level AS Level,
                path AS Path,
                is_active AS IsActive,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM categories
            WHERE id = @Id";

        return await Connection.QueryFirstOrDefaultAsync<CategoryDto>(sql, new { Id = id });
    }

    public override async Task<IReadOnlyList<CategoryDto>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                id AS Id,
                name AS Name,
                description AS Description,
                parent_id AS ParentId,
                slug AS Slug,
                level AS Level,
                path AS Path,
                is_active AS IsActive,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM categories
            ORDER BY name";

        var categoryDtos = await Connection.QueryAsync<CategoryDto>(sql);
        return categoryDtos.ToList();
    }

    public async Task<CategoryDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                id AS Id,
                name AS Name,
                description AS Description,
                parent_id AS ParentId,
                slug AS Slug,
                level AS Level,
                path AS Path,
                is_active AS IsActive,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM categories
            WHERE slug = @Slug";

        return await Connection.QueryFirstOrDefaultAsync<CategoryDto>(sql, new { Slug = slug });
    }

    public async Task<IReadOnlyList<CategoryDto>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                id AS Id,
                name AS Name,
                description AS Description,
                parent_id AS ParentId,
                slug AS Slug,
                level AS Level,
                path AS Path,
                is_active AS IsActive,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM categories
            WHERE parent_id IS NULL
            ORDER BY name";

        var categoryDtos = await Connection.QueryAsync<CategoryDto>(sql);
        return categoryDtos.ToList();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetChildCategoriesAsync(Guid parentId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                id AS Id,
                name AS Name,
                description AS Description,
                parent_id AS ParentId,
                slug AS Slug,
                level AS Level,
                path AS Path,
                is_active AS IsActive,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM categories
            WHERE parent_id = @ParentId
            ORDER BY name";

        var categoryDtos = await Connection.QueryAsync<CategoryDto>(sql, new { ParentId = parentId });
        return categoryDtos.ToList();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoryPathAsync(Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            WITH RECURSIVE category_path AS (
                SELECT
                    id,
                    name,
                    description,
                    parent_id,
                    slug,
                    level,
                    path,
                    is_active,
                    created_at,
                    updated_at
                FROM categories
                WHERE id = @CategoryId

                UNION ALL

                SELECT
                    c.id,
                    c.name,
                    c.description,
                    c.parent_id,
                    c.slug,
                    c.level,
                    c.path,
                    c.is_active,
                    c.created_at,
                    c.updated_at
                FROM categories c
                JOIN category_path cp ON c.id = cp.parent_id
            )
            SELECT
                id AS Id,
                name AS Name,
                description AS Description,
                parent_id AS ParentId,
                slug AS Slug,
                level AS Level,
                path AS Path,
                is_active AS IsActive,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM category_path
            ORDER BY level DESC";

        var categoryDtos = await Connection.QueryAsync<CategoryDto>(sql, new { CategoryId = categoryId });
        return categoryDtos.ToList();
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        string sql;
        object parameters;

        if (excludeId.HasValue)
        {
            sql = @"
                SELECT COUNT(1) FROM categories
                WHERE slug = @Slug AND id != @ExcludeId";
            parameters = new { Slug = slug, ExcludeId = excludeId.Value };
        }
        else
        {
            sql = @"
                SELECT COUNT(1) FROM categories
                WHERE slug = @Slug";
            parameters = new { Slug = slug };
        }

        int count = await Connection.ExecuteScalarAsync<int>(sql, parameters);
        return count > 0;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetByIdsAsync(IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids == null || !ids.Any())
            return new List<CategoryDto>();

        // Convert the IDs to an array for SQL parameters
        var idArray = ids.ToArray();

        const string sql = @"
        SELECT
            id AS Id,
            name AS Name,
            description AS Description,
            parent_id AS ParentId,
            slug AS Slug,
            level AS Level,
            path AS Path,
            is_active AS IsActive,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM categories
        WHERE id = ANY(@Ids)
        ORDER BY array_position(@Ids, id)";

        var parameters = new { Ids = idArray };
        var categoryDtos = await Connection.QueryAsync<CategoryDto>(sql, parameters);
        return categoryDtos.ToList();
    }

    // Add method to get categories with details (product count and parent name)
    public async Task<DataTableResult<CategoryDetailDto>> GetCategoryDetailDataTableAsync(
        DataTableRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return new DataTableResult<CategoryDetailDto>(0, "Invalid request");

            // Base query with join to get parent name and product count
            var selectSql = new StringBuilder(@"
                SELECT
                    c.id AS Id,
                    c.name AS Name,
                    c.description AS Description,
                    c.parent_id AS ParentId,
                    c.slug AS Slug,
                    c.level AS Level,
                    c.path AS Path,
                    c.is_active AS IsActive,
                    c.created_at AS CreatedAt,
                    c.updated_at AS UpdatedAt,
                    parent.name AS ParentName,
                    (SELECT COUNT(*) FROM product_categories pc WHERE pc.category_id = c.id) AS ProductCount
                FROM categories c
                LEFT JOIN categories parent ON c.parent_id = parent.id");

            // Base count query
            const string countSql =
                "SELECT COUNT(*) FROM categories c LEFT JOIN categories parent ON c.parent_id = parent.id";

            // Where clause for filtering
            var whereClause = new StringBuilder();
            var parameters = new DynamicParameters();

            // Apply global search if provided
            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                whereClause.Append(" WHERE (");
                whereClause.Append("c.name ILIKE @SearchValue OR ");
                whereClause.Append("c.description ILIKE @SearchValue OR ");
                whereClause.Append("c.slug ILIKE @SearchValue OR ");
                whereClause.Append("parent.name ILIKE @SearchValue");
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
                                "name" => "c.name",
                                "parentname" => "parent.name",
                                "productcount" => "ProductCount",
                                "level" => "c.level",
                                "isactive" => "c.is_active",
                                "createdat" => "c.created_at",
                                _ => "c.name" // Default
                            };

                            orderByClause.Append($"{dbColumn} {(order.IsDescending ? "DESC" : "ASC")}");
                        }
                        else
                        {
                            orderByClause.Append("c.name ASC");
                        }
                    }
                    else
                    {
                        orderByClause.Append("c.name ASC");
                    }
                }
            }
            else
            {
                orderByClause.Append("c.name ASC");
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
                ? await Connection.ExecuteScalarAsync<int>(countSql + whereClause.ToString(), parameters)
                : totalCount;

            var data = await Connection.QueryAsync<CategoryDetailDto>(finalSelectSql, parameters);

            return new DataTableResult<CategoryDetailDto>(request.Draw, totalCount, filteredCount, data.ToList());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing CategoryDetailDto DataTable query");
            return new DataTableResult<CategoryDetailDto>(request.Draw, $"Error: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<CategoryDto>> GetByProductIdAsync(Guid productId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                c.id AS Id,
                c.name AS Name,
                c.description AS Description,
                c.parent_id AS ParentId,
                c.slug AS Slug,
                c.level AS Level,
                c.path AS Path,
                c.is_active AS IsActive,
                c.created_at AS CreatedAt,
                c.updated_at AS UpdatedAt
            FROM categories c
            JOIN product_categories pc ON c.id = pc.category_id
            WHERE pc.product_id = @ProductId";

        var categoryDtos = await Connection.QueryAsync<CategoryDto>(sql, new { ProductId = productId });
        return categoryDtos.ToList();
    }
}
