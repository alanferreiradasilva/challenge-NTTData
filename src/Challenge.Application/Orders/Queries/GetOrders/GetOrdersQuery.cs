using Challenge.Application.DTOs;
using MediatR;

namespace Challenge.Application.Orders.Queries.GetOrders;

public record GetOrdersQuery : IRequest<PagedResult<OrderDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
