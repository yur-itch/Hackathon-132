namespace PlantCare.Api.Dtos;

public record CreateExchangeOfferDto(
    string Title,
    string Description,
    string WantedPlantDescription,
    Guid? UserPlantId);

public record ExchangeOfferDto(
    Guid Id,
    string OwnerId,
    string Title,
    string Description,
    string WantedPlantDescription,
    Guid? UserPlantId,
    string? PlantName,
    string? PlantImageUrl,
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
