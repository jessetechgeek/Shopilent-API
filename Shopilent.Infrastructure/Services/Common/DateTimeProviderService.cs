using Shopilent.Domain.Common;

namespace Shopilent.Infrastructure.Services.Common;

public class DateTimeProviderService : IDateTimeProvider
{
    public DateTime Now => DateTime.UtcNow;
}