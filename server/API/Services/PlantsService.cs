using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Dtos;
using PlantCare.Api.Models;

namespace PlantCare.Api.Services;

public interface IPlantsService
{
    Task<IReadOnlyCollection<PlantListItemDto>> GetPlantsAsync(string? search, bool? isPoisonous);
    Task<PlantDto?> GetPlantByIdAsync(int id);
}

public sealed class PlantsService : IPlantsService
{
    private readonly AppDbContext _db;

    public PlantsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<PlantListItemDto>> GetPlantsAsync(string? search, bool? isPoisonous)
    {
        var query = _db.Plants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var text = search.Trim().ToLower();
            query = query.Where(plant =>
                plant.Name.ToLower().Contains(text) ||
                (plant.LatinName != null && plant.LatinName.ToLower().Contains(text)));
        }

        var plants = await query.OrderBy(plant => plant.Name).ToListAsync();

        if (isPoisonous is not null)
        {
            plants = plants.Where(plant => IsPoisonous(plant.Toxicity) == isPoisonous).ToList();
        }

        return plants.Select(ToListItemDto).ToList();
    }

    public async Task<PlantDto?> GetPlantByIdAsync(int id)
    {
        var plant = await _db.Plants.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return plant is null ? null : ToDto(plant);
    }

    private static PlantListItemDto ToListItemDto(Plant plant)
    {
        return new PlantListItemDto(
            plant.Id,
            plant.Name,
            plant.Description,
            $"раз в {plant.WateringFrequencyDays} дней",
            plant.Light,
            IsPoisonous(plant.Toxicity),
            plant.ImageUrl);
    }

    private static PlantDto ToDto(Plant plant)
    {
        return new PlantDto(
            plant.Id,
            plant.Name,
            plant.Description,
            $"раз в {plant.WateringFrequencyDays} дней",
            plant.Light,
            plant.RepottingFrequencyMonths is null
                ? null
                : $"раз в {plant.RepottingFrequencyMonths} месяцев",
            IsPoisonous(plant.Toxicity),
            plant.Toxicity,
            BuildCareFeatures(plant),
            plant.ImageUrl);
    }

    private static string? BuildCareFeatures(Plant plant)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(plant.Humidity))
        {
            parts.Add($"Влажность: {plant.Humidity}");
        }

        if (!string.IsNullOrWhiteSpace(plant.Temperature))
        {
            parts.Add($"Температура: {plant.Temperature}");
        }

        if (!string.IsNullOrWhiteSpace(plant.Difficulty))
        {
            parts.Add($"Сложность: {plant.Difficulty}");
        }

        return parts.Count == 0 ? null : string.Join("; ", parts);
    }

    private static bool IsPoisonous(string? toxicity)
    {
        if (string.IsNullOrWhiteSpace(toxicity))
        {
            return false;
        }

        return !toxicity.Contains("нетокс", StringComparison.OrdinalIgnoreCase);
    }
}
