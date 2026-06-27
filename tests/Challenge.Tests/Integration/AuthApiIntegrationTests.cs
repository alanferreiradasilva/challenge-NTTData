using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Challenge.Tests.Integration;

public class AuthApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = "dev@martech.com",
            password = "Senha@123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = "wrong@email.com",
            password = "wrong"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private record LoginResponse(string Token, DateTime ExpiresAt);
}
