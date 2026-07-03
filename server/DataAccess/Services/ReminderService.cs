using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Dtos;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Services.Implementations;

public class ReminderService : IReminderService
{
    private readonly AppDbContext _db;

    public ReminderService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Reminder>> GetMineAsync(string ownerId, bool dueOnly)
    {
        var q = _db.Reminders
            .Include(r => r.UserPlant)
            .ThenInclude(up => up!.Plant)
            .Where(r => r.UserPlant!.OwnerId == ownerId && r.Enabled);

        if (dueOnly)
        {
            q = q.Where(r => r.NextDueAt <= DateTime.UtcNow);
        }

        return await q.OrderBy(r => r.NextDueAt).ToListAsync();
    }

    public async Task<Reminder?> CreateAsync(CreateReminderDto dto, string ownerId)
    {
        // Проверяем, что UserPlant существует и принадлежит текущему пользователю
        var owns = await _db.UserPlants.AnyAsync(x => x.Id == dto.UserPlantId && x.OwnerId == ownerId);
        if (!owns) return null;

        var r = new Reminder
        {
            UserPlantId = dto.UserPlantId,
            Type = dto.Type,
            IntervalDays = dto.IntervalDays,
            NextDueAt = dto.NextDueAt ?? DateTime.UtcNow.AddDays(dto.IntervalDays),
            Enabled = true
        };

        _db.Reminders.Add(r);
        await _db.SaveChangesAsync();
        return r;
    }

    public async Task<bool> UpdateAsync(int id, UpdateReminderDto dto, string ownerId)
    {
        var r = await _db.Reminders
            .Include(x => x.UserPlant)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserPlant!.OwnerId == ownerId);

        if (r == null) return false;

        r.IntervalDays = dto.IntervalDays;
        r.Enabled = dto.Enabled;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkDoneAsync(int id, string ownerId)
    {
        var r = await _db.Reminders
            .Include(x => x.UserPlant)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserPlant!.OwnerId == ownerId);

        if (r == null) return false;

        r.LastDoneAt = DateTime.UtcNow;
        r.NextDueAt = DateTime.UtcNow.AddDays(r.IntervalDays);

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, string ownerId)
    {
        var r = await _db.Reminders
            .Include(x => x.UserPlant)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserPlant!.OwnerId == ownerId);

        if (r == null) return false;

        _db.Reminders.Remove(r);
        await _db.SaveChangesAsync();
        return true;
    }
}
