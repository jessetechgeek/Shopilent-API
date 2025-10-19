using Shopilent.Domain.Catalog;
using Shopilent.Domain.Identity;
using Shopilent.Infrastructure.IntegrationTests.TestData.Builders;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;

namespace Shopilent.Infrastructure.IntegrationTests.TestData.Fixtures;

public static class TestDataSeeder
{
    public static async Task<List<Category>> SeedCategoriesAsync(ApplicationDbContext context, int count = 5)
    {
        var categories = CategoryBuilder.CreateMany(count);
        
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
        
        return categories;
    }

    public static async Task<List<Product>> SeedProductsAsync(ApplicationDbContext context, int count = 10, List<Guid>? categoryIds = null)
    {
        var products = new List<Product>();
        
        for (int i = 0; i < count; i++)
        {
            var productBuilder = ProductBuilder.Random();
            
            // Note: Category relationship handling simplified for now
            
            products.Add(productBuilder.Build());
        }
        
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
        
        return products;
    }

    public static async Task<List<User>> SeedUsersAsync(ApplicationDbContext context, int count = 5)
    {
        var users = UserBuilder.CreateMany(count);
        
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
        
        return users;
    }

    public static async Task<(List<Category> categories, List<Product> products, List<User> users)> SeedCompleteTestDataAsync(
        ApplicationDbContext context,
        int categoriesCount = 3,
        int productsCount = 10,
        int usersCount = 5)
    {
        var categories = await SeedCategoriesAsync(context, categoriesCount);
        var categoryIds = categories.Select(c => c.Id).ToList();
        
        var products = await SeedProductsAsync(context, productsCount, categoryIds);
        var users = await SeedUsersAsync(context, usersCount);
        
        return (categories, products, users);
    }

    public static async Task<Category> SeedSingleCategoryAsync(ApplicationDbContext context, string? name = null)
    {
        var categoryBuilder = CategoryBuilder.Random();
        
        if (!string.IsNullOrEmpty(name))
        {
            categoryBuilder.WithName(name);
        }
        
        var category = categoryBuilder.Build();
        
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();
        
        return category;
    }

    public static async Task<Product> SeedSingleProductAsync(ApplicationDbContext context, string? name = null, Guid? categoryId = null)
    {
        var productBuilder = ProductBuilder.Random();
        
        if (!string.IsNullOrEmpty(name))
        {
            productBuilder.WithName(name);
        }
        
        // Note: Category relationship handling simplified for now
        
        var product = productBuilder.Build();
        
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
        
        return product;
    }

    public static async Task<User> SeedSingleUserAsync(ApplicationDbContext context, string? email = null, bool isAdmin = false)
    {
        var userBuilder = UserBuilder.Random();
        
        if (!string.IsNullOrEmpty(email))
        {
            userBuilder.WithEmail(email);
        }
        
        if (isAdmin)
        {
            userBuilder.AsAdmin().WithVerifiedEmail();
        }
        
        var user = userBuilder.Build();
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        
        return user;
    }
}