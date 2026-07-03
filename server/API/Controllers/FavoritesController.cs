using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Models;

namespace PlantCare.Api.Controllers;

/// <summary>Избранные растения из справочника.</summary>
[ApiController]
[Route("api/[controller]")]
public class FavoritesController : ControllerBase
{
    private readonly AppDbContext _db;
    public FavoritesController(AppDbContext db) => _db = db;

    private string OwnerId =>
        Request.Headers.TryGetValue("X-User-Id", out var v) && !string.IsNullOrWhiteSpace(v)
            ? v.ToString() : "local";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Plant>>> GetMine()
        => await _db.Favorites
            .Where(f => f.OwnerId == OwnerId)
            .Include(f => f.Plant)
            .Select(f => f.Plant!)
            .ToListAsync();

    [HttpPost("{plantId:int}")]
    public async Task<IActionResult> Add(int plantId)
    {
        if (!await _db.Plants.AnyAsync(p => p.Id == plantId)) return NotFound();
        if (await _db.Favorites.AnyAsync(f => f.OwnerId == OwnerId && f.PlantId == plantId))
            return NoContent();

        _db.Favorites.Add(new Favorite { OwnerId = OwnerId, PlantId = plantId });
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{plantId:int}")]
    public async Task<IActionResult> Remove(int plantId)
    {
        var fav = await _db.Favorites.FirstOrDefaultAsync(f => f.OwnerId == OwnerId && f.PlantId == plantId);
        if (fav is null) return NotFound();

        _db.Favorites.Remove(fav);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
