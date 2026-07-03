using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Dtos;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

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
        var userPlants = await _userPlantsService.GetUserPlantsAsync(this.GetOwnerId());
        return Ok(userPlants.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<UserPlantDto>> AddUserPlant(
        [FromBody] CreateUserPlantDto dto)
    {
        var result = await _userPlantsService.AddUserPlantAsync(
            this.GetOwnerId(),
            dto.PlantId,
            dto.Note,
            dto.NextWateringDate,
            dto.NextRepottingDate);

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

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserPlantDto>> UpdateUserPlant(
        Guid id,
        [FromBody] UpdateUserPlantDto dto)
    {
        var userPlant = await _userPlantsService.UpdateUserPlantAsync(
            this.GetOwnerId(),
            id,
            dto.Note,
            dto.NextWateringDate,
            dto.NextRepottingDate);

        return userPlant is null ? NotFound() : Ok(ToDto(userPlant));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUserPlant(
        Guid id)
    {
        var deleted = await _userPlantsService.DeleteUserPlantAsync(this.GetOwnerId(), id);
        return deleted ? NoContent() : NotFound();
    }

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
            repottingReminder?.NextDueAt,
            wateringReminder?.Id,
            repottingReminder?.Id);
    }
}
