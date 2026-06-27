using Challenge.Application.Auth.Queries.Login;
using Challenge.Application.DTOs;
using Challenge.Application.Interfaces;
using FluentAssertions;
using Moq;

namespace Challenge.Tests.Unit;

public class LoginQueryHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly LoginQueryHandler _handler;

    public LoginQueryHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _handler = new LoginQueryHandler(_authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsAuthResult()
    {
        var expected = new AuthResult { Token = "jwt-token", ExpiresAt = DateTime.UtcNow.AddHours(1) };
        _authServiceMock.Setup(x => x.LoginAsync("dev@martech.com", "Senha@123"))
            .ReturnsAsync(expected);

        var query = new LoginQuery { Email = "dev@martech.com", Password = "Senha@123" };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ReturnsNull()
    {
        _authServiceMock.Setup(x => x.LoginAsync("wrong@email.com", "wrong"))
            .ReturnsAsync((AuthResult?)null);

        var query = new LoginQuery { Email = "wrong@email.com", Password = "wrong" };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
