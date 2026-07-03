using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Dtos;
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
    public async Task<ActionResult<IReadOnlyCollection<PlantListItemDto>>> GetFavorites()
    {
        var favorites = await _favoritesService.GetFavoritesAsync(GetOwnerId());

        var plants = favorites
            .Where(favorite => favorite.Plant is not null)
            .Select(favorite => ToDto(favorite.Plant!))
            .ToList();

        return Ok(plants);
    }

    [HttpPost("{plantId:int}")]
    public async Task<IActionResult> AddFavorite(int plantId)
    {
        var alreadyFavorite = await _favoritesService.IsFavoriteAsync(GetOwnerId(), plantId);
        if (alreadyFavorite)
        {
            return Conflict("Plant is already in favorites.");
        }

        var added = await _favoritesService.AddToFavoritesAsync(GetOwnerId(), plantId);
        if (!added)
        {
            return NotFound("Plant not found.");
        }

        return Created($"/api/favorites/{plantId}", null);
    }

    [HttpDelete("{plantId:int}")]
    public async Task<IActionResult> DeleteFavorite(int plantId)
    {
        var deleted = await _favoritesService.RemoveFromFavoritesAsync(GetOwnerId(), plantId);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private string GetOwnerId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id claim is missing.");

    private static PlantListItemDto ToDto(Plant plant)
    {
        return new PlantListItemDto(
            plant.Id,
            plant.Name,
            plant.Description,
            BuildWateringRecommendations(plant),
            plant.Light,
            IsPoisonous(plant),
            plant.ImageUrl);
    }

    private static string BuildWateringRecommendations(Plant plant)
        => plant.WateringFrequencyDays > 0
            ? $"Every {plant.WateringFrequencyDays} days"
            : "No watering recommendation";

    private static bool IsPoisonous(Plant plant)
        => !string.IsNullOrWhiteSpace(plant.Toxicity) &&
           !plant.Toxicity.Contains("нетокс", StringComparison.OrdinalIgnoreCase) &&
           !plant.Toxicity.Contains("non-toxic", StringComparison.OrdinalIgnoreCase) &&
           !plant.Toxicity.Contains("not toxic", StringComparison.OrdinalIgnoreCase);
}
