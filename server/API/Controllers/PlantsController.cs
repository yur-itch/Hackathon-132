using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Dtos;
using PlantCare.Api.Services;

namespace PlantCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PlantsController : ControllerBase
{
    private readonly IPlantsService _plantsService;

    public PlantsController(IPlantsService plantsService)
    {
        _plantsService = plantsService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PlantListItemDto>>> GetPlants(
        [FromQuery] string? search,
        [FromQuery] bool? isPoisonous)
    {
        var plants = await _plantsService.GetPlantsAsync(search, isPoisonous);
        return Ok(plants);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlantDto>> GetPlantById(
        int id)
    {
        var plant = await _plantsService.GetPlantByIdAsync(id);
        return plant is null ? NotFound() : Ok(plant);
    }
}
