namespace Shopilent.Domain.Sales.DTOs;

public class CartDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public IReadOnlyList<CartItemDto> Items { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}