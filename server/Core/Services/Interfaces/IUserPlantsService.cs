using PlantCare.Api.Dtos;

namespace PlantCare.Api.Services.Interfaces;

public interface IUserPlantsService
{
    /// <summary>
    /// Получить список всех растений в коллекции пользователя в формате DTO.
    /// </summary>
    Task<IReadOnlyCollection<UserPlantDto>> GetUserPlantsAsync(string ownerId);

    /// <summary>
    /// Добавить новое растение в коллекцию.
    /// </summary>
    Task<AddUserPlantResult> AddUserPlantAsync(string ownerId, CreateUserPlantDto dto);

    /// <summary>
    /// Обновить параметры растения в коллекции.
    /// </summary>
    Task<UserPlantDto?> UpdateUserPlantAsync(string ownerId, int id, UpdateUserPlantDto dto);

    /// <summary>
    /// Удалить растение из коллекции.
    /// </summary>
    Task<bool> DeleteUserPlantAsync(string ownerId, int id);
}
