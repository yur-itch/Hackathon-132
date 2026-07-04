using PlantCare.Api.Models;

namespace PlantCare.Api.Services.Interfaces;

public enum CreateOfferResult
{
    Created,
    OfferedPlantNotFound, // растение не найдено или не принадлежит пользователю
    WantedPlantNotFound   // желаемого вида нет в каталоге
}

public enum ConfirmExchangeResult
{
    Confirmed,
    OfferNotFound,        // объявление не найдено/закрыто/не принадлежит пользователю
    OfferedPlantMissing,  // владелец уже не владеет отдаваемым растением
    WantedPlantMissing    // у собеседника больше нет нужного растения
}

public interface IExchangeService
{
    /// <summary>
    /// Создать новое предложение обмена (двустороннее): отдаём своё растение
    /// (userPlantId), хотим получить растение из каталога (wantedPlantId).
    /// </summary>
    Task<(CreateOfferResult Result, ExchangeOffer? Offer)> CreateOfferAsync(
        string ownerId,
        string title,
        string? description,
        int wantedPlantId,
        Guid userPlantId);

    /// <summary>
    /// Может ли пользователь зайти в чат по объявлению: владелец — всегда,
    /// остальные — только если у них в коллекции есть желаемое растение.
    /// </summary>
    Task<bool> CanUserAccessChatAsync(Guid exchangeOfferId, string userId);

    /// <summary>
    /// Получить список всех активных предложений обмена.
    /// </summary>
    Task<IEnumerable<ExchangeOffer>> GetActiveOffersAsync();

    /// <summary>
    /// Получить подробную информацию об объявлении по его ID.
    /// </summary>
    Task<ExchangeOffer?> GetOfferByIdAsync(Guid id);

    /// <summary>
    /// Закрыть объявление обмена (сделать неактивным).
    /// </summary>
    Task<bool> CloseOfferAsync(Guid id, string ownerId);

    /// <summary>
    /// Отправить сообщение в чат по конкретному обмену.
    /// </summary>
    Task<ChatMessage?> SendMessageAsync(
        string senderId, 
        Guid exchangeOfferId, 
        string receiverId, 
        string text);

    /// <summary>
    /// Получить историю переписки между текущим пользователем и другим участником по конкретному объявлению обмена.
    /// </summary>
    Task<IEnumerable<ChatMessage>> GetChatMessagesAsync(
        Guid exchangeOfferId, 
        string otherUserId, 
        string currentUserId);

    /// <summary>
    /// Получить список активных диалогов (чатов) текущего пользователя.
    /// Возвращает кортеж: (Предложение обмена, ID собеседника, Отображаемое имя собеседника, Последнее сообщение).
    /// </summary>
    Task<IEnumerable<(ExchangeOffer Offer, string OtherUserId, string OtherUserDisplayName, ChatMessage LastMessage)>> GetMyChatsAsync(string currentUserId);

    /// <summary>
    /// Подтвердить двусторонний обмен: растение владельца уходит собеседнику, а
    /// подходящее растение собеседника (совпадающее с желаемым видом) — владельцу.
    /// Напоминания пересоздаются для обоих новых хозяев, объявление закрывается.
    /// </summary>
    Task<ConfirmExchangeResult> ConfirmExchangeAsync(Guid exchangeOfferId, string currentUserId, string otherUserId);
}
