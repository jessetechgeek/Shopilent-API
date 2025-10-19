namespace Shopilent.Application.Features.Sales.Commands.CreateCart.V1;

public class CreateCartResponseV1
{
    public Guid Id { get; init; }
    public Guid? UserId { get; init; }
    public int ItemCount { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}