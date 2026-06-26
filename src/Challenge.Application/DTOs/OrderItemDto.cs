namespace Challenge.Application.DTOs;

public record OrderItemDto
{
    public Guid Id { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
