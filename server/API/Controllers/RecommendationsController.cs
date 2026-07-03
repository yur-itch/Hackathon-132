using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Dtos;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/recommendations")]
public sealed class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PlantListItemDto>>> GetRecommendations(
        [FromQuery] int count = 3)
    {
        if (count <= 0)
        {
            return BadRequest("Count must be greater than zero.");
        }

        var recommendations = await _recommendationService.GetRecommendationsAsync(
            this.GetOwnerId(),
            count);

        return Ok(recommendations.Select(ToDto).ToList());
    }

    private static PlantListItemDto ToDto(Plant plant)
    {
        return new PlantListItemDto(
            plant.Id,
            plant.Name,
            plant.Description,
            $"Water every {plant.WateringFrequencyDays} days",
            plant.Light,
            !string.IsNullOrWhiteSpace(plant.Toxicity),
            plant.ImageUrl);
    }
}
