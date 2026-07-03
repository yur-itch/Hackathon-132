using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Dtos;
using PlantCare.Api.Models;

namespace PlantCare.Api.Controllers;

/// <summary>Личная коллекция растений пользователя.</summary>
[ApiController]
[Route("api/[controller]")]
public class UserPlantsController : ControllerBase
{
    private readonly AppDbContext _db;
    public UserPlantsController(AppDbContext db) => _db = db;

    // В базовой версии владелец берётся из заголовка X-User-Id (по умолчанию "local").
    // Усложнение «авторизация» заменит это на id из токена.
    private string OwnerId =>
        Request.Headers.TryGetValue("X-User-Id", out var v) && !string.IsNullOrWhiteSpace(v)
            ? v.ToString() : "local";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserPlant>>> GetMine()
        => await _db.UserPlants
            .Include(up => up.Plant)
            .Where(up => up.OwnerId == OwnerId)
            .OrderByDescending(up => up.AddedAt)
            .ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserPlant>> GetById(int id)
    {
        var up = await _db.UserPlants.Include(x => x.Plant)
            .FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == OwnerId);
        return up is null ? NotFound() : up;
    }

    [HttpPost]
    public async Task<ActionResult<UserPlant>> Create(CreateUserPlantDto dto)
    {
        var up = new UserPlant
        {
            OwnerId = OwnerId,
            PlantId = dto.PlantId,
            Nickname = dto.Nickname,
            Location = dto.Location,
            Notes = dto.Notes
        };
        _db.UserPlants.Add(up);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = up.Id }, up);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserPlantDto dto)
    {
        var up = await _db.UserPlants.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == OwnerId);
        if (up is null) return NotFound();

        up.Nickname = dto.Nickname;
        up.Location = dto.Location;
        up.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var up = await _db.UserPlants.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == OwnerId);
        if (up is null) return NotFound();

        _db.UserPlants.Remove(up);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
