using Challenge.Domain.Entities;
using Challenge.Domain.Interfaces;
using MediatR;

namespace Challenge.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _repository;

    public CreateOrderCommandHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var items = request.Items
            .Select(i => new OrderItem(i.ProductName, i.Quantity, i.UnitPrice))
            .ToList();

        var order = new Order(request.CustomerId, items);

        await _repository.AddAsync(order, cancellationToken);

        return order.Id;
    }
}
