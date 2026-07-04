using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Dtos;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reminders")]
public sealed class RemindersController : ControllerBase
{
    private readonly IReminderService _reminderService;

    public RemindersController(IReminderService reminderService)
    {
        _reminderService = reminderService;
    }

    [HttpGet]
    // dueOnly - получить только актуальные/просроченные напоминания (NextDueAt <= сейчас);
    // при false — все, по умолчанию false.
    public async Task<ActionResult<IReadOnlyCollection<ReminderDto>>> GetReminders([FromQuery] bool dueOnly = false)
    {
        var reminders = await _reminderService.GetMineAsync(this.GetOwnerId(), dueOnly);

        var dtos = reminders.Select(r => new ReminderDto(
            r.Id,
            r.UserPlantId,
            r.Type,
            r.NextDueAt,
            r.UserPlant?.Plant?.Name ?? r.UserPlant?.Nickname ?? "растение")).ToList();

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReminder([FromBody] CreateReminderDto dto)
    {
        var reminder = await _reminderService.CreateAsync(
            this.GetOwnerId(),
            dto.UserPlantId,
            dto.Type,
            dto.IntervalDays,
            dto.NextDueAt);

        if (reminder is null)
        {
            return NotFound("User plant not found.");
        }

        return Created($"/api/reminders/{reminder.Id}", reminder);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateReminder(Guid id, [FromBody] UpdateReminderDto dto)
    {
        var updated = await _reminderService.UpdateAsync(
            this.GetOwnerId(),
            id,
            dto.IntervalDays,
            dto.Enabled);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/done")]
    public async Task<IActionResult> MarkReminderDone(Guid id)
    {
        var updated = await _reminderService.MarkDoneAsync(id, this.GetOwnerId());

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteReminder(Guid id)
    {
        var deleted = await _reminderService.DeleteAsync(id, this.GetOwnerId());

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
