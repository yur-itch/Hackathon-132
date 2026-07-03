using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Dtos;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Services.Implementations;

public class PlantsService : IPlantsService
{
    private readonly AppDbContext _db;

    public PlantsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<PlantListItemDto>> GetPlantsAsync(string? search, bool? isPoisonous)
    {
        var q = _db.Plants.AsNoTracking();

        if (isPoisonous.HasValue)
        {
            if (isPoisonous.Value)
            {
                // Ядовитые (поле Toxicity заполнено и содержит информацию о вреде)
                q = q.Where(p => p.Toxicity != null && p.Toxicity != "" && !p.Toxicity.ToLower().Contains("non-toxic") && !p.Toxicity.ToLower().Contains("not toxic"));
            }
            else
            {
                // Неядовитые
                q = q.Where(p => p.Toxicity == null || p.Toxicity == "" || p.Toxicity.ToLower().Contains("non-toxic") || p.Toxicity.ToLower().Contains("not toxic"));
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLowerInvariant();
            q = q.Where(p => p.Name.ToLower().Contains(searchLower) ||
                             (p.LatinName != null && p.LatinName.ToLower().Contains(searchLower)) ||
                             (p.Description != null && p.Description.ToLower().Contains(searchLower)));
        }

        var list = await q.ToListAsync();

        return list.Select(p => new PlantListItemDto(
            Id: p.Id,
            Name: p.Name,
            Description: p.Description,
            WateringRecommendations: $"Полив каждые {p.WateringFrequencyDays} дней",
            LightingRecommendations: p.Light,
            IsPoisonous: CheckIsPoisonous(p.Toxicity),
            ImageUrl: p.ImageUrl
        )).ToList();
    }

    public async Task<PlantDto?> GetPlantByIdAsync(int id)
    {
        var p = await _db.Plants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return null;

        return new PlantDto(
            Id: p.Id,
            Name: p.Name,
            Description: p.Description,
            WateringRecommendations: $"Рекомендуемый интервал полива: раз в {p.WateringFrequencyDays} дней.",
            LightingRecommendations: p.Light,
            RepottingInfo: p.RepottingFrequencyMonths.HasValue 
                ? $"Рекомендуется пересадка каждые {p.RepottingFrequencyMonths.Value} месяцев." 
                : "Специфических требований к частоте пересадки нет.",
            IsPoisonous: CheckIsPoisonous(p.Toxicity),
            ToxicityInfo: p.Toxicity,
            CareFeatures: $"Уровень сложности ухода: {TranslateDifficulty(p.Difficulty)}. Влажность: {p.Humidity ?? "обычная"}. Температура: {p.Temperature ?? "комнатная"}.",
            ImageUrl: p.ImageUrl
        );
    }

    private static bool CheckIsPoisonous(string? toxicity)
    {
        if (string.IsNullOrWhiteSpace(toxicity)) return false;
        var lower = toxicity.ToLowerInvariant();
        return !lower.Contains("non-toxic") && !lower.Contains("not toxic") && !lower.Contains("safe");
    }

    private static string TranslateDifficulty(string difficulty)
    {
        return difficulty.ToLowerInvariant() switch
        {
            "easy" => "Простой",
            "medium" => "Средний",
            "hard" => "Сложный",
            _ => difficulty
        };
    }
}
