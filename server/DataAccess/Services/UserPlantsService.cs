using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Services.Implementations;

public class UserPlantsService : IUserPlantsService
{
    private readonly AppDbContext _db;

    public UserPlantsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<UserPlant>> GetUserPlantsAsync(string ownerId)
    {
        return await _db.UserPlants
            .Include(up => up.Plant)
            .Include(up => up.Reminders)
            .Where(up => up.OwnerId == ownerId)
            .OrderByDescending(up => up.AddedAt)
            .ToListAsync();
    }

    // Npgsql пишет только Kind=Utc в колонки timestamptz, а из JSON даты приходят
    // как Kind=Unspecified — без этого падает с ArgumentException при сохранении.
    private static DateTime AsUtc(DateTime dt)
        => dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    public async Task<(CreateUserPlantResult Result, UserPlant? UserPlant)> AddUserPlantAsync(
        string ownerId,
        int plantId,
        string? note,
        DateTime? nextWateringDate,
        DateTime? nextRepottingDate)
    {
        var catalogPlant = await _db.Plants.FindAsync(plantId);
        if (catalogPlant == null)
        {
            return (CreateUserPlantResult.PlantNotFound, null);
        }

        var exists = await _db.UserPlants.AnyAsync(up => up.OwnerId == ownerId && up.PlantId == plantId);
        if (exists)
        {
            return (CreateUserPlantResult.AlreadyExists, null);
        }

        var userPlant = new UserPlant
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            PlantId = plantId,
            Nickname = catalogPlant.Name,
            Notes = note,
            AddedAt = DateTime.UtcNow
        };

        _db.UserPlants.Add(userPlant);
        await _db.SaveChangesAsync();

        int wateringInterval = catalogPlant.WateringFrequencyDays > 0 ? catalogPlant.WateringFrequencyDays : 7;
        var wateringReminder = new Reminder
        {
            Id = Guid.NewGuid(),
            UserPlantId = userPlant.Id,
            Type = ReminderType.Watering,
            IntervalDays = wateringInterval,
            NextDueAt = nextWateringDate.HasValue ? AsUtc(nextWateringDate.Value) : DateTime.UtcNow.AddDays(wateringInterval),
            Enabled = true
        };
        _db.Reminders.Add(wateringReminder);

        if (catalogPlant.RepottingFrequencyMonths.HasValue && catalogPlant.RepottingFrequencyMonths.Value > 0)
        {
            int repottingIntervalDays = catalogPlant.RepottingFrequencyMonths.Value * 30;
            var repottingReminder = new Reminder
            {
                Id = Guid.NewGuid(),
                UserPlantId = userPlant.Id,
                Type = ReminderType.Repotting,
                IntervalDays = repottingIntervalDays,
                NextDueAt = nextRepottingDate.HasValue ? AsUtc(nextRepottingDate.Value) : DateTime.UtcNow.AddMonths(catalogPlant.RepottingFrequencyMonths.Value),
                Enabled = true
            };
            _db.Reminders.Add(repottingReminder);
        }
        else if (nextRepottingDate.HasValue)
        {
            var repottingReminder = new Reminder
            {
                Id = Guid.NewGuid(),
                UserPlantId = userPlant.Id,
                Type = ReminderType.Repotting,
                IntervalDays = 365,
                NextDueAt = AsUtc(nextRepottingDate.Value),
                Enabled = true
            };
            _db.Reminders.Add(repottingReminder);
        }

        await _db.SaveChangesAsync();

        userPlant.Plant = catalogPlant;
        userPlant.Reminders = await _db.Reminders.Where(r => r.UserPlantId == userPlant.Id).ToListAsync();

        return (CreateUserPlantResult.Created, userPlant);
    }

    public async Task<UserPlant?> UpdateUserPlantAsync(
        string ownerId, 
        Guid id, 
        string? note, 
        DateTime? nextWateringDate, 
        DateTime? nextRepottingDate)
    {
        var userPlant = await _db.UserPlants
            .Include(up => up.Plant)
            .Include(up => up.Reminders)
            .FirstOrDefaultAsync(up => up.Id == id && up.OwnerId == ownerId);

        if (userPlant == null) return null;

        userPlant.Notes = note;

        var wateringReminder = userPlant.Reminders.FirstOrDefault(r => r.Type == ReminderType.Watering);
        if (nextWateringDate.HasValue)
        {
            if (wateringReminder != null)
            {
                wateringReminder.NextDueAt = AsUtc(nextWateringDate.Value);
                wateringReminder.Enabled = true;
            }
            else
            {
                int interval = userPlant.Plant?.WateringFrequencyDays ?? 7;
                _db.Reminders.Add(new Reminder
                {
                    Id = Guid.NewGuid(),
                    UserPlantId = userPlant.Id,
                    Type = ReminderType.Watering,
                    IntervalDays = interval,
                    NextDueAt = AsUtc(nextWateringDate.Value),
                    Enabled = true
                });
            }
        }

        var repottingReminder = userPlant.Reminders.FirstOrDefault(r => r.Type == ReminderType.Repotting);
        if (nextRepottingDate.HasValue)
        {
            if (repottingReminder != null)
            {
                repottingReminder.NextDueAt = AsUtc(nextRepottingDate.Value);
                repottingReminder.Enabled = true;
            }
            else
            {
                int interval = (userPlant.Plant?.RepottingFrequencyMonths ?? 12) * 30;
                _db.Reminders.Add(new Reminder
                {
                    Id = Guid.NewGuid(),
                    UserPlantId = userPlant.Id,
                    Type = ReminderType.Repotting,
                    IntervalDays = interval,
                    NextDueAt = AsUtc(nextRepottingDate.Value),
                    Enabled = true
                });
            }
        }

        await _db.SaveChangesAsync();
        return userPlant;
    }

    public async Task<bool> DeleteUserPlantAsync(string ownerId, Guid id)
    {
        var userPlant = await _db.UserPlants
            .FirstOrDefaultAsync(up => up.Id == id && up.OwnerId == ownerId);

        if (userPlant == null) return false;

        _db.UserPlants.Remove(userPlant);
        await _db.SaveChangesAsync();
        return true;
    }
}
