using PlantCare.Api.Models;

namespace PlantCare.Api.Services.Interfaces;

public interface IExchangeService
{
    /// <summary>
    /// Создать новое предложение обмена.
    /// </summary>
    Task<ExchangeOffer> CreateOfferAsync(
        string ownerId, 
        string title, 
        string description, 
        string wantedPlantDescription, 
        Guid? userPlantId);

    /// <summary>
    /// Получить список всех активных предложений обмена (с возможностью текстового поиска).
    /// </summary>
    Task<IEnumerable<ExchangeOffer>> GetActiveOffersAsync(string? search);

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
    /// Подтвердить обмен с другим пользователем: передает права владения привязанным UserPlant,
    /// пересоздает напоминания для нового хозяина и переводит объявление в статус неактивного.
    /// </summary>
    Task<bool> ConfirmExchangeAsync(Guid exchangeOfferId, string currentUserId, string otherUserId);
}
