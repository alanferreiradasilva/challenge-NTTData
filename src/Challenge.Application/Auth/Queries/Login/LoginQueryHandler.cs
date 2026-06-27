using Challenge.Application.DTOs;
using Challenge.Application.Interfaces;
using MediatR;

namespace Challenge.Application.Auth.Queries.Login;

public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResult?>
{
    private readonly IAuthService _authService;

    public LoginQueryHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResult?> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.Email, request.Password);
    }
}
