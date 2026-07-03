using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Dtos;
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

    public async Task<IReadOnlyCollection<UserPlantDto>> GetUserPlantsAsync(string ownerId)
    {
        var plants = await _db.UserPlants
            .Include(up => up.Plant)
            .Include(up => up.Reminders)
            .Where(up => up.OwnerId == ownerId)
            .OrderByDescending(up => up.AddedAt)
            .ToListAsync();

        return plants.Select(MapToDto).ToList();
    }

    public async Task<AddUserPlantResult> AddUserPlantAsync(string ownerId, CreateUserPlantDto dto)
    {
        // 1. Проверяем существование растения в справочнике
        var catalogPlant = await _db.Plants.FindAsync(dto.PlantId);
        if (catalogPlant == null)
        {
            return new AddUserPlantResult(CreateUserPlantResult.PlantNotFound, null);
        }

        // 2. Проверяем, не добавлено ли оно уже (если мы хотим уникальность по справочнику)
        var exists = await _db.UserPlants.AnyAsync(up => up.OwnerId == ownerId && up.PlantId == dto.PlantId);
        if (exists)
        {
            return new AddUserPlantResult(CreateUserPlantResult.AlreadyExists, null);
        }

        // 3. Создаем UserPlant
        var userPlant = new UserPlant
        {
            OwnerId = ownerId,
            PlantId = dto.PlantId,
            Nickname = catalogPlant.Name,
            Notes = dto.Note,
            AddedAt = DateTime.UtcNow
        };

        _db.UserPlants.Add(userPlant);
        await _db.SaveChangesAsync(); // Сохраняем, чтобы получить Id растения в коллекции

        // 4. Создаем напоминания
        // Напоминание по поливу
        int wateringInterval = catalogPlant.WateringFrequencyDays > 0 ? catalogPlant.WateringFrequencyDays : 7;
        var wateringReminder = new Reminder
        {
            UserPlantId = userPlant.Id,
            Type = ReminderType.Watering,
            IntervalDays = wateringInterval,
            NextDueAt = dto.NextWateringDate ?? DateTime.UtcNow.AddDays(wateringInterval),
            Enabled = true
        };
        _db.Reminders.Add(wateringReminder);

        // Напоминание по пересадке
        if (catalogPlant.RepottingFrequencyMonths.HasValue && catalogPlant.RepottingFrequencyMonths.Value > 0)
        {
            int repottingIntervalDays = catalogPlant.RepottingFrequencyMonths.Value * 30;
            var repottingReminder = new Reminder
            {
                UserPlantId = userPlant.Id,
                Type = ReminderType.Repotting,
                IntervalDays = repottingIntervalDays,
                NextDueAt = dto.NextRepottingDate ?? DateTime.UtcNow.AddMonths(catalogPlant.RepottingFrequencyMonths.Value),
                Enabled = true
            };
            _db.Reminders.Add(repottingReminder);
        }
        else if (dto.NextRepottingDate.HasValue)
        {
            var repottingReminder = new Reminder
            {
                UserPlantId = userPlant.Id,
                Type = ReminderType.Repotting,
                IntervalDays = 365, // Год по умолчанию
                NextDueAt = dto.NextRepottingDate.Value,
                Enabled = true
            };
            _db.Reminders.Add(repottingReminder);
        }

        await _db.SaveChangesAsync();

        // Загружаем связанные сущности для корректного маппинга
        userPlant.Plant = catalogPlant;
        userPlant.Reminders = await _db.Reminders.Where(r => r.UserPlantId == userPlant.Id).ToListAsync();

        return new AddUserPlantResult(CreateUserPlantResult.Created, MapToDto(userPlant));
    }

    public async Task<UserPlantDto?> UpdateUserPlantAsync(string ownerId, int id, UpdateUserPlantDto dto)
    {
        var userPlant = await _db.UserPlants
            .Include(up => up.Plant)
            .Include(up => up.Reminders)
            .FirstOrDefaultAsync(up => up.Id == id && up.OwnerId == ownerId);

        if (userPlant == null) return null;

        userPlant.Notes = dto.Note;

        // Обновляем напоминание о поливе
        var wateringReminder = userPlant.Reminders.FirstOrDefault(r => r.Type == ReminderType.Watering);
        if (dto.NextWateringDate.HasValue)
        {
            if (wateringReminder != null)
            {
                wateringReminder.NextDueAt = dto.NextWateringDate.Value;
                wateringReminder.Enabled = true;
            }
            else
            {
                int interval = userPlant.Plant?.WateringFrequencyDays ?? 7;
                _db.Reminders.Add(new Reminder
                {
                    UserPlantId = userPlant.Id,
                    Type = ReminderType.Watering,
                    IntervalDays = interval,
                    NextDueAt = dto.NextWateringDate.Value,
                    Enabled = true
                });
            }
        }

        // Обновляем напоминание о пересадке
        var repottingReminder = userPlant.Reminders.FirstOrDefault(r => r.Type == ReminderType.Repotting);
        if (dto.NextRepottingDate.HasValue)
        {
            if (repottingReminder != null)
            {
                repottingReminder.NextDueAt = dto.NextRepottingDate.Value;
                repottingReminder.Enabled = true;
            }
            else
            {
                int interval = (userPlant.Plant?.RepottingFrequencyMonths ?? 12) * 30;
                _db.Reminders.Add(new Reminder
                {
                    UserPlantId = userPlant.Id,
                    Type = ReminderType.Repotting,
                    IntervalDays = interval,
                    NextDueAt = dto.NextRepottingDate.Value,
                    Enabled = true
                });
            }
        }

        await _db.SaveChangesAsync();
        return MapToDto(userPlant);
    }

    public async Task<bool> DeleteUserPlantAsync(string ownerId, int id)
    {
        var userPlant = await _db.UserPlants
            .FirstOrDefaultAsync(up => up.Id == id && up.OwnerId == ownerId);

        if (userPlant == null) return false;

        _db.UserPlants.Remove(userPlant);
        await _db.SaveChangesAsync();
        return true;
    }

    private UserPlantDto MapToDto(UserPlant up)
    {
        var wateringReminder = up.Reminders.FirstOrDefault(r => r.Type == ReminderType.Watering && r.Enabled);
        var repottingReminder = up.Reminders.FirstOrDefault(r => r.Type == ReminderType.Repotting && r.Enabled);

        return new UserPlantDto(
            Id: up.Id,
            PlantId: up.PlantId ?? 0,
            PlantName: up.Plant?.Name ?? up.Nickname ?? "Растение",
            PlantImageUrl: up.Plant?.ImageUrl,
            Note: up.Notes,
            NextWateringDate: wateringReminder?.NextDueAt,
            NextRepottingDate: repottingReminder?.NextDueAt
        );
    }
}
