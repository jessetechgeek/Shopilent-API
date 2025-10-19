namespace Shopilent.Domain.Common;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}