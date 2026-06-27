using Challenge.Application.DTOs;
using Challenge.Application.Orders.Queries.GetOrderById;
using Challenge.Domain.Entities;
using Challenge.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Challenge.Tests.Unit;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _handler = new GetOrderByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingOrder_ReturnsOrderDto()
    {
        var customerId = Guid.NewGuid();
        var order = new Order(customerId, [new OrderItem("Product", 3, 15.0m)]);
        _repositoryMock.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var query = new GetOrderByIdQuery { Id = order.Id };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.CustomerId.Should().Be(customerId);
        result.Status.Should().Be("Pending");
        result.TotalAmount.Should().Be(45.0m);
        result.Items.Should().HaveCount(1);
        result.Items[0].ProductName.Should().Be("Product");
        result.Items[0].Quantity.Should().Be(3);
        result.Items[0].UnitPrice.Should().Be(15.0m);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ReturnsNull()
    {
        var orderId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var query = new GetOrderByIdQuery { Id = orderId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
