using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
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

    // Npgsql пишет только Kind=Utc в колонки timestamptz, а из JSON даты приходят
    // как Kind=Unspecified — без этого падает с ArgumentException при сохранении.
    private static DateTime AsUtc(DateTime dt)
        => dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    public async Task<Reminder?> CreateAsync(
        string ownerId,
        Guid userPlantId,
        ReminderType type,
        int intervalDays,
        DateTime? nextDueAt)
    {
        var owns = await _db.UserPlants.AnyAsync(x => x.Id == userPlantId && x.OwnerId == ownerId);
        if (!owns) return null;

        var r = new Reminder
        {
            Id = Guid.NewGuid(),
            UserPlantId = userPlantId,
            Type = type,
            IntervalDays = intervalDays,
            NextDueAt = nextDueAt.HasValue ? AsUtc(nextDueAt.Value) : DateTime.UtcNow.AddDays(intervalDays),
            Enabled = true
        };

        _db.Reminders.Add(r);
        await _db.SaveChangesAsync();
        return r;
    }

    public async Task<bool> UpdateAsync(
        string ownerId, 
        Guid id, 
        int intervalDays, 
        bool enabled)
    {
        var r = await _db.Reminders
            .Include(x => x.UserPlant)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserPlant!.OwnerId == ownerId);

        if (r == null) return false;

        r.IntervalDays = intervalDays;
        r.Enabled = enabled;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkDoneAsync(Guid id, string ownerId)
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

    public async Task<bool> DeleteAsync(Guid id, string ownerId)
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
