using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Catalog.Repositories.Read;
using Shopilent.Domain.Common.Models;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Dtos.Catalog;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Read;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Catalog.Read;

public class ProductReadRepository : AggregateReadRepositoryBase<Product, ProductDto>, IProductReadRepository
{
    public ProductReadRepository(IDapperConnectionFactory connectionFactory, ILogger<ProductReadRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    public override async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                p.id AS Id,
                p.name AS Name,
                p.description AS Description,
                p.base_price AS BasePrice,
                p.currency AS Currency,
                p.sku AS Sku,
                p.slug AS Slug,
                metadata::text AS MetadataJson,
                p.is_active AS IsActive,
                p.created_at AS CreatedAt,
                p.updated_at AS UpdatedAt
            FROM products p
            WHERE p.id = @Id";

        var result = await Connection.QueryFirstOrDefaultAsync<ProductDtoWithJson>(sql, new { Id = id });

        if (result == null)
            return null;

        var productDto = new ProductDto()
        {
            Id = result.Id,
            Name = result.Name,
            Description = result.Description,
            Currency = result.Currency,
            BasePrice = result.BasePrice,
            Sku = result.Sku,
            Slug = result.Slug,
            IsActive = result.IsActive,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };

        // Parse the configuration JSON
        if (!string.IsNullOrEmpty(result.MetadataJson))
        {
            try
            {
                productDto.Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    result.MetadataJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new Dictionary<string, object>();
            }
            catch
            {
                productDto.Metadata = new Dictionary<string, object>();
            }
        }
        else
        {
            productDto.Metadata = new Dictionary<string, object>();
        }

        return productDto;
    }

    public override async Task<IReadOnlyList<ProductDto>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                name AS Name,
                description AS Description,
                base_price AS BasePrice,
                currency AS Currency,
                sku AS Sku,
                slug AS Slug,
                is_active AS IsActive,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM products
            ORDER BY name";

        var productDtos = await Connection.QueryAsync<ProductDto>(sql);
        return productDtos.ToList();
    }

    public async Task<ProductDetailDto> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Get basic product info
        const string productSql = @"
        SELECT 
            p.id AS Id,
            p.name AS Name,
            p.description AS Description,
            p.base_price AS BasePrice,
            p.currency AS Currency,
            p.sku AS Sku,
            p.slug AS Slug,
            p.metadata::text AS MetadataJson,
            p.is_active AS IsActive,
            p.created_by AS CreatedBy,
            p.modified_by AS ModifiedBy,
            p.last_modified AS LastModified,
            p.created_at AS CreatedAt,
            p.updated_at AS UpdatedAt
        FROM products p
        WHERE p.id = @Id";

        var productDetailWithJson =
            await Connection.QueryFirstOrDefaultAsync<ProductDetailDtoWithJson>(productSql, new { Id = id });

        if (productDetailWithJson == null)
            return null;

        var productDetail = new ProductDetailDto
        {
            Id = productDetailWithJson.Id,
            Name = productDetailWithJson.Name,
            Description = productDetailWithJson.Description,
            BasePrice = productDetailWithJson.BasePrice,
            Currency = productDetailWithJson.Currency,
            Sku = productDetailWithJson.Sku,
            Slug = productDetailWithJson.Slug,
            IsActive = productDetailWithJson.IsActive,
            CreatedBy = productDetailWithJson.CreatedBy,
            ModifiedBy = productDetailWithJson.ModifiedBy,
            LastModified = productDetailWithJson.LastModified,
            CreatedAt = productDetailWithJson.CreatedAt,
            UpdatedAt = productDetailWithJson.UpdatedAt,
            Categories = new List<CategoryDto>(),
            Attributes = new List<ProductAttributeDto>(),
            Variants = new List<ProductVariantDto>(),
            Images = new List<ProductImageDto>() // Initialize the Images collection
        };

        // Parse the metadata JSON
        if (!string.IsNullOrEmpty(productDetailWithJson.MetadataJson))
        {
            try
            {
                productDetail.Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    productDetailWithJson.MetadataJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new Dictionary<string, object>();
            }
            catch
            {
                productDetail.Metadata = new Dictionary<string, object>();
            }
        }
        else
        {
            productDetail.Metadata = new Dictionary<string, object>();
        }

        // Get categories
        const string categoriesSql = @"
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

        productDetail.Categories = (await Connection.QueryAsync<CategoryDto>(
            categoriesSql, new { ProductId = id })).ToList();

        // Get attributes (using JSON conversion)
        const string attributesSql = @"
        SELECT 
            pa.id AS Id,
            pa.product_id AS ProductId,
            pa.attribute_id AS AttributeId,
            a.name AS AttributeName,
            a.display_name AS AttributeDisplayName,
            pa.values::text AS ValuesJson,
            pa.created_at AS CreatedAt,
            pa.updated_at AS UpdatedAt
        FROM product_attributes pa
        JOIN attributes a ON pa.attribute_id = a.id
        WHERE pa.product_id = @ProductId";

        var attributes = (await Connection.QueryAsync<ProductAttributeDtoWithJson>(
            attributesSql, new { ProductId = id })).ToList();

        // Convert JSON strings to dictionaries
        productDetail.Attributes = attributes.Select(attr => new ProductAttributeDto
        {
            Id = attr.Id,
            ProductId = attr.ProductId,
            AttributeId = attr.AttributeId,
            AttributeName = attr.AttributeName,
            AttributeDisplayName = attr.AttributeDisplayName,
            Values = JsonSerializer.Deserialize<Dictionary<string, object>>(
                attr.ValuesJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Dictionary<string, object>(),
            CreatedAt = attr.CreatedAt,
            UpdatedAt = attr.UpdatedAt
        }).ToList();

        // Get variants
        const string variantsSql = @"
        SELECT 
            pv.id AS Id,
            pv.product_id AS ProductId,
            pv.sku AS Sku,
            pv.price AS Price,
            pv.currency AS Currency,
            pv.stock_quantity AS StockQuantity,
            pv.is_active AS IsActive,
            pv.metadata::text AS MetadataJson,
            pv.created_at AS CreatedAt,
            pv.updated_at AS UpdatedAt
        FROM product_variants pv
        WHERE pv.product_id = @ProductId";

        var variants = await Connection.QueryAsync<ProductVariantDtoWithJson>(
            variantsSql, new { ProductId = id });

        var variantList = variants.Select(variant => new ProductVariantDto
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            Sku = variant.Sku,
            Price = variant.Price,
            Currency = variant.Currency,
            StockQuantity = variant.StockQuantity,
            IsActive = variant.IsActive,
            Metadata = !string.IsNullOrEmpty(variant.MetadataJson)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(variant.MetadataJson,
                      new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
                  new Dictionary<string, object>()
                : new Dictionary<string, object>(),
            CreatedAt = variant.CreatedAt,
            UpdatedAt = variant.UpdatedAt,
            Attributes = new List<VariantAttributeDto>(),
            Images = new List<ProductImageDto>() // Initialize variant images collection
        }).ToList();

        // Get variant attributes and images for each variant
        foreach (var variant in variantList)
        {
            // Get variant attributes
            const string variantAttributesSql = @"
            SELECT 
                va.variant_id AS VariantId,
                va.attribute_id AS AttributeId,
                a.name AS AttributeName,
                a.display_name AS AttributeDisplayName,
                va.value::text AS ValueJson
            FROM variant_attributes va
            JOIN attributes a ON va.attribute_id = a.id
            WHERE va.variant_id = @VariantId";

            var variantAttributes = await Connection.QueryAsync<VariantAttributeDtoWithJson>(
                variantAttributesSql, new { VariantId = variant.Id });

            variant.Attributes = variantAttributes.Select(va => new VariantAttributeDto
            {
                VariantId = va.VariantId,
                AttributeId = va.AttributeId,
                AttributeName = va.AttributeName,
                AttributeDisplayName = va.AttributeDisplayName,
                Value = JsonSerializer.Deserialize<Dictionary<string, object>>(
                            va.ValueJson,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
                        new Dictionary<string, object>()
            }).ToList();

            // Get variant images
            const string variantImagesSql = @"
            SELECT 
                image_key AS ImageKey,
                thumbnail_key AS ThumbnailKey,
                alt_text AS AltText,
                is_default AS IsDefault,
                display_order AS DisplayOrder
            FROM product_variant_images
            WHERE variant_id = @VariantId
            ORDER BY display_order, is_default DESC";

            var variantImagesData = await Connection.QueryAsync<ProductImageDto>(
                variantImagesSql, new { VariantId = variant.Id });

            variant.Images = variantImagesData.ToList();

            Console.WriteLine("variant.Images.Count: " + variant.Images.Count);
        }

        productDetail.Variants = variantList;

        // Get product images
        const string imagesSql = @"
        SELECT 
            image_key AS ImageKey,
            thumbnail_key AS ThumbnailKey,
            alt_text AS AltText,
            is_default AS IsDefault,
            display_order AS DisplayOrder
        FROM product_images
        WHERE product_id = @ProductId
        ORDER BY display_order, is_default DESC";

        var imagesData = await Connection.QueryAsync<ProductImageDto>(imagesSql, new { ProductId = id });

        productDetail.Images = imagesData.ToList();

        return productDetail;
    }

    public async Task<ProductDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                name AS Name,
                description AS Description,
                base_price AS BasePrice,
                currency AS Currency,
                sku AS Sku,
                slug AS Slug,
                is_active AS IsActive,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM products
            WHERE slug = @Slug";

        return await Connection.QueryFirstOrDefaultAsync<ProductDto>(sql, new { Slug = slug });
    }

    public async Task<IReadOnlyList<ProductDto>> GetByCategoryAsync(Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                p.id AS Id,
                p.name AS Name,
                p.description AS Description,
                p.base_price AS BasePrice,
                p.currency AS Currency,
                p.sku AS Sku,
                p.slug AS Slug,
                p.is_active AS IsActive,
                p.created_at AS CreatedAt,
                p.updated_at AS UpdatedAt
            FROM products p
            JOIN product_categories pc ON p.id = pc.product_id
            WHERE pc.category_id = @CategoryId
            ORDER BY p.name";

        var productDtos = await Connection.QueryAsync<ProductDto>(sql, new { CategoryId = categoryId });
        return productDtos.ToList();
    }

    public async Task<IReadOnlyList<ProductDto>> SearchAsync(string searchTerm, Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var sql = new StringBuilder(@"
            SELECT 
                p.id AS Id,
                p.name AS Name,
                p.description AS Description,
                p.base_price AS BasePrice,
                p.currency AS Currency,
                p.sku AS Sku,
                p.slug AS Slug,
                p.is_active AS IsActive,
                p.created_at AS CreatedAt,
                p.updated_at AS UpdatedAt
            FROM products p");

        var parameters = new DynamicParameters();
        parameters.Add("SearchTerm", $"%{searchTerm}%");

        if (categoryId.HasValue)
        {
            sql.Append(@"
                JOIN product_categories pc ON p.id = pc.product_id
                WHERE (p.name ILIKE @SearchTerm OR p.description ILIKE @SearchTerm OR p.sku ILIKE @SearchTerm)
                AND pc.category_id = @CategoryId");

            parameters.Add("CategoryId", categoryId.Value);
        }
        else
        {
            sql.Append(@"
                WHERE p.name ILIKE @SearchTerm OR p.description ILIKE @SearchTerm OR p.sku ILIKE @SearchTerm");
        }

        sql.Append(" ORDER BY p.name");

        var productDtos = await Connection.QueryAsync<ProductDto>(sql.ToString(), parameters);
        return productDtos.ToList();
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        string sql;
        object parameters;

        if (excludeId.HasValue)
        {
            sql = @"
                SELECT COUNT(1) FROM products
                WHERE slug = @Slug AND id != @ExcludeId";
            parameters = new { Slug = slug, ExcludeId = excludeId.Value };
        }
        else
        {
            sql = @"
                SELECT COUNT(1) FROM products
                WHERE slug = @Slug";
            parameters = new { Slug = slug };
        }

        int count = await Connection.ExecuteScalarAsync<int>(sql, parameters);
        return count > 0;
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;

        string sql;
        object parameters;

        if (excludeId.HasValue)
        {
            sql = @"
                SELECT COUNT(1) FROM products
                WHERE sku = @Sku AND id != @ExcludeId";
            parameters = new { Sku = sku, ExcludeId = excludeId.Value };
        }
        else
        {
            sql = @"
                SELECT COUNT(1) FROM products
                WHERE sku = @Sku";
            parameters = new { Sku = sku };
        }

        int count = await Connection.ExecuteScalarAsync<int>(sql, parameters);
        return count > 0;
    }

    public override async Task<PaginatedResult<ProductDto>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        string sortColumn = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        // Validate the sort column and provide a default
        string orderByClause;
        if (string.IsNullOrEmpty(sortColumn))
        {
            orderByClause = "p.name";
        }
        else
        {
            // Map the sortColumn (which might be a DTO property) to the actual DB column
            sortColumn = sortColumn.ToLower();
            orderByClause = sortColumn switch
            {
                "name" => "p.name",
                "price" or "baseprice" => "p.base_price",
                "sku" => "p.sku",
                "isactive" or "active" => "p.is_active",
                "createdat" or "created" => "p.created_at",
                _ => "p.name" // Default
            };
        }

        orderByClause += sortDescending ? " DESC" : " ASC";

        // Count query
        const string countSql = @"
        SELECT COUNT(*) 
        FROM products p
        WHERE p.is_active = true";

        // Main query with images
        var sql = $@"
        SELECT 
            p.id AS Id,
            p.name AS Name,
            p.description AS Description,
            p.base_price AS BasePrice,
            p.currency AS Currency,
            p.sku AS Sku,
            p.slug AS Slug,
            p.metadata::text AS MetadataJson,
            p.is_active AS IsActive,
            p.created_at AS CreatedAt,
            p.updated_at AS UpdatedAt,
            -- Images as JSON array
            COALESCE(
                (SELECT jsonb_agg(
                    jsonb_build_object(
                        'imageKey', pi.image_key,
                        'thumbnailKey', pi.thumbnail_key,
                        'altText', pi.alt_text,
                        'isDefault', pi.is_default,
                        'displayOrder', pi.display_order
                    ) ORDER BY pi.display_order, pi.is_default DESC
                )
                FROM product_images pi
                WHERE pi.product_id = p.id), '[]'::jsonb)::text AS ImagesJson
        FROM products p
        WHERE p.is_active = true
        ORDER BY {orderByClause}
        LIMIT @PageSize OFFSET @Offset";

        var parameters = new
        {
            PageSize = pageSize,
            Offset = (pageNumber - 1) * pageSize
        };

        // Execute the queries
        var totalCount = await Connection.ExecuteScalarAsync<int>(countSql);

        // Execute query and process results
        var productDtos = new List<ProductDto>();
        using (var reader = await Connection.ExecuteReaderAsync(sql, parameters))
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            while (reader.Read())
            {
                var productDto = new ProductDto
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Description")),
                    BasePrice = reader.GetDecimal(reader.GetOrdinal("BasePrice")),
                    Currency = reader.GetString(reader.GetOrdinal("Currency")),
                    Sku = reader.IsDBNull(reader.GetOrdinal("Sku"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Sku")),
                    Slug = reader.GetString(reader.GetOrdinal("Slug")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                    Metadata = new Dictionary<string, object>(),
                    Images = new List<ProductImageDto>()
                };

                // Parse metadata JSON
                if (!reader.IsDBNull(reader.GetOrdinal("MetadataJson")))
                {
                    string metadataJson = reader.GetString(reader.GetOrdinal("MetadataJson"));
                    if (!string.IsNullOrEmpty(metadataJson))
                    {
                        try
                        {
                            productDto.Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(
                                metadataJson, jsonOptions) ?? new Dictionary<string, object>();
                        }
                        catch
                        {
                            productDto.Metadata = new Dictionary<string, object>();
                        }
                    }
                }

                // Parse images JSON
                string imagesJson = reader.GetString(reader.GetOrdinal("ImagesJson"));
                if (!string.IsNullOrEmpty(imagesJson) && imagesJson != "[]")
                {
                    try
                    {
                        productDto.Images = JsonSerializer.Deserialize<List<ProductImageDto>>(
                            imagesJson, jsonOptions) ?? new List<ProductImageDto>();
                    }
                    catch
                    {
                        productDto.Images = new List<ProductImageDto>();
                    }
                }

                productDtos.Add(productDto);
            }
        }

        return new PaginatedResult<ProductDto>(productDtos, totalCount, pageNumber, pageSize);
    }

    // Override the DataTable method to provide custom implementation
    public override async Task<DataTableResult<ProductDto>> GetDataTableAsync(
        DataTableRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            return new DataTableResult<ProductDto>(0, "Invalid request");

        try
        {
            // Base query
            var selectSql = new StringBuilder(@"
                SELECT 
                    p.id AS Id,
                    p.name AS Name,
                    p.description AS Description,
                    p.base_price AS BasePrice,
                    p.currency AS Currency,
                    p.sku AS Sku,
                    p.slug AS Slug,
                    p.is_active AS IsActive,
                    p.created_at AS CreatedAt,
                    p.updated_at AS UpdatedAt
                FROM products p");

            // Count query
            const string countSql = "SELECT COUNT(*) FROM products p";

            // Where clause for filtering
            var whereClause = new StringBuilder();
            var parameters = new DynamicParameters();

            // Apply global search if provided
            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                whereClause.Append(" WHERE (");
                whereClause.Append("p.name ILIKE @SearchValue OR ");
                whereClause.Append("p.description ILIKE @SearchValue OR ");
                whereClause.Append("p.sku ILIKE @SearchValue OR ");
                whereClause.Append("p.slug ILIKE @SearchValue");
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
                                "name" => "p.name",
                                "price" or "baseprice" => "p.base_price",
                                "sku" => "p.sku",
                                "isactive" or "active" => "p.is_active",
                                "createdat" => "p.created_at",
                                _ => "p.name" // Default
                            };

                            orderByClause.Append($"{dbColumn} {(order.IsDescending ? "DESC" : "ASC")}");
                        }
                        else
                        {
                            orderByClause.Append("p.name ASC");
                        }
                    }
                    else
                    {
                        orderByClause.Append("p.name ASC");
                    }
                }
            }
            else
            {
                orderByClause.Append("p.name ASC");
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

            var data = await Connection.QueryAsync<ProductDto>(finalSelectSql, parameters);

            return new DataTableResult<ProductDto>(request.Draw, totalCount, filteredCount, data.AsList());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing ProductDto DataTable query");
            return new DataTableResult<ProductDto>(request.Draw, $"Error: {ex.Message}");
        }
    }

    public async Task<PaginatedResult<ProductDto>> GetPaginatedByCategoryAsync(
        Guid categoryId,
        int pageNumber,
        int pageSize,
        string sortColumn = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        // Validate the sort column and provide a default
        string orderByClause;
        if (string.IsNullOrEmpty(sortColumn))
        {
            orderByClause = "p.name";
        }
        else
        {
            // Map the sortColumn to the actual DB column
            sortColumn = sortColumn.ToLower();
            orderByClause = sortColumn switch
            {
                "name" => "p.name",
                "price" or "baseprice" => "p.base_price",
                "sku" => "p.sku",
                "isactive" or "active" => "p.is_active",
                "createdat" or "created" => "p.created_at",
                _ => "p.name" // Default
            };
        }

        orderByClause += sortDescending ? " DESC" : " ASC";

        // Count query
        const string countSql = @"
        SELECT COUNT(*) 
        FROM products p
        JOIN product_categories pc ON p.id = pc.product_id
        WHERE pc.category_id = @CategoryId AND p.is_active = true";

        // Data query with pagination
        var sql = $@"
        SELECT 
            p.id AS Id,
            p.name AS Name,
            p.description AS Description,
            p.base_price AS BasePrice,
            p.currency AS Currency,
            p.sku AS Sku,
            p.slug AS Slug,
            p.is_active AS IsActive,
            p.created_at AS CreatedAt,
            p.updated_at AS UpdatedAt
        FROM products p
        JOIN product_categories pc ON p.id = pc.product_id
        WHERE pc.category_id = @CategoryId AND p.is_active = true
        ORDER BY {orderByClause}
        LIMIT @PageSize OFFSET @Offset";

        var parameters = new
        {
            CategoryId = categoryId,
            PageSize = pageSize,
            Offset = (pageNumber - 1) * pageSize
        };

        // Execute the queries
        var totalCount = await Connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await Connection.QueryAsync<ProductDto>(sql, parameters);

        return new PaginatedResult<ProductDto>(items.AsList(), totalCount, pageNumber, pageSize);
    }

    public async Task<IReadOnlyList<ProductDto>> GetByIdsAsync(IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids == null || !ids.Any())
            return new List<ProductDto>();

        // Convert the IDs to an array for SQL parameters
        var idArray = ids.ToArray();

        const string sql = @"
        SELECT 
            p.id AS Id,
            p.name AS Name,
            p.description AS Description,
            p.base_price AS BasePrice,
            p.currency AS Currency,
            p.sku AS Sku,
            p.slug AS Slug,
            p.is_active AS IsActive,
            p.created_at AS CreatedAt,
            p.updated_at AS UpdatedAt
        FROM products p
        WHERE p.id = ANY(@Ids)
        ORDER BY array_position(@Ids, p.id)";

        var parameters = new { Ids = idArray };
        var productDtos = await Connection.QueryAsync<ProductDto>(sql, parameters);
        return productDtos.ToList();
    }


    public async Task<DataTableResult<ProductDetailDto>> GetProductDetailDataTableAsync(
        DataTableRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return new DataTableResult<ProductDetailDto>(0, "Invalid request");

            // Get the total count
            const string countSql = "SELECT COUNT(*) FROM products";
            var totalCount = await Connection.ExecuteScalarAsync<int>(countSql);

            // Build the main product query
            var whereClause = new StringBuilder();
            var parameters = new DynamicParameters();

            // Apply global search if provided
            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                whereClause.Append(" WHERE (");
                whereClause.Append("p.name ILIKE @SearchValue OR ");
                whereClause.Append("p.description ILIKE @SearchValue OR ");
                whereClause.Append("p.sku ILIKE @SearchValue OR ");
                whereClause.Append("p.slug ILIKE @SearchValue");
                whereClause.Append(")");
                parameters.Add("SearchValue", $"%{request.Search.Value}%");
            }

            // Get the filtered count
            var filteredCountSql = "SELECT COUNT(*) FROM products p" + whereClause.ToString();
            var filteredCount = whereClause.Length > 0
                ? await Connection.ExecuteScalarAsync<int>(filteredCountSql, parameters)
                : totalCount;

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
                                "name" => "p.name",
                                "price" or "baseprice" => "p.base_price",
                                "sku" => "p.sku",
                                "isactive" or "active" => "p.is_active",
                                "createdat" => "p.created_at",
                                _ => "p.name" // Default
                            };

                            orderByClause.Append($"{dbColumn} {(order.IsDescending ? "DESC" : "ASC")}");
                        }
                        else
                        {
                            orderByClause.Append("p.name ASC");
                        }
                    }
                    else
                    {
                        orderByClause.Append("p.name ASC");
                    }
                }
            }
            else
            {
                orderByClause.Append("p.name ASC");
            }

            // Pagination
            var paginationClause = " LIMIT @Length OFFSET @Start";
            parameters.Add("Length", request.Length);
            parameters.Add("Start", request.Start);

            // Optimized single query that leverages PostgreSQL's JSON capabilities
            var productSql = @"
            WITH selected_products AS (
                SELECT 
                    p.id,
                    p.name,
                    p.description,
                    p.base_price,
                    p.sku,
                    p.slug,
                    p.is_active,
                    p.metadata,
                    p.created_by,
                    p.modified_by,
                    p.last_modified,
                    p.created_at,
                    p.updated_at
                FROM products p"
                             + whereClause.ToString()
                             + orderByClause.ToString()
                             + paginationClause + @"
            )
            SELECT 
                p.id AS Id,
                p.name AS Name,
                p.description AS Description,
                p.base_price AS BasePrice,
                'USD' AS Currency,
                p.sku AS Sku,
                p.slug AS Slug,
                p.is_active AS IsActive,
                p.metadata::text AS MetadataJson,
                p.created_by AS CreatedBy,
                p.modified_by AS ModifiedBy,
                p.last_modified AS LastModified,
                p.created_at AS CreatedAt,
                p.updated_at AS UpdatedAt,
                -- Categories as JSON array
                COALESCE(
                    (SELECT jsonb_agg(
                        jsonb_build_object(
                            'id', c.id,
                            'name', c.name,
                            'description', c.description,
                            'parentId', c.parent_id,
                            'slug', c.slug,
                            'level', c.level,
                            'path', c.path,
                            'isActive', c.is_active,
                            'createdAt', c.created_at,
                            'updatedAt', c.updated_at
                        )
                    )
                    FROM categories c
                    JOIN product_categories pc ON c.id = pc.category_id
                    WHERE pc.product_id = p.id
                    GROUP BY pc.product_id), '[]'::jsonb)::text AS CategoriesJson,
                -- Attributes as JSON array
                COALESCE(
                    (SELECT jsonb_agg(
                        jsonb_build_object(
                            'id', pa.id,
                            'productId', pa.product_id,
                            'attributeId', pa.attribute_id,
                            'attributeName', a.name,
                            'attributeDisplayName', a.display_name,
                            'values', pa.values,
                            'createdAt', pa.created_at,
                            'updatedAt', pa.updated_at
                        )
                    )
                    FROM product_attributes pa
                    JOIN attributes a ON pa.attribute_id = a.id
                    WHERE pa.product_id = p.id
                    GROUP BY pa.product_id), '[]'::jsonb)::text AS AttributesJson,
                -- Variants as JSON array with nested attributes
                COALESCE(
                    (SELECT jsonb_agg(
                        jsonb_build_object(
                            'id', pv.id,
                            'productId', pv.product_id,
                            'sku', pv.sku,
                            'price', pv.price,
                            'currency', 'USD',
                            'stockQuantity', pv.stock_quantity,
                            'isActive', pv.is_active,
                            'metadata', pv.metadata,
                            'createdAt', pv.created_at,
                            'updatedAt', pv.updated_at,
                            'attributes', COALESCE(
                                (SELECT jsonb_agg(
                                    jsonb_build_object(
                                        'variantId', va.variant_id,
                                        'attributeId', va.attribute_id,
                                        'attributeName', a2.name,
                                        'attributeDisplayName', a2.display_name,
                                        'value', va.value
                                    )
                                )
                                FROM variant_attributes va
                                JOIN attributes a2 ON va.attribute_id = a2.id
                                WHERE va.variant_id = pv.id
                                GROUP BY va.variant_id), '[]'::jsonb)
                        )
                    )
                    FROM product_variants pv
                    WHERE pv.product_id = p.id
                    GROUP BY pv.product_id), '[]'::jsonb)::text AS VariantsJson
            FROM selected_products p";

            // Create a list to hold the results
            var productDtos = new List<ProductDetailDto>();

            // Execute query and process results
            using (var reader = await Connection.ExecuteReaderAsync(productSql, parameters))
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                while (reader.Read())
                {
                    // Create base product DTO
                    var product = new ProductDetailDto
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("Description")),
                        BasePrice = reader.GetDecimal(reader.GetOrdinal("BasePrice")),
                        Currency = reader.GetString(reader.GetOrdinal("Currency")),
                        Sku = reader.IsDBNull(reader.GetOrdinal("Sku"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("Sku")),
                        Slug = reader.GetString(reader.GetOrdinal("Slug")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy"))
                            ? null
                            : (Guid?)reader.GetGuid(reader.GetOrdinal("CreatedBy")),
                        ModifiedBy = reader.IsDBNull(reader.GetOrdinal("ModifiedBy"))
                            ? null
                            : (Guid?)reader.GetGuid(reader.GetOrdinal("ModifiedBy")),
                        LastModified = reader.IsDBNull(reader.GetOrdinal("LastModified"))
                            ? null
                            : (DateTime?)reader.GetDateTime(reader.GetOrdinal("LastModified")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                        Metadata = new Dictionary<string, object>(),
                        Categories = new List<CategoryDto>(),
                        Attributes = new List<ProductAttributeDto>(),
                        Variants = new List<ProductVariantDto>()
                    };

                    // Parse metadata
                    if (!reader.IsDBNull(reader.GetOrdinal("MetadataJson")))
                    {
                        string metadataJson = reader.GetString(reader.GetOrdinal("MetadataJson"));
                        if (!string.IsNullOrEmpty(metadataJson))
                        {
                            product.Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(
                                metadataJson, jsonOptions);
                        }
                    }

                    // Parse categories
                    string categoriesJson = reader.GetString(reader.GetOrdinal("CategoriesJson"));
                    if (!string.IsNullOrEmpty(categoriesJson) && categoriesJson != "[]")
                    {
                        product.Categories = JsonSerializer.Deserialize<List<CategoryDto>>(
                            categoriesJson, jsonOptions);
                    }

                    // Parse attributes
                    string attributesJson = reader.GetString(reader.GetOrdinal("AttributesJson"));
                    if (!string.IsNullOrEmpty(attributesJson) && attributesJson != "[]")
                    {
                        product.Attributes = JsonSerializer.Deserialize<List<ProductAttributeDto>>(
                            attributesJson, jsonOptions);
                    }

                    // Parse variants
                    string variantsJson = reader.GetString(reader.GetOrdinal("VariantsJson"));
                    if (!string.IsNullOrEmpty(variantsJson) && variantsJson != "[]")
                    {
                        product.Variants = JsonSerializer.Deserialize<List<ProductVariantDto>>(
                            variantsJson, jsonOptions);
                    }

                    productDtos.Add(product);
                }
            }

            return new DataTableResult<ProductDetailDto>(request.Draw, totalCount, filteredCount, productDtos);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing ProductDetailDto DataTable query: {ErrorMessage}", ex.Message);
            return new DataTableResult<ProductDetailDto>(request.Draw, $"Error: {ex.Message}");
        }
    }
}