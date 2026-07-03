namespace PlantCare.Api.Models;

/// <summary>
/// Растение из справочника (reference card). Общая база на всех пользователей.
/// </summary>
public class Plant
{
    public int Id { get; set; }

    public string Name { get; set; } = "";        // «Монстера»
    public string? LatinName { get; set; }         // «Monstera deliciosa»
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    // Условия ухода
    public string Light { get; set; } = "";        // «Яркий рассеянный свет»
    public int WateringFrequencyDays { get; set; } // базовый интервал полива, дней
    public int? RepottingFrequencyMonths { get; set; }
    public string? Humidity { get; set; }
    public string? Temperature { get; set; }
    public string? Toxicity { get; set; }          // токсичность для животных/детей
    public string Difficulty { get; set; } = "easy"; // easy | medium | hard

    // GBIF taxon id — для стабильного матчинга результата распознавания по id,
    // а не по строке латинского имени. Необязательное поле.
    public string? GbifId { get; set; }
}
