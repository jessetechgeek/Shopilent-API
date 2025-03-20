using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog;

public class ProductCategory : Entity
{
    private ProductCategory()
    {
        // Required by EF Core
    }

    private ProductCategory(Product product, Category category)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (category == null)
            throw new ArgumentNullException(nameof(category));

        ProductId = product.Id;
        CategoryId = category.Id;
    }

    // Add static factory method
    public static ProductCategory Create(Product product, Category category)
    {
        return new ProductCategory(product, category);
    }

    public Guid ProductId { get; private set; }
    public Guid CategoryId { get; private set; }
}