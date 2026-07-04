using PlantCare.Api.Models;

namespace PlantCare.Api.Dtos;

public record CreateReminderDto(
    Guid UserPlantId,
    ReminderType Type,
    int IntervalDays,
    DateTime? NextDueAt);

public record UpdateReminderDto(
    int IntervalDays,
    bool Enabled);

// Готовое напоминание для клиента. Имя растения кладём явно, потому что
// навигация Reminder.UserPlant помечена [JsonIgnore] и в сырой сущности его нет.
public record ReminderDto(
    Guid Id,
    Guid UserPlantId,
    ReminderType Type,
    DateTime NextDueAt,
    string PlantName);
