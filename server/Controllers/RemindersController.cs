using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Dtos;
using PlantCare.Api.Models;

namespace PlantCare.Api.Controllers;

/// <summary>Напоминания по растениям пользователя (полив, пересадка, подкормка).</summary>
[ApiController]
[Route("api/[controller]")]
public class RemindersController : ControllerBase
{
    private readonly AppDbContext _db;
    public RemindersController(AppDbContext db) => _db = db;

    private string OwnerId =>
        Request.Headers.TryGetValue("X-User-Id", out var v) && !string.IsNullOrWhiteSpace(v)
            ? v.ToString() : "local";

    /// <summary>Все напоминания пользователя. due=true — только просроченные/сегодняшние.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reminder>>> GetMine([FromQuery] bool due = false)
    {
        var q = _db.Reminders
            .Where(r => r.UserPlant!.OwnerId == OwnerId && r.Enabled);

        if (due)
            q = q.Where(r => r.NextDueAt <= DateTime.UtcNow);

        return await q.OrderBy(r => r.NextDueAt).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Reminder>> Create(CreateReminderDto dto)
    {
        // проверяем, что растение принадлежит пользователю
        var owns = await _db.UserPlants.AnyAsync(x => x.Id == dto.UserPlantId && x.OwnerId == OwnerId);
        if (!owns) return BadRequest("UserPlant не найдено у текущего пользователя.");

        var r = new Reminder
        {
            UserPlantId = dto.UserPlantId,
            Type = dto.Type,
            IntervalDays = dto.IntervalDays,
            NextDueAt = dto.NextDueAt ?? DateTime.UtcNow.AddDays(dto.IntervalDays)
        };
        _db.Reminders.Add(r);
        await _db.SaveChangesAsync();
        return Created($"/api/reminders/{r.Id}", r);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateReminderDto dto)
    {
        var r = await _db.Reminders.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserPlant!.OwnerId == OwnerId);
        if (r is null) return NotFound();

        r.IntervalDays = dto.IntervalDays;
        r.Enabled = dto.Enabled;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Отметить действие выполненным — сдвигает следующий срок на интервал.</summary>
    [HttpPost("{id:int}/done")]
    public async Task<IActionResult> MarkDone(int id)
    {
        var r = await _db.Reminders.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserPlant!.OwnerId == OwnerId);
        if (r is null) return NotFound();

        r.LastDoneAt = DateTime.UtcNow;
        r.NextDueAt = DateTime.UtcNow.AddDays(r.IntervalDays);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _db.Reminders.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserPlant!.OwnerId == OwnerId);
        if (r is null) return NotFound();

        _db.Reminders.Remove(r);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
