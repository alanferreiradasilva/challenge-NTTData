using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Challenge.Tests.Integration;

public class OrdersApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrdersApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    private async Task<string> GetTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = "dev@martech.com",
            password = "Senha@123"
        });

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
        return result!.Token;
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

    [Fact]
    public async Task GetOrders_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_WithValidData_Returns201AndId()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            customerId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            items = new[]
            {
                new { productName = "Notebook", quantity = 2, unitPrice = 25.50 }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        body.GetProperty("id").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetOrders_ReturnsPagedResult()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new("Bearer", token);

        var response = await _client.GetAsync("/api/orders?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        result.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        result.GetProperty("page").GetInt32().Should().Be(1);
        result.GetProperty("pageSize").GetInt32().Should().Be(10);
    }

    [Fact]
    public async Task GetOrderById_ReturnsCorrectData()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/orders", new
        {
            customerId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            items = new[]
            {
                new { productName = "Mouse", quantity = 2, unitPrice = 25.50 }
            }
        });

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var orderId = created.GetProperty("id").GetGuid();

        var getResponse = await _client.GetAsync($"/api/orders/{orderId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var order = await getResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        order.GetProperty("status").GetString().Should().Be("Pending");
        order.GetProperty("totalAmount").GetDecimal().Should().Be(51.0m);
        order.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task CancelOrder_Returns204AndChangesStatus()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/orders", new
        {
            customerId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            items = new[]
            {
                new { productName = "Book", quantity = 1, unitPrice = 15.0 }
            }
        });

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var orderId = created.GetProperty("id").GetGuid();

        var cancelResponse = await _client.PatchAsync($"/api/orders/{orderId}/cancel", null);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/orders/{orderId}");
        var order = await getResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        order.GetProperty("status").GetString().Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelAlreadyCancelledOrder_Returns409()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/orders", new
        {
            customerId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            items = new[]
            {
                new { productName = "Pen", quantity = 1, unitPrice = 5.0 }
            }
        });

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var orderId = created.GetProperty("id").GetGuid();

        await _client.PatchAsync($"/api/orders/{orderId}/cancel", null);
        var secondCancel = await _client.PatchAsync($"/api/orders/{orderId}/cancel", null);

        secondCancel.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateOrder_WithEmptyItems_Returns400()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            customerId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            items = Array.Empty<object>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithQuantityZero_Returns400()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            customerId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            items = new[]
            {
                new { productName = "Test", quantity = 0, unitPrice = 10.0 }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetNonExistentOrder_Returns404()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new("Bearer", token);

        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private record LoginResponse(string Token, DateTime ExpiresAt);
}
