using Challenge.Application.DTOs;

namespace Challenge.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResult?> LoginAsync(string email, string password);
}
