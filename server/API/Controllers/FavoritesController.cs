using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/favorites")]
public sealed class FavoritesController : ControllerBase
{
    private readonly IFavoritesService _favoritesService;

    public FavoritesController(IFavoritesService favoritesService)
    {
        _favoritesService = favoritesService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<Plant>>> GetFavorites()
    {
        var favorites = await _favoritesService.GetFavoritesAsync(this.GetOwnerId());

        var plants = favorites
            .Where(favorite => favorite.Plant is not null)
            .Select(favorite => favorite.Plant!)
            .ToList();

        return Ok(plants);
    }

    [HttpPost("{plantId:int}")]
    public async Task<IActionResult> AddFavorite(int plantId)
    {
        var alreadyFavorite = await _favoritesService.IsFavoriteAsync(this.GetOwnerId(), plantId);
        if (alreadyFavorite)
        {
            return Conflict("Plant is already in favorites.");
        }

        var added = await _favoritesService.AddToFavoritesAsync(this.GetOwnerId(), plantId);
        if (!added)
        {
            return NotFound("Plant not found.");
        }

        return Created($"/api/favorites/{plantId}", null);
    }

    [HttpDelete("{plantId:int}")]
    public async Task<IActionResult> DeleteFavorite(int plantId)
    {
        var deleted = await _favoritesService.RemoveFromFavoritesAsync(this.GetOwnerId(), plantId);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
