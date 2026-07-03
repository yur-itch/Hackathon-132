using System.IO;
using System.Text.Json;
using PlantCare.Api.Models;

namespace PlantCare.Api.Data;

/// <summary>
/// Наполнение справочника стартовыми растениями. Вызывается на старте,
/// если таблица Plants пуста. Дополняйте список — это ваша демо-витрина.
/// </summary>
public static class SeedData
{
    public static void EnsureSeeded(AppDbContext db)
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plants-seed.json");
        if (!File.Exists(filePath))
        {
            filePath = "plants-seed.json";
        }

        if (File.Exists(filePath))
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var plants = JsonSerializer.Deserialize<List<Plant>>(json, options);
                if (plants != null && plants.Count > 0)
                {
                    var existingPlantsByLatinName = db.Plants
                        .Where(plant => plant.LatinName != null)
                        .ToDictionary(plant => plant.LatinName!);

                    foreach (var plant in plants)
                    {
                        if (existingPlantsByLatinName.TryGetValue(plant.LatinName ?? "", out var existingPlant))
                        {
                            existingPlant.ImageUrl = plant.ImageUrl;
                            continue;
                        }

                        db.Plants.Add(plant);
                    }

                    db.SaveChanges();
                    Console.WriteLine($"Справочник синхронизирован с {filePath}.");
                }
                else
                {
                    Console.WriteLine($"Файл {filePath} пуст или не содержит корректных данных.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при десериализации/сохранении данных: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Файл данных {filePath} не найден.");
        }
    }
}
