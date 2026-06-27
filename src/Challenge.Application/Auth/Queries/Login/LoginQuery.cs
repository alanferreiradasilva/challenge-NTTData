using Challenge.Application.DTOs;
using MediatR;

namespace Challenge.Application.Auth.Queries.Login;

public record LoginQuery : IRequest<AuthResult?>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
