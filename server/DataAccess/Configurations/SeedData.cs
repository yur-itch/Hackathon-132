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
        if (db.Plants.Any()) return;

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
                    db.Plants.AddRange(plants);
                    db.SaveChanges();
                    Console.WriteLine($"Успешно импортировано {plants.Count} растений из {filePath}.");
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
