namespace Challenge.Application.DTOs;

public record AuthResult
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
