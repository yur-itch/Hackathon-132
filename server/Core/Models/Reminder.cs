using System.Text.Json.Serialization;

namespace PlantCare.Api.Models;

public enum ReminderType
{
    Watering, // полив
    Repotting, // пересадка
    Fertilizing // подкормка
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

    // Когда последний раз реально отправили push по этому NextDueAt — чтобы не
    // слать одно и то же напоминание повторно на каждой проверке фонового сервиса.
    [JsonIgnore]
    public DateTime? NotifiedAt { get; set; }
}
