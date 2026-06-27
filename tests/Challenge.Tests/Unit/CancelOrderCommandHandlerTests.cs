using Challenge.Application.Orders.Commands.CancelOrder;
using Challenge.Domain.Entities;
using Challenge.Domain.Enums;
using Challenge.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Challenge.Tests.Unit;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _handler = new CancelOrderCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingPendingOrder_CancelsAndUpdates()
    {
        var order = new Order(Guid.NewGuid(), [new OrderItem("Test", 1, 10.0m)]);
        var orderId = order.Id;

        _repositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CancelOrderCommand { OrderId = orderId };
        await _handler.Handle(command, CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Cancelled);
        _repositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ThrowsKeyNotFoundException()
    {
        var orderId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new CancelOrderCommand { OrderId = orderId };
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Order with id {orderId} not found.");
    }
}
