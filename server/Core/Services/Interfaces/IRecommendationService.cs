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
}
