namespace Shopilent.Application.Features.Catalog.Commands.UpdateVariant.V1;

public class UpdateVariantResponseV1
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string? Sku { get; init; }
    public decimal Price { get; init; }
    public string? Currency { get; init; }
    public int StockQuantity { get; init; }
    public bool IsActive { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
    public List<VariantImageResponseDto> Images { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class VariantImageResponseDto
{
    public string Url { get; init; }
    public string AltText { get; init; }
    public bool IsDefault { get; init; }
    public int DisplayOrder { get; init; }
}