using Challenge.Domain.Interfaces;
using MediatR;

namespace Challenge.Application.Orders.Commands.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _repository;

    public CancelOrderCommandHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with id {request.OrderId} not found.");

        order.Cancel();

        await _repository.UpdateAsync(order, cancellationToken);
    }
}
