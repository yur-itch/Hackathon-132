using PlantCare.Api.Models;

namespace PlantCare.Api.Dtos;

public record CreateReminderDto(
    int UserPlantId,
    ReminderType Type,
    int IntervalDays,
    DateTime? NextDueAt);

public record UpdateReminderDto(
    int IntervalDays,
    bool Enabled);
