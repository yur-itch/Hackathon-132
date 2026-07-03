using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Dtos;
using PlantCare.Api.Services;

namespace PlantCare.Api.Controllers;

[ApiController]
[Route("api/user-plants")]
public sealed class UserPlantsController : ControllerBase
{
    private readonly IUserPlantsService _userPlantsService;

    public UserPlantsController(IUserPlantsService userPlantsService)
    {
        _userPlantsService = userPlantsService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserPlantDto>>> GetUserPlants()
    {
        var userPlants = await _userPlantsService.GetUserPlantsAsync(GetOwnerId());
        return Ok(userPlants);
    }

    [HttpPost]
    public async Task<ActionResult<UserPlantDto>> AddUserPlant(
        [FromBody] CreateUserPlantDto dto)
    {
        var result = await _userPlantsService.AddUserPlantAsync(GetOwnerId(), dto);

        return result.Result switch
        {
            CreateUserPlantResult.Created => CreatedAtAction(
                nameof(GetUserPlants),
                new { id = result.UserPlant!.Id },
                result.UserPlant),
            CreateUserPlantResult.PlantNotFound => NotFound("Plant not found."),
            CreateUserPlantResult.AlreadyExists => Conflict("Plant is already added to the collection."),
            _ => BadRequest()
        };
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserPlantDto>> UpdateUserPlant(
        int id,
        [FromBody] UpdateUserPlantDto dto)
    {
        var userPlant = await _userPlantsService.UpdateUserPlantAsync(GetOwnerId(), id, dto);
        return userPlant is null ? NotFound() : Ok(userPlant);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUserPlant(
        int id)
    {
        var deleted = await _userPlantsService.DeleteUserPlantAsync(GetOwnerId(), id);
        return deleted ? NoContent() : NotFound();
    }

    private string GetOwnerId()
        => Request.Headers.TryGetValue("X-User-Id", out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : "local";
}
