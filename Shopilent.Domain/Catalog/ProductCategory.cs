using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Domain.Catalog;

public class ProductCategory : Entity
{
    private ProductCategory()
    {
        // Required by EF Core
    }

    private ProductCategory(Product product, Category category)
    {
        ProductId = product.Id;
        CategoryId = category.Id;
    }

    // Add static factory method
    public static Result<ProductCategory> Create(Product product, Category category)
    {
        if (product == null)
            return Result.Failure<ProductCategory>(ProductErrors.NotFound(Guid.Empty));

        if (category == null)
            return Result.Failure<ProductCategory>(CategoryErrors.NotFound(Guid.Empty));
            
        if (!category.IsActive)
            return Result.Failure<ProductCategory>(CategoryErrors.InvalidCategoryStatus);

        return Result.Success(new ProductCategory(product, category));
    }

    public Guid ProductId { get; private set; }
    public Guid CategoryId { get; private set; }
}