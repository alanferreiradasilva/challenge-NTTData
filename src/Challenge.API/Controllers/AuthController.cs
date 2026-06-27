using Challenge.Application.Auth.Queries.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Challenge.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var query = new LoginQuery
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await _mediator.Send(query);

        if (result is null)
            return Unauthorized(new { message = "Invalid credentials." });

        return Ok(result);
    }
}

public record LoginRequest(string Email, string Password);
