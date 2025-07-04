using Microsoft.Extensions.Logging;
using Moq;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Catalog.Repositories.Read;
using Shopilent.Domain.Catalog.Repositories.Write;
using Shopilent.Domain.Identity.Repositories.Read;
using Shopilent.Domain.Identity.Repositories.Write;

namespace Shopilent.Application.UnitTests.Testing;

/// <summary>
/// Test fixture for setting up shared resources for unit tests
/// </summary>
public class TestFixture
{
    // Mock services
    public Mock<IUnitOfWork> MockUnitOfWork { get; private set; }
    public Mock<ICurrentUserContext> MockCurrentUserContext { get; private set; }
    public Mock<ICacheService> MockCacheService { get; private set; }
    public Mock<IAuthenticationService> MockAuthenticationService { get; private set; }
    
    // Mock repositories
    public Mock<ICategoryReadRepository> MockCategoryReadRepository { get; private set; }
    public Mock<ICategoryWriteRepository> MockCategoryWriteRepository { get; private set; }
    public Mock<IProductReadRepository> MockProductReadRepository { get; private set; }
    public Mock<IProductWriteRepository> MockProductWriteRepository { get; private set; }
    public Mock<IAttributeReadRepository> MockAttributeReadRepository { get; private set; }
    public Mock<IAttributeWriteRepository> MockAttributeWriteRepository { get; private set; }
    public Mock<IUserReadRepository> MockUserReadRepository { get; private set; }
    public Mock<IUserWriteRepository> MockUserWriteRepository { get; private set; }
    
    // Generic mocks for different logger types
    private readonly Dictionary<Type, object> _loggers = new();

    public TestFixture()
    {
        SetUpMocks();
    }

    private void SetUpMocks()
    {
        // Initialize mocks
        MockUnitOfWork = new Mock<IUnitOfWork>();
        MockCurrentUserContext = new Mock<ICurrentUserContext>();
        MockCacheService = new Mock<ICacheService>();
        MockAuthenticationService = new Mock<IAuthenticationService>();
        
        // Initialize repository mocks
        MockCategoryReadRepository = new Mock<ICategoryReadRepository>();
        MockCategoryWriteRepository = new Mock<ICategoryWriteRepository>();
        MockProductReadRepository = new Mock<IProductReadRepository>();
        MockProductWriteRepository = new Mock<IProductWriteRepository>();
        MockAttributeReadRepository = new Mock<IAttributeReadRepository>();
        MockAttributeWriteRepository = new Mock<IAttributeWriteRepository>();
        MockUserReadRepository = new Mock<IUserReadRepository>();
        MockUserWriteRepository = new Mock<IUserWriteRepository>();
        
        // Set up Unit of Work to return the repository mocks
        MockUnitOfWork.Setup(uow => uow.CategoryReader).Returns(MockCategoryReadRepository.Object);
        MockUnitOfWork.Setup(uow => uow.CategoryWriter).Returns(MockCategoryWriteRepository.Object);
        MockUnitOfWork.Setup(uow => uow.ProductReader).Returns(MockProductReadRepository.Object);
        MockUnitOfWork.Setup(uow => uow.ProductWriter).Returns(MockProductWriteRepository.Object);
        MockUnitOfWork.Setup(uow => uow.AttributeReader).Returns(MockAttributeReadRepository.Object);
        MockUnitOfWork.Setup(uow => uow.AttributeWriter).Returns(MockAttributeWriteRepository.Object);
        MockUnitOfWork.Setup(uow => uow.UserReader).Returns(MockUserReadRepository.Object);
        MockUnitOfWork.Setup(uow => uow.UserWriter).Returns(MockUserWriteRepository.Object);
        
        // Setup default save changes to return success
        MockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        
        MockUnitOfWork.Setup(uow => uow.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        // Set up basic CurrentUserContext behaviors
        MockCurrentUserContext.Setup(ctx => ctx.UserId).Returns((Guid?)null);
        MockCurrentUserContext.Setup(ctx => ctx.IsAuthenticated).Returns(false);
    }

    /// <summary>
    /// Get a mock logger for the specified type
    /// </summary>
    public ILogger<T> GetLogger<T>()
    {
        var type = typeof(T);
        if (!_loggers.ContainsKey(type))
        {
            // Create a NullLogger instead of a mocked logger
            _loggers[type] = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<T>();
        }
    
        return (ILogger<T>)_loggers[type];
    }
    
    /// <summary>
    /// Set the current user context for tests requiring an authenticated user
    /// </summary>
    public void SetAuthenticatedUser(Guid userId, string email = "test@example.com", bool isAdmin = false)
    {
        MockCurrentUserContext.Setup(ctx => ctx.UserId).Returns(userId);
        MockCurrentUserContext.Setup(ctx => ctx.Email).Returns(email);
        MockCurrentUserContext.Setup(ctx => ctx.IsAuthenticated).Returns(true);
        MockCurrentUserContext.Setup(ctx => ctx.IsInRole("Admin")).Returns(isAdmin);
    }
    
    
}