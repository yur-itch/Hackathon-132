using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Dtos;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Controllers;

[ApiController]
[Route("api/reminders")]
public sealed class RemindersController : ControllerBase
{
    private readonly IReminderService _reminderService;

    public RemindersController(IReminderService reminderService)
    {
        _reminderService = reminderService;
    }

    [HttpGet]
    // dueOnly - получить тлько актуальные или просроченные напоминания, где дата уже все
    // иначе все напоминания если false, по умолчанию flase
    public async Task<IActionResult> GetReminders([FromQuery] bool dueOnly = false)
    {
        var reminders = await _reminderService.GetMineAsync(GetOwnerId(), dueOnly);
        return Ok(reminders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReminder([FromBody] CreateReminderDto dto)
    {
        var reminder = await _reminderService.CreateAsync(
            GetOwnerId(),
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateReminder(int id, [FromBody] UpdateReminderDto dto)
    {
        var updated = await _reminderService.UpdateAsync(
            GetOwnerId(),
            id,
            dto.IntervalDays,
            dto.Enabled);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id:int}/done")]
    public async Task<IActionResult> MarkReminderDone(int id)
    {
        var updated = await _reminderService.MarkDoneAsync(id, GetOwnerId());

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReminder(int id)
    {
        var deleted = await _reminderService.DeleteAsync(id, GetOwnerId());

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private string GetOwnerId()
        => Request.Headers.TryGetValue("X-User-Id", out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : "local";
}
