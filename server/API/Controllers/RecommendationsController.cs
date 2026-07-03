using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    // Отдаём Plant напрямую — та же форма, что и в справочнике,
    // чтобы фронт рисовал рекомендации той же карточкой PlantCard.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<Plant>>> GetRecommendations(
        [FromQuery] int count = 3)
    {
        if (count <= 0)
        {
            return BadRequest("Count must be greater than zero.");
        }

        var recommendations = await _recommendationService.GetRecommendationsAsync(
            this.GetOwnerId(),
            count);

        return Ok(recommendations.ToList());
    }
}
