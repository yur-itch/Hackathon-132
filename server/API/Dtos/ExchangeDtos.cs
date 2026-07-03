namespace PlantCare.Api.Dtos;

public record CreateExchangeOfferDto(
    string Title,
    string Description,
    string WantedPlantDescription,
    int? UserPlantId);

public record SendChatMessageDto(
    int ExchangeOfferId,
    string ReceiverId,
    string Text);

public record ChatDto(
    int ExchangeOfferId,
    string ExchangeOfferTitle,
    string OtherUserId,
    string OtherUserDisplayName,
    DateTime LastMessageAt,
    string LastMessageText);
