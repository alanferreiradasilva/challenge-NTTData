using Challenge.Domain.Enums;

namespace Challenge.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(i => i.Quantity * i.UnitPrice);

    private Order() { }

    public Order(Guid customerId, List<OrderItem> items)
    {
        if (items is null || items.Count == 0)
            throw new ArgumentException("Order must have at least one item.", nameof(items));

        if (items.Any(i => i.Quantity <= 0))
            throw new ArgumentException("Quantity must be greater than zero.");

        if (items.Any(i => i.UnitPrice <= 0))
            throw new ArgumentException("UnitPrice must be greater than zero.");

        Id = Guid.NewGuid();
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        foreach (var item in items)
        {
            item.SetOrderId(Id);
            _items.Add(item);
        }
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only orders with Pending status can be cancelled.");

        Status = OrderStatus.Cancelled;
    }
}
