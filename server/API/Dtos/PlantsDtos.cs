namespace PlantCare.Api.Dtos;

public sealed record PlantListItemDto(
    int Id,
    string Name,
    string? Description,
    string WateringRecommendations,
    string LightingRecommendations,
    bool IsPoisonous,
    string? ImageUrl);

public sealed record PlantDto(
    int Id,
    string Name,
    string? Description,
    string WateringRecommendations,
    string LightingRecommendations,
    string? RepottingInfo,
    bool IsPoisonous,
    string? ToxicityInfo,
    string? CareFeatures,
    string? ImageUrl);
