namespace PlantCare.Api.Models;

/// <summary>
/// Подписка браузера на Web Push (Service Worker PushManager.subscribe()).
/// Привязана к OwnerId — тому же владельцу коллекции, что и UserPlants/Reminders.
/// </summary>
public class PushSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string OwnerId { get; set; } = "local";

    public string Endpoint { get; set; } = "";
    public string P256dh { get; set; } = "";
    public string Auth { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
