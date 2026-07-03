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

        db.Plants.AddRange(
            new Plant
            {
                Name = "Монстера",
                LatinName = "Monstera deliciosa",
                Description = "Крупное тропическое растение с резными листьями.",
                Light = "Яркий рассеянный свет",
                WateringFrequencyDays = 7,
                RepottingFrequencyMonths = 24,
                Humidity = "Средняя-высокая",
                Temperature = "18–27 °C",
                Toxicity = "Токсична для кошек и собак",
                Difficulty = "easy",
                GbifId = "2868241", // реальный GBIF id Monstera deliciosa (сверено с ответом Pl@ntNet)
                ImageUrl = "https://images.unsplash.com/photo-1614594975525-e45190c55d0b"
            },
            new Plant
            {
                Name = "Сансевиерия",
                LatinName = "Sansevieria trifasciata",
                Description = "Неприхотливое растение, «тёщин язык».",
                Light = "От тени до яркого света",
                WateringFrequencyDays = 14,
                RepottingFrequencyMonths = 36,
                Humidity = "Низкая",
                Temperature = "15–29 °C",
                Toxicity = "Слаботоксична",
                Difficulty = "easy",
                ImageUrl = "https://images.unsplash.com/photo-1593482892290-f54927ae2b7c"
            },
            new Plant
            {
                Name = "Фикус лировидный",
                LatinName = "Ficus lyrata",
                Description = "Эффектное дерево с крупными листьями-скрипками.",
                Light = "Яркий рассеянный свет",
                WateringFrequencyDays = 7,
                RepottingFrequencyMonths = 18,
                Humidity = "Средняя",
                Temperature = "16–24 °C",
                Toxicity = "Токсичен для животных",
                Difficulty = "hard",
                ImageUrl = "https://images.unsplash.com/photo-1597055181300-e3633a917b68"
            },
            new Plant
            {
                Name = "Орхидея Фаленопсис",
                LatinName = "Phalaenopsis",
                Description = "Популярная комнатная орхидея с долгим цветением.",
                Light = "Яркий рассеянный свет, без прямого солнца",
                WateringFrequencyDays = 7,
                RepottingFrequencyMonths = 24,
                Humidity = "Высокая",
                Temperature = "18–25 °C",
                Toxicity = "Нетоксична",
                Difficulty = "medium",
                ImageUrl = "https://images.unsplash.com/photo-1524598171365-b45f6f8f8c0c"
            },
            new Plant
            {
                Name = "Суккулент Эхеверия",
                LatinName = "Echeveria",
                Description = "Розеточный суккулент, любит солнце и редкий полив.",
                Light = "Прямой солнечный свет",
                WateringFrequencyDays = 14,
                RepottingFrequencyMonths = 24,
                Humidity = "Низкая",
                Temperature = "18–27 °C",
                Toxicity = "Нетоксична",
                Difficulty = "easy",
                ImageUrl = "https://images.unsplash.com/photo-1509423350716-97f9360b4e09"
            }
        );

        db.SaveChanges();
    }
}
