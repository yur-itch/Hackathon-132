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
                GbifId = "2770610", // GBIF usageKey Sansevieria trifasciata (api.gbif.org/v1/species/match)
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
                GbifId = "5361899", // GBIF usageKey Ficus lyrata (api.gbif.org/v1/species/match)
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
                GbifId = "2804680", // GBIF usageKey Phalaenopsis (api.gbif.org/v1/species/match)
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
                GbifId = "5362052", // GBIF usageKey Echeveria (api.gbif.org/v1/species/match)
                ImageUrl = "https://images.unsplash.com/photo-1509423350716-97f9360b4e09"
            },
            new Plant
            {
                Name = "Аглаонема",
                LatinName = "Aglaonema nitidum",
                Description = "Теневыносливое растение с плотными пятнистыми листьями.",
                Light = "Полутень, переносит недостаток света",
                WateringFrequencyDays = 7,
                RepottingFrequencyMonths = 24,
                Humidity = "Средняя-высокая",
                Temperature = "18–26 °C",
                Toxicity = "Токсична для кошек и собак",
                Difficulty = "easy",
                GbifId = "2866438" // сверено с ответом Pl@ntNet
            },
            new Plant
            {
                Name = "Адиантум (Венерин волос)",
                LatinName = "Adiantum raddianum",
                Description = "Изящный папоротник с тонкими ажурными листьями, требователен к влажности.",
                Light = "Рассеянный свет, без прямого солнца",
                WateringFrequencyDays = 4,
                RepottingFrequencyMonths = 12,
                Humidity = "Высокая",
                Temperature = "18–24 °C",
                Toxicity = "Нетоксичен",
                Difficulty = "hard",
                GbifId = "2651826" // сверено с ответом Pl@ntNet
            },
            new Plant
            {
                Name = "Пилея Мунвэлли",
                LatinName = "Pilea mollis",
                Description = "Компактное растение с рельефными тёмно-зелёными листьями.",
                Light = "Яркий рассеянный свет",
                WateringFrequencyDays = 5,
                RepottingFrequencyMonths = 12,
                Humidity = "Средняя",
                Temperature = "18–24 °C",
                Toxicity = "Нетоксична",
                Difficulty = "medium",
                GbifId = "5670312" // сверено с ответом Pl@ntNet
            },
            new Plant
            {
                Name = "Антуриум Андре",
                LatinName = "Anthurium andraeanum",
                Description = "Цветущее растение с глянцевыми листьями и яркими покрывалами соцветий.",
                Light = "Яркий рассеянный свет",
                WateringFrequencyDays = 7,
                RepottingFrequencyMonths = 24,
                Humidity = "Высокая",
                Temperature = "20–28 °C",
                Toxicity = "Токсичен для животных и людей",
                Difficulty = "medium",
                GbifId = "2873057" // сверено с ответом Pl@ntNet
            },
            new Plant
            {
                Name = "Спатифиллум",
                LatinName = "Spathiphyllum floribundum",
                Description = "Цветущее комнатное растение с белыми покрывалами соцветий.",
                Light = "Полутень, рассеянный свет",
                WateringFrequencyDays = 5,
                RepottingFrequencyMonths = 18,
                Humidity = "Высокая",
                Temperature = "18–26 °C",
                Toxicity = "Ядовит при попадании внутрь",
                Difficulty = "easy",
                GbifId = "2869650" // сверено с ответом Pl@ntNet
            },
            new Plant
            {
                Name = "Диффенбахия",
                LatinName = "Dieffenbachia seguine",
                Description = "Крупное декоративно-лиственное растение с пятнистыми листьями.",
                Light = "Яркий рассеянный свет",
                WateringFrequencyDays = 7,
                RepottingFrequencyMonths = 24,
                Humidity = "Средняя-высокая",
                Temperature = "18–26 °C",
                Toxicity = "Сильно токсична, сок опасен для кожи и слизистых",
                Difficulty = "medium",
                GbifId = "2869345" // сверено с ответом Pl@ntNet
            },
            new Plant
            {
                Name = "Ктенанте Бурле-Маркса",
                LatinName = "Ctenanthe burle-marxii",
                Description = "Растение с полосатыми листьями, складывающимися на ночь.",
                Light = "Полутень, рассеянный свет",
                WateringFrequencyDays = 5,
                RepottingFrequencyMonths = 18,
                Humidity = "Высокая",
                Temperature = "18–24 °C",
                Toxicity = "Нетоксична",
                Difficulty = "medium",
                GbifId = "2762406" // сверено с ответом Pl@ntNet
            },
            new Plant
            {
                Name = "Драцена окаймлённая",
                LatinName = "Dracaena marginata",
                Description = "Неприхотливое деревце с узкими листьями на тонком стволе.",
                Light = "Яркий рассеянный свет, переносит полутень",
                WateringFrequencyDays = 10,
                RepottingFrequencyMonths = 24,
                Humidity = "Средняя",
                Temperature = "18–25 °C",
                Toxicity = "Токсична для кошек и собак",
                Difficulty = "easy",
                GbifId = "10818704" // GBIF usageKey Dracaena marginata (api.gbif.org/v1/species/match)
            },
            new Plant
            {
                Name = "Хлорофитум хохлатый",
                LatinName = "Chlorophytum comosum",
                Description = "Неприхотливое растение с длинными полосатыми листьями, легко размножается.",
                Light = "От полутени до яркого рассеянного света",
                WateringFrequencyDays = 7,
                RepottingFrequencyMonths = 18,
                Humidity = "Средняя",
                Temperature = "15–25 °C",
                Toxicity = "Нетоксичен",
                Difficulty = "easy",
                GbifId = "2774846" // GBIF usageKey Chlorophytum comosum (api.gbif.org/v1/species/match)
            },
            new Plant
            {
                Name = "Пеперомия туполистная",
                LatinName = "Peperomia obtusifolia",
                Description = "Компактное растение с плотными мясистыми листьями.",
                Light = "Яркий рассеянный свет, переносит полутень",
                WateringFrequencyDays = 10,
                RepottingFrequencyMonths = 24,
                Humidity = "Средняя",
                Temperature = "18–24 °C",
                Toxicity = "Нетоксична",
                Difficulty = "easy",
                GbifId = "3086423" // GBIF usageKey Peperomia obtusifolia (api.gbif.org/v1/species/match)
            }
        );

        db.SaveChanges();
    }
}
