using FastEndpoints;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Abstractions.S3Storage;
using Shopilent.Application.Common.Constants;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Common;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;

namespace Shopilent.API.Endpoints.Test;

public class TestEndpoint : EndpointWithoutRequest<TestResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    // public TestEndpoint(IUnitOfWork unitOfWork)

    private readonly ApplicationDbContext _dbContext;
    private readonly IS3StorageService _s3StorageService;
private readonly IDateTimeProvider _dateTimeProvider;
    
    public TestEndpoint(ApplicationDbContext dbContext, IUnitOfWork unitOfWork, IS3StorageService s3Storage,
        IDateTimeProvider dateTimeProvider
        )
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _s3StorageService = s3Storage;
        _dateTimeProvider = dateTimeProvider;
    }

    public override void Configure()
    {
        Get("v1/test");
        Description(b => b
            .WithTags("Test")
            .WithDescription("Test Endpoint - Creates a test product"));
        Policies(AuthorizationPolicy.RequireAdminOrManager.ToString()); // Using our strongly typed enum
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var image = await _s3StorageService.GetPresignedUrlAsync("shopilent",
            "products/40027858-6656-4c3f-aa29-ab5d2008c002/d8a842f1-1954-4a0f-9184-7012a0b3dd0a.webp",
            TimeSpan.FromMinutes(5)
            
        );
        await SendAsync(
            new TestResponse { Message = image.Value }
        );
        return;
        var user = _dbContext.Users.Where(x => x.Email.Value == "jesse@demo.com").SingleOrDefault();
        if (user == null)
        {
            var newUser = Domain.Identity.User.Create(
                Email.Create("jesse@demo.com"),
                "Jesse123$",
                FullName.Create("Jesse", "Doe"));
            _dbContext.Users.Add(newUser.Value);
        }
        else
        {
            var refreshToken = user.AddRefreshToken(Guid.NewGuid().ToString(), DateTime.UtcNow.AddDays(7));
            //
        }

        await _dbContext.SaveChangesAsync(ct);

        await SendAsync(
            new TestResponse { Message = "Test Successful!" }
        );
        // try
        // {
        //     // Create a slug for the product
        //     var slugResult = Slug.Create("test-product");
        //     if (slugResult.IsFailure)
        //     {
        //         await SendAsync(new TestResponse { Message = $"Failed to create slug: {slugResult.Error.Message}" }, 400, ct);
        //         return;
        //     }
        //
        //     // Create money object for base price
        //     var priceResult = Money.Create(19.99m, "USD");
        //     if (priceResult.IsFailure)
        //     {
        //         await SendAsync(new TestResponse { Message = $"Failed to create price: {priceResult.Error.Message}" }, 400, ct);
        //         return;
        //     }
        //
        //     // Create a product using the domain factory method
        //     var productResult = Product.Create(
        //         name: "Test Product", 
        //         slug: slugResult.Value, 
        //         basePrice: priceResult.Value, 
        //         sku: "TEST-001");
        //
        //     if (productResult.IsFailure)
        //     {
        //         await SendAsync(new TestResponse { Message = $"Failed to create product: {productResult.Error.Message}" }, 400, ct);
        //         return;
        //     }
        //
        //     // Add the product to the repository
        //     await _unitOfWork.ProductRepository.AddAsync(productResult.Value, ct);
        //     
        //     // Save changes through the unit of work
        //     await _unitOfWork.SaveEntitiesAsync(ct);
        //
        //     // Return success response
        //     await SendOkAsync(
        //         new TestResponse
        //         {
        //             Message = $"Test Successful! Product created with ID: {productResult.Value.Id}"
        //         }, ct
        //     );
        // }
        // catch (Exception ex)
        // {
        //     await SendAsync(new TestResponse { Message = $"Error: {ex.Message}" }, 500, ct);
        // }
    }
}