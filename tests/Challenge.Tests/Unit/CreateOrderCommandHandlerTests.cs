using Challenge.Application.Orders.Commands.CreateOrder;
using Challenge.Domain.Entities;
using Challenge.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Challenge.Tests.Unit;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _handler = new CreateOrderCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsOrderId()
    {
        Order? addedOrder = null;
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((o, _) => addedOrder = o)
            .Returns(Task.CompletedTask);

        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items =
            [
                new CreateOrderItemDto { ProductName = "Notebook", Quantity = 2, UnitPrice = 25.50m }
            ]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        addedOrder.Should().NotBeNull();
        addedOrder!.Id.Should().Be(result);
        addedOrder.CustomerId.Should().Be(command.CustomerId);
        addedOrder.Items.Should().HaveCount(1);
        addedOrder.TotalAmount.Should().Be(51.0m);
    }

    [Fact]
    public async Task Handle_WithMultipleItems_ComputesTotalCorrectly()
    {
        Order? addedOrder = null;
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((o, _) => addedOrder = o)
            .Returns(Task.CompletedTask);

        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items =
            [
                new CreateOrderItemDto { ProductName = "Item A", Quantity = 3, UnitPrice = 10.0m },
                new CreateOrderItemDto { ProductName = "Item B", Quantity = 2, UnitPrice = 5.50m }
            ]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        addedOrder.Should().NotBeNull();
        addedOrder!.Items.Should().HaveCount(2);
        addedOrder.TotalAmount.Should().Be(41.0m);
    }
}
