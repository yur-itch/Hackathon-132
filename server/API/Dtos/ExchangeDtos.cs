using System.ComponentModel.DataAnnotations;

namespace PlantCare.Api.Dtos;

public record CreateExchangeOfferDto(
    [Required(ErrorMessage = "Укажите название объявления.")] string Title,
    string? Description,
    // Растение из каталога, которое хотим получить взамен.
    [Range(1, int.MaxValue, ErrorMessage = "Выберите растение, которое хотите получить.")] int WantedPlantId,
    // Растение из своей коллекции, которое отдаём.
    [Required(ErrorMessage = "Выберите своё растение для обмена.")] Guid UserPlantId);

public record ExchangeOfferDto(
    Guid Id,
    string OwnerId,
    string Title,
    string? Description,
    // Что отдаёт владелец (из его коллекции)
    Guid? UserPlantId,
    string? OfferedPlantName,
    string? OfferedPlantImageUrl,
    // Что хочет получить (из каталога) — по нему проверяется право откликнуться
    int WantedPlantId,
    string? WantedPlantName,
    string? WantedPlantImageUrl,
    DateTime CreatedAt,
    bool IsActive);

public record SendChatMessageDto(
    string ReceiverId,
    string Text);

public record ChatMessageDto(
    Guid Id,
    Guid ExchangeOfferId,
    string SenderId,
    string ReceiverId,
    string Text,
    DateTime SentAt,
    bool IsRead);

public record ChatDto(
    Guid ExchangeOfferId,
    string ExchangeOfferTitle,
    string OtherUserId,
    string OtherUserDisplayName,
    DateTime LastMessageAt,
    string LastMessageText);

public record ConfirmExchangeDto(
    string OtherUserId);
