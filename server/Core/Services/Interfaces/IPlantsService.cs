using PlantCare.Api.Dtos;

namespace PlantCare.Api.Services.Interfaces;

public interface IPlantsService
{
    /// <summary>
    /// Получить список растений из справочника с фильтрацией по имени/описанию и ядовитости.
    /// </summary>
    Task<IReadOnlyCollection<PlantListItemDto>> GetPlantsAsync(string? search, bool? isPoisonous);

    /// <summary>
    /// Получить полную карточку растения из справочника по его ID.
    /// </summary>
    Task<PlantDto?> GetPlantByIdAsync(int id);
}
