using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Dtos;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private const string AccessTokenCookieName = "access_token";
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto dto)
    {
        var user = await _authService.RegisterAsync(dto.Email, dto.Password, dto.DisplayName);
        if (user is null)
        {
            return BadRequest("Registration failed.");
        }

        var token = await _authService.LoginAsync(dto.Email, dto.Password);
        if (token is not null)
        {
            Response.Cookies.Append(AccessTokenCookieName, token, CreateAccessTokenCookieOptions());
        }

        return Ok(new UserDto(user.Id, user.Email, user.DisplayName, user.CreatedAt));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var token = await _authService.LoginAsync(dto.Email, dto.Password);
        if (token is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        Response.Cookies.Append(AccessTokenCookieName, token, CreateAccessTokenCookieOptions());
        return Ok(new AuthResponseDto(true));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AccessTokenCookieName, CreateAccessTokenCookieOptions());
        return NoContent();
    }

    private CookieOptions CreateAccessTokenCookieOptions()
    {
        var expiryMinutes = double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "1440");

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes),
            MaxAge = TimeSpan.FromMinutes(expiryMinutes),
            Path = "/"
        };
    }
}
