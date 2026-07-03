using PlantCare.Api.Models;

namespace PlantCare.Api.Services.Interfaces;

public enum CreateUserPlantResult
{
    Created,
    PlantNotFound,
    AlreadyExists
}

public interface IUserPlantsService
{
    /// <summary>
    /// Получить все растения из личной коллекции пользователя.
    /// </summary>
    Task<IReadOnlyCollection<UserPlant>> GetUserPlantsAsync(string ownerId);

    /// <summary>
    /// Добавить растение в коллекцию.
    /// </summary>
    Task<(CreateUserPlantResult Result, UserPlant? UserPlant)> AddUserPlantAsync(
        string ownerId, 
        int plantId, 
        string? note, 
        DateTime? nextWateringDate, 
        DateTime? nextRepottingDate);

    /// <summary>
    /// Обновить параметры растения в коллекции.
    /// </summary>
    Task<UserPlant?> UpdateUserPlantAsync(
        string ownerId, 
        int id, 
        string? note, 
        DateTime? nextWateringDate, 
        DateTime? nextRepottingDate);

    /// <summary>
    /// Удалить растение из коллекции.
    /// </summary>
    Task<bool> DeleteUserPlantAsync(string ownerId, int id);
}
