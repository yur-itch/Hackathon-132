using System.Text.Json.Serialization;

namespace PlantCare.Api.Models;

/// <summary>
/// Растение в личной коллекции пользователя.
/// В базовой версии владелец — строка OwnerId (по умолчанию "local", без аккаунтов).
/// Усложнение «авторизация» просто подставляет реальный id пользователя.
/// </summary>
public class UserPlant
{
    public int Id { get; set; }

    public string OwnerId { get; set; } = "local";

    public int? PlantId { get; set; }              // ссылка на справочник (может быть null для «своего» растения)
    public Plant? Plant { get; set; }

    public string Nickname { get; set; } = "";     // как пользователь назвал растение
    public string? Location { get; set; }          // «Кухня, подоконник»
    public string? Notes { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public List<Reminder> Reminders { get; set; } = new();
}
