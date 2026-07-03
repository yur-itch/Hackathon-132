using System.Text.Json.Serialization;

namespace PlantCare.Api.Models;

/// <summary>
/// Предложение обмена растениями между пользователями.
/// </summary>
public class ExchangeOffer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // ID пользователя-владельца предложения (создателя объявления)
    public string OwnerId { get; set; } = "";

    // Название предложения (например, "Обменяю черенок Монстеры")
    public string Title { get; set; } = "";

    // Описание предложения, состояние растения и т.д.
    public string Description { get; set; } = "";

    // Что пользователь хочет получить взамен (предпочтения)
    public string WantedPlantDescription { get; set; } = "";

    // Ссылка на конкретное растение из личной коллекции (может быть null, если растение не добавлено в систему)
    public Guid? UserPlantId { get; set; }
    public UserPlant? UserPlant { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Статус объявления: true - активно, false - закрыто
    public bool IsActive { get; set; } = true;
}
