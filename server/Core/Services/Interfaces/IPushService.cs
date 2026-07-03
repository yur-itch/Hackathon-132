namespace PlantCare.Api.Services.Interfaces;

public interface IPushService
{
    string GetPublicKey();

    Task SubscribeAsync(string ownerId, string endpoint, string p256dh, string auth);

    Task UnsubscribeAsync(string ownerId, string endpoint);

    /// <summary>
    /// Отправить уведомление всем подпискам владельца. Невалидные (410/404) подписки удаляются.
    /// </summary>
    Task NotifyOwnerAsync(string ownerId, string title, string body, CancellationToken ct = default);
}
