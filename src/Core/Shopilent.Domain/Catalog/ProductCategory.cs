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
    internal static ProductCategory Create(Product product, Category category)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (category == null)
            throw new ArgumentNullException(nameof(category));

        if (!category.IsActive)
            throw new ArgumentException("Category is not active", nameof(category));

        return new ProductCategory(product, category);
    }

    // For use by the aggregates which should validate inputs
    internal static Result<ProductCategory> Create(Result<Product> productResult, Category category)
    {
        if (productResult.IsFailure)
            return Result.Failure<ProductCategory>(productResult.Error);

        if (category == null)
            return Result.Failure<ProductCategory>(CategoryErrors.NotFound(Guid.Empty));

        if (!category.IsActive)
            return Result.Failure<ProductCategory>(CategoryErrors.InvalidCategoryStatus);

        return Result.Success(new ProductCategory(productResult.Value, category));
    }

    public Guid ProductId { get; private set; }
    public Guid CategoryId { get; private set; }
}