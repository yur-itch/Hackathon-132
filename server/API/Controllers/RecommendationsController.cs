using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Controllers;

[ApiController]
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
    // Для залогиненных коллекция берётся из БД; гость передаёт id растений
    // своей localStorage-коллекции через ?plantIds=1,2,3.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<Plant>>> GetRecommendations(
        [FromQuery] int count = 3,
        [FromQuery] string? plantIds = null)
    {
        if (count <= 0)
        {
            return BadRequest("Count must be greater than zero.");
        }

        IEnumerable<Plant> recommendations;
        if (User.Identity?.IsAuthenticated == true)
        {
            recommendations = await _recommendationService.GetRecommendationsAsync(
                this.GetOwnerId(),
                count);
        }
        else
        {
            var ids = (plantIds ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => int.TryParse(part, out var id) ? id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            recommendations = await _recommendationService.GetRecommendationsForCollectionAsync(ids, count);
        }

        return Ok(recommendations.ToList());
    }
}
