using System.Text.Json.Serialization;

namespace PlantCare.Api.Models;

public enum ReminderType
{
    Watering,
    Repotting,
    Fertilizing
}

/// <summary>
/// Напоминание по конкретному растению из личной коллекции.
/// </summary>
public class Reminder
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserPlantId { get; set; }
    [JsonIgnore]
    public UserPlant? UserPlant { get; set; }

    public ReminderType Type { get; set; }
    public int IntervalDays { get; set; }
    public DateTime NextDueAt { get; set; }
    public DateTime? LastDoneAt { get; set; }
    public bool Enabled { get; set; } = true;
}
