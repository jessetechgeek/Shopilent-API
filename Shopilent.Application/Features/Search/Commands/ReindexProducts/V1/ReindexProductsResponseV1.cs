namespace Shopilent.Application.Features.Search.Commands.ReindexProducts.V1;

public class ReindexProductsResponseV1
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ProductsIndexed { get; set; }
    public DateTime IndexedAt { get; set; }
    public TimeSpan Duration { get; set; }
}