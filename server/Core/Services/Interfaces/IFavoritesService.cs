using PlantCare.Api.Models;

namespace PlantCare.Api.Services.Interfaces;

public interface IFavoritesService
{
    /// <summary>
    /// Получить все избранные растения пользователя.
    /// </summary>
    Task<IEnumerable<Favorite>> GetFavoritesAsync(string ownerId);

    /// <summary>
    /// Добавить растение в избранное.
    /// </summary>
    /// <returns>True, если успешно добавлено; False, если растение не найдено или уже в избранном.</returns>
    Task<bool> AddToFavoritesAsync(string ownerId, int plantId);

    /// <summary>
    /// Удалить растение из избранного.
    /// </summary>
    /// <returns>True, если успешно удалено; False, если не найдено в избранном.</returns>
    Task<bool> RemoveFromFavoritesAsync(string ownerId, int plantId);

    /// <summary>
    /// Проверить, находится ли растение в избранном у пользователя.
    /// </summary>
    Task<bool> IsFavoriteAsync(string ownerId, int plantId);
}
