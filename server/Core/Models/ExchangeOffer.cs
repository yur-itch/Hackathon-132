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

    // Растение из каталога, которое владелец хочет получить взамен (двусторонний обмен).
    // Отвечать на объявление может только тот, у кого это растение есть в коллекции.
    public int WantedPlantId { get; set; }
    public Plant? WantedPlant { get; set; }

    // Растение владельца из его личной коллекции, которое он отдаёт.
    // На уровне БД nullable (SetNull при удалении растения), но при создании
    // объявления обязательно — сервис это проверяет.
    public Guid? UserPlantId { get; set; }
    public UserPlant? UserPlant { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Статус объявления: true - активно, false - закрыто
    public bool IsActive { get; set; } = true;
}
