namespace PlantCare.Api.Models;

/// <summary>
/// Сообщение в чате обсуждения обмена.
/// </summary>
public class ChatMessage
{
    public int Id { get; set; }

    // ID предложения обмена, по которому ведется переписка
    public int ExchangeOfferId { get; set; }
    public ExchangeOffer? ExchangeOffer { get; set; }

    // ID отправителя сообщения
    public string SenderId { get; set; } = "";

    // ID получателя сообщения
    public string ReceiverId { get; set; } = "";

    // Текст сообщения
    public string Text { get; set; } = "";

    // Время отправки сообщения
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Флаг прочтения
    public bool IsRead { get; set; } = false;
}
