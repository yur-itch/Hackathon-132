using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Dtos;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Controllers;

[ApiController]
[Authorize]
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
        return Ok(userPlants.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<UserPlantDto>> AddUserPlant(
        [FromBody] CreateUserPlantDto dto)
    {
        var result = await _userPlantsService.AddUserPlantAsync(GetOwnerId(), 
        dto.PlantId,
        dto.Note,
        dto.NextWateringDate,
        dto.NextRepottingDate
        );

        if (result.Result == CreateUserPlantResult.Created)
        {
            var userPlantDto = ToDto(result.UserPlant!);
            return CreatedAtAction(nameof(GetUserPlants), new { id = userPlantDto.Id }, userPlantDto);
        }

        if (result.Result == CreateUserPlantResult.PlantNotFound)
        {
            return NotFound("Plant not found.");
        }

        if (result.Result == CreateUserPlantResult.AlreadyExists)
        {
            return Conflict("Plant is already added to the collection.");
        }

        return BadRequest();
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserPlantDto>> UpdateUserPlant(
        int id,
        [FromBody] UpdateUserPlantDto dto)
    {
        var userPlant = await _userPlantsService.UpdateUserPlantAsync(GetOwnerId(), id, 
        dto.Note,
        dto.NextWateringDate,
        dto.NextRepottingDate);

        if (userPlant is null)
        {
            return NotFound();
        }

        return Ok(ToDto(userPlant));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUserPlant(
        int id)
    {
        var deleted = await _userPlantsService.DeleteUserPlantAsync(GetOwnerId(), id);
        return deleted ? NoContent() : NotFound();
    }

    private string GetOwnerId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id claim is missing.");

    private static UserPlantDto ToDto(UserPlant userPlant)
    {
        var wateringReminder = userPlant.Reminders
            .FirstOrDefault(reminder => reminder.Type == ReminderType.Watering && reminder.Enabled);

        var repottingReminder = userPlant.Reminders
            .FirstOrDefault(reminder => reminder.Type == ReminderType.Repotting && reminder.Enabled);

        return new UserPlantDto(
            userPlant.Id,
            userPlant.PlantId ?? 0,
            userPlant.Plant?.Name ?? userPlant.Nickname,
            userPlant.Plant?.ImageUrl,
            userPlant.Notes,
            wateringReminder?.NextDueAt,
            repottingReminder?.NextDueAt);
    }
}
