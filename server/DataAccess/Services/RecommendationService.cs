using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Services.Implementations;

public class RecommendationService : IRecommendationService
{
    private readonly AppDbContext _db;

    public RecommendationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Plant>> GetRecommendationsAsync(string ownerId, int count = 3)
    {
        // Получаем ID растений, которые уже есть у пользователя, и дальше
        // работаем так же, как для гостевой коллекции
        var userPlantIds = await _db.UserPlants
            .Where(up => up.OwnerId == ownerId && up.PlantId.HasValue)
            .Select(up => up.PlantId!.Value)
            .ToListAsync();

        return await GetRecommendationsForCollectionAsync(userPlantIds, count);
    }

    public async Task<IEnumerable<Plant>> GetRecommendationsForCollectionAsync(
        IReadOnlyCollection<int> plantIds, int count = 3)
    {
        var userPlantIds = plantIds.ToList();

        // 2. Получаем растения коллекции из справочника, чтобы проанализировать предпочтения
        var userPlants = await _db.Plants
            .Where(p => userPlantIds.Contains(p.Id))
            .ToListAsync();

        // 3. Если коллекция пуста, рекомендуем самые легкие растения из каталога, которых нет в коллекции
        if (!userPlants.Any())
        {
            return await _db.Plants
                .Where(p => !userPlantIds.Contains(p.Id))
                .OrderBy(p => p.Difficulty == "easy" ? 0 : p.Difficulty == "medium" ? 1 : 2)
                .ThenBy(p => p.Name)
                .Take(count)
                .ToListAsync();
        }

        // 4. Анализируем предпочтения:
        // - Самая частая сложность (easy/medium/hard)
        var preferredDifficulty = userPlants
            .GroupBy(p => p.Difficulty)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? "easy";

        // - Средняя частота полива
        var averageWateringDays = userPlants.Average(p => p.WateringFrequencyDays);

        // 5. Выбираем кандидатов из каталога (исключая уже добавленные)
        var candidates = await _db.Plants
            .Where(p => !userPlantIds.Contains(p.Id))
            .ToListAsync();

        // 6. Ранжируем кандидатов по близости к предпочтениям
        var recommendations = candidates
            .Select(p => new
            {
                Plant = p,
                // Штраф за несовпадение сложности
                DifficultyScore = p.Difficulty == preferredDifficulty ? 0 : 10,
                // Штраф за разницу в поливе
                WateringScore = Math.Abs(p.WateringFrequencyDays - averageWateringDays)
            })
            .OrderBy(c => c.DifficultyScore)
            .ThenBy(c => c.WateringScore)
            .Select(c => c.Plant)
            .Take(count);

        return recommendations;
    }
}
