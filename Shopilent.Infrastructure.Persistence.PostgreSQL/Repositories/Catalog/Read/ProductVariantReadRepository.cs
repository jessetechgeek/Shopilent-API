using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Catalog.Repositories.Read;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Dtos.Catalog;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Read;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Catalog.Read;

public class ProductVariantReadRepository : EntityReadRepositoryBase<ProductVariant, ProductVariantDto>,
    IProductVariantReadRepository
{
    public ProductVariantReadRepository(IDapperConnectionFactory connectionFactory,
        ILogger<ProductVariantReadRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    public override async Task<ProductVariantDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                pv.id AS Id,
                pv.product_id AS ProductId,
                pv.sku AS Sku,
                pv.price AS Price,
                pv.currency AS Currency,
                pv.stock_quantity AS StockQuantity,
                pv.is_active AS IsActive,
                pv.metadata AS Metadata,
                pv.created_at AS CreatedAt,
                pv.updated_at AS UpdatedAt
            FROM product_variants pv
            WHERE pv.id = @Id";

        var variant = await Connection.QueryFirstOrDefaultAsync<ProductVariantDtoWithJson>(sql, new { Id = id });


        if (variant == null)
            return null;

        var variantDto = new ProductVariantDto()
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            Sku = variant.Sku,
            Price = variant.Price,
            Currency = variant.Currency,
            StockQuantity = variant.StockQuantity,
            IsActive = variant.IsActive,
            CreatedAt = variant.CreatedAt,
            UpdatedAt = variant.UpdatedAt
        };

        if (!string.IsNullOrEmpty(variant.MetadataJson))
        {
            try
            {
                variantDto.Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    variant.MetadataJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new Dictionary<string, object>();
            }
            catch
            {
                variantDto.Metadata = new Dictionary<string, object>();
            }
        }
        else
        {
            variantDto.Metadata = new Dictionary<string, object>();
        }

        return variantDto;
    }

    public override async Task<IReadOnlyList<ProductVariantDto>> ListAllAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                pv.id AS Id,
                pv.product_id AS ProductId,
                pv.sku AS Sku,
                pv.price AS Price,
                pv.currency AS Currency,
                pv.stock_quantity AS StockQuantity,
                pv.is_active AS IsActive,
                pv.metadata AS Metadata,
                pv.created_at AS CreatedAt,
                pv.updated_at AS UpdatedAt
            FROM product_variants pv";

        var variants = await Connection.QueryAsync<ProductVariantDto>(sql);
        var variantsList = variants.ToList();

        // Load variant attributes for each variant
        foreach (var variant in variantsList)
        {
            await LoadVariantAttributes(variant);
        }

        return variantsList;
    }

    public async Task<IReadOnlyList<ProductVariantDto>> GetByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
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

        var variants = await Connection.QueryAsync<ProductVariantDtoWithJson>(sql, new { ProductId = productId });
        var variantList = variants.ToList();

        var variantDtoList = new List<ProductVariantDto>();

        foreach (var variant in variantList)
        {
            var variantDto = new ProductVariantDto()
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                Sku = variant.Sku,
                Price = variant.Price,
                Currency = variant.Currency,
                StockQuantity = variant.StockQuantity,
                IsActive = variant.IsActive,
                CreatedAt = variant.CreatedAt,
                UpdatedAt = variant.UpdatedAt
            };

            if (!string.IsNullOrEmpty(variant.MetadataJson))
            {
                try
                {
                    variantDto.Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        variant.MetadataJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new Dictionary<string, object>();
                }
                catch
                {
                    variantDto.Metadata = new Dictionary<string, object>();
                }
            }
            else
            {
                variantDto.Metadata = new Dictionary<string, object>();
            }

            await LoadVariantAttributes(variantDto);

            variantDtoList.Add(variantDto);
        }

        return variantDtoList;
    }

    public async Task<ProductVariantDto> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return null;

        const string sql = @"
            SELECT 
                pv.id AS Id,
                pv.product_id AS ProductId,
                pv.sku AS Sku,
                pv.price AS Price,
                pv.currency AS Currency,
                pv.stock_quantity AS StockQuantity,
                pv.is_active AS IsActive,
                pv.metadata AS Metadata,
                pv.created_at AS CreatedAt,
                pv.updated_at AS UpdatedAt
            FROM product_variants pv
            WHERE pv.sku = @Sku";

        var variant = await Connection.QueryFirstOrDefaultAsync<ProductVariantDto>(sql, new { Sku = sku });

        if (variant != null)
        {
            await LoadVariantAttributes(variant);
        }

        return variant;
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
                SELECT COUNT(1) FROM product_variants
                WHERE sku = @Sku AND id != @ExcludeId";
            parameters = new { Sku = sku, ExcludeId = excludeId.Value };
        }
        else
        {
            sql = @"
                SELECT COUNT(1) FROM product_variants
                WHERE sku = @Sku";
            parameters = new { Sku = sku };
        }

        int count = await Connection.ExecuteScalarAsync<int>(sql, parameters);
        return count > 0;
    }

    public async Task<IReadOnlyList<ProductVariantDto>> GetInStockVariantsAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                pv.id AS Id,
                pv.product_id AS ProductId,
                pv.sku AS Sku,
                pv.price AS Price,
                pv.currency AS Currency,
                pv.stock_quantity AS StockQuantity,
                pv.is_active AS IsActive,
                pv.metadata AS Metadata,
                pv.created_at AS CreatedAt,
                pv.updated_at AS UpdatedAt
            FROM product_variants pv
            WHERE pv.product_id = @ProductId
            AND pv.is_active = true
            AND pv.stock_quantity > 0";

        var variants = await Connection.QueryAsync<ProductVariantDto>(sql, new { ProductId = productId });
        var variantList = variants.ToList();

        // Load variant attributes for each variant
        foreach (var variant in variantList)
        {
            await LoadVariantAttributes(variant);
        }

        return variantList;
    }

    private async Task LoadVariantAttributes(ProductVariantDto variant)
    {
        if (variant == null) return;

        const string attributesSql = @"
            SELECT 
                va.variant_id AS VariantId,
                va.attribute_id AS AttributeId,
                a.name AS AttributeName,
                a.display_name AS AttributeDisplayName,
                a.type AS AttributeType,
                va.value::text AS ValueJson
            FROM variant_attributes va
            JOIN attributes a ON va.attribute_id = a.id
            WHERE va.variant_id = @VariantId";

        try
        {
            var attributeDtoList = (await Connection.QueryAsync<VariantAttributeDtoWithJson>(
                attributesSql, new { VariantId = variant.Id })).ToList();

            var returnDtoList = new List<VariantAttributeDto>();

            foreach (var jsonAttribute in attributeDtoList)
            {
                var attributeDto = new VariantAttributeDto()
                {
                    VariantId = jsonAttribute.VariantId,
                    AttributeId = jsonAttribute.AttributeId,
                    AttributeName = jsonAttribute.AttributeName,
                    AttributeDisplayName = jsonAttribute.AttributeDisplayName,
                };

                if (!string.IsNullOrEmpty(jsonAttribute.ValueJson))
                {
                    try
                    {
                        attributeDto.Value = JsonSerializer.Deserialize<Dictionary<string, object>>(
                            jsonAttribute.ValueJson,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }) ?? new Dictionary<string, object>();

                        returnDtoList.Add(attributeDto);
                    }
                    catch
                    {
                        attributeDto.Value = new Dictionary<string, object>();
                    }
                }
                else
                {
                    attributeDto.Value = new Dictionary<string, object>();
                }
            }

            variant.Attributes = returnDtoList;
        }
        catch (Exception ex)
        {
            variant.Attributes = new List<VariantAttributeDto>();
        }
    }
}