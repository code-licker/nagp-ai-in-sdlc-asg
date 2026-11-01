using InvoiceApp.Api.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace InvoiceApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly DemoUsersOptions _options;
    private readonly SessionStore _sessions;

    public AuthController(IOptions<DemoUsersOptions> options, SessionStore sessions)
    {
        _options = options.Value;
        _sessions = sessions;
    }

    public record LoginRequest(string Username, string Password);
    public record LoginResponse(string Token, string Username, string Role);

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var user = _options.Users.FirstOrDefault(u =>
            u.Username == request.Username && u.Password == request.Password);

        if (user is null)
        {
            return Unauthorized(new { error = "Invalid username or password." });
        }

        var token = _sessions.Create(new UserSession(user.Username, user.Role));
        return Ok(new LoginResponse(token, user.Username, user.Role));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var header = Request.Headers.Authorization.ToString();
        if (header.StartsWith("Bearer "))
        {
            _sessions.Remove(header["Bearer ".Length..]);
        }

        return NoContent();
    }
}
