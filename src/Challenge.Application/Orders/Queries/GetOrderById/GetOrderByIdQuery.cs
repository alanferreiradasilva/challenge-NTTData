using Challenge.Application.DTOs;
using MediatR;

namespace Challenge.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery : IRequest<OrderDto?>
{
    public Guid Id { get; init; }
}
