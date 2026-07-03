using PlantCare.Api.Models;

namespace PlantCare.Api.Services.Recognition.Models;

public enum MatchStatus
{
    Matched,              // вид определён и есть карточка в справочнике
    RecognizedButNoCard,  // вид определён, но карточки нет (самый частый штатный исход)
    LowConfidence,        // не удалось уверенно распознать (не растение / размытьё)
    Failed                // реальный сбой сервиса (таймаут, лимит, плохой ключ)
}

public class RecognitionResult
{
    public MatchStatus Status { get; set; }

    /// <summary>Топ-имя для отображения (даже когда карточки нет).</summary>
    public string? RecognizedLatinName { get; set; }
    public string? RecognizedCommonName { get; set; }
    public double? TopScore { get; set; }

    /// <summary>Заполнено только при Status = Matched.</summary>
    public Plant? MatchedCard { get; set; }

    /// <summary>Топ-кандидаты (для показа альтернатив / отладки).</summary>
    public List<CandidateDto> Candidates { get; set; } = new();

    /// <summary>Человекочитаемое сообщение для пользователя.</summary>
    public string? Message { get; set; }
}

public class CandidateDto
{
    public string? LatinName { get; set; }
    public string? CommonName { get; set; }
    public double Score { get; set; }
    public string? GbifId { get; set; }
}
