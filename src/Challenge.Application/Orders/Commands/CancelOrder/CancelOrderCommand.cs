using MediatR;

namespace Challenge.Application.Orders.Commands.CancelOrder;

public record CancelOrderCommand : IRequest
{
    public Guid OrderId { get; init; }
}
