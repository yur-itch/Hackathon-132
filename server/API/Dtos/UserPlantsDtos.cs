using System.ComponentModel.DataAnnotations;

namespace PlantCare.Api.Dtos;

public sealed class CreateUserPlantDto
{
    [Range(1, int.MaxValue)]
    public int PlantId { get; init; }

    [MaxLength(1000)]
    public string? Note { get; init; }

    public DateTime? NextWateringDate { get; init; }

    public DateTime? NextRepottingDate { get; init; }
}

public sealed class UpdateUserPlantDto
{
    [MaxLength(1000)]
    public string? Note { get; init; }

    public DateTime? NextWateringDate { get; init; }

    public DateTime? NextRepottingDate { get; init; }
}

public sealed record UserPlantDto(
    int Id,
    int PlantId,
    string PlantName,
    string? PlantImageUrl,
    string? Note,
    DateTime? NextWateringDate,
    DateTime? NextRepottingDate);

