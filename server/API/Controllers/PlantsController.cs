using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Controllers;

[ApiController]
[Route("api/plants")]
public sealed class PlantsController : ControllerBase
{
    private readonly IPlantsService _plantsService;

    public PlantsController(IPlantsService plantsService)
    {
        _plantsService = plantsService;
    }

    // Отдаём Plant напрямую (как и остальные read-эндпоинты) — так OpenAPI-схема
    // совпадает с реальным JSON, который читает фронт.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<Plant>>> GetPlants(
        [FromQuery] bool? isPoisonous)
    {
        var plants = await _plantsService.GetPlantsAsync(isPoisonous);
        return Ok(plants);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Plant>> GetPlantById(
        int id)
    {
        var plant = await _plantsService.GetPlantByIdAsync(id);
        return plant is null ? NotFound() : Ok(plant);
    }
}
