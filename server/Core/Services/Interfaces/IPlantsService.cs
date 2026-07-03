using PlantCare.Api.Models;

namespace PlantCare.Api.Services.Interfaces;

public interface IPlantsService
{
    /// <summary>
    /// Получить список растений из справочника с фильтрацией по ядовитости.
    /// </summary>
    Task<IReadOnlyCollection<Plant>> GetPlantsAsync(bool? isPoisonous);

    /// <summary>
    /// Получить растение из справочника по его ID.
    /// </summary>
    Task<Plant?> GetPlantByIdAsync(int id);
}
