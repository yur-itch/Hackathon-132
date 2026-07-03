using PlantCare.Api.Models;

namespace PlantCare.Api.Dtos;

// --- UserPlant ---
public record CreateUserPlantDto(
    int? PlantId,
    string Nickname,
    string? Location,
    string? Notes);

public record UpdateUserPlantDto(
    string Nickname,
    string? Location,
    string? Notes);

// --- Reminder ---
public record CreateReminderDto(
    int UserPlantId,
    ReminderType Type,
    int IntervalDays,
    DateTime? NextDueAt);

public record UpdateReminderDto(
    int IntervalDays,
    bool Enabled);

// --- Auth (усложнение) ---
public record RegisterDto(string Email, string Password, string DisplayName);
public record LoginDto(string Email, string Password);
