using MediatR;

namespace Challenge.Application.Orders.Commands.CancelOrder;

public record CancelOrderCommand : IRequest<Unit>
{
    public Guid OrderId { get; init; }
}
