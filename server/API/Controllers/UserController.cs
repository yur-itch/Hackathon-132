using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Dtos;

namespace PlantCare.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/user")]
public sealed class UserController : ControllerBase
{
    private readonly AppDbContext _db;

    public UserController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Id == userId.Value);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(new UserDto(user.Id, user.Email, user.DisplayName, user.CreatedAt));
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateUserDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var displayName = dto.DisplayName.Trim();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return BadRequest("Display name is required.");
        }

        var user = await _db.Users.FirstOrDefaultAsync(item => item.Id == userId.Value);
        if (user is null)
        {
            return NotFound();
        }

        user.DisplayName = displayName;
        await _db.SaveChangesAsync();

        return Ok(new UserDto(user.Id, user.Email, user.DisplayName, user.CreatedAt));
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
