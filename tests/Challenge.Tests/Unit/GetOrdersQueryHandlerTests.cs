using Challenge.Application.DTOs;
using Challenge.Application.Orders.Queries.GetOrders;
using Challenge.Domain.Entities;
using Challenge.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Challenge.Tests.Unit;

public class GetOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly GetOrdersQueryHandler _handler;

    public GetOrdersQueryHandlerTests()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _handler = new GetOrdersQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingOrders_ReturnsPagedResult()
    {
        var orders = new List<Order>
        {
            new Order(Guid.NewGuid(), [new OrderItem("Item 1", 2, 10.0m)]),
            new Order(Guid.NewGuid(), [new OrderItem("Item 2", 1, 5.0m)])
        };

        _repositoryMock.Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((orders.AsReadOnly(), 2));

        var query = new GetOrdersQuery { Page = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNoOrders_ReturnsEmptyPagedResult()
    {
        _repositoryMock.Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Order>().AsReadOnly(), 0));

        var query = new GetOrdersQuery { Page = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectTotalPages()
    {
        var orders = new List<Order>
        {
            new Order(Guid.NewGuid(), [new OrderItem("Item", 1, 10.0m)])
        };

        _repositoryMock.Setup(x => x.GetPagedAsync(1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((orders.AsReadOnly(), 12));

        var query = new GetOrdersQuery { Page = 1, PageSize = 5 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalPages.Should().Be(3);
    }
}
