using PlantCare.Api.Dtos;
using PlantCare.Api.Models;

namespace PlantCare.Api.Services.Interfaces;

public interface IExchangeService
{
    /// <summary>
    /// Создать новое предложение обмена.
    /// </summary>
    Task<ExchangeOffer> CreateOfferAsync(CreateExchangeOfferDto dto, string ownerId);

    /// <summary>
    /// Получить список всех активных предложений обмена (с возможностью текстового поиска).
    /// </summary>
    Task<IEnumerable<ExchangeOffer>> GetActiveOffersAsync(string? search);

    /// <summary>
    /// Получить подробную информацию об объявлении по его ID.
    /// </summary>
    Task<ExchangeOffer?> GetOfferByIdAsync(int id);

    /// <summary>
    /// Закрыть объявление обмена (сделать неактивным).
    /// </summary>
    Task<bool> CloseOfferAsync(int id, string ownerId);

    /// <summary>
    /// Отправить сообщение в чат по конкретному обмену.
    /// </summary>
    Task<ChatMessage?> SendMessageAsync(SendChatMessageDto dto, string senderId);

    /// <summary>
    /// Получить историю переписки между текущим пользователем и другим участником по конкретному объявлению обмена.
    /// </summary>
    Task<IEnumerable<ChatMessage>> GetChatMessagesAsync(int exchangeOfferId, string otherUserId, string currentUserId);

    /// <summary>
    /// Получить список активных диалогов (чатов) текущего пользователя.
    /// </summary>
    Task<IEnumerable<ChatDto>> GetMyChatsAsync(string currentUserId);
}
