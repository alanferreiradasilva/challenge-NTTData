using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Challenge.Application.DTOs;
using Challenge.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Challenge.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly JwtSettings _settings;

    private const string FixedEmail = "dev@martech.com";
    private const string FixedPassword = "Senha@123";

    public AuthService(JwtSettings settings)
    {
        _settings = settings;
    }

    public Task<AuthResult?> LoginAsync(string email, string password)
    {
        if (email != FixedEmail || password != FixedPassword)
            return Task.FromResult<AuthResult?>(null);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_settings.Secret);
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpirationInMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, email),
                new Claim(ClaimTypes.Email, email)
            ]),
            Expires = expiresAt,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Task.FromResult<AuthResult?>(new AuthResult
        {
            Token = tokenString,
            ExpiresAt = expiresAt
        });
    }
}
