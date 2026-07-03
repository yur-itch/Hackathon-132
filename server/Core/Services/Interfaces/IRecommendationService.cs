using PlantCare.Api.Models;

namespace PlantCare.Api.Services.Interfaces;

public interface IRecommendationService
{
    /// <summary>
    /// Получить рекомендованные растения для пользователя на основе его текущей коллекции.
    /// </summary>
    /// <param name="ownerId">Идентификатор пользователя</param>
    /// <param name="count">Максимальное количество рекомендаций</param>
    Task<IEnumerable<Plant>> GetRecommendationsAsync(string ownerId, int count = 3);

    /// <summary>
    /// Рекомендации по явному списку растений из справочника — для гостевого режима,
    /// где коллекция хранится в браузере и передаётся списком id.
    /// </summary>
    Task<IEnumerable<Plant>> GetRecommendationsForCollectionAsync(
        IReadOnlyCollection<int> plantIds, int count = 3);
}
