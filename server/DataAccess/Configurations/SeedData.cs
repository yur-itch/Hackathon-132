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
        var existingPlantsByLatinName = db.Plants
            .Where(plant => plant.LatinName != null)
            .ToDictionary(plant => plant.LatinName!);

        var plants = new[]
        {
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
                CareFeatures = "Любит опору для лазания (мох-палка); воздушные корни можно направлять в почву.",
                GbifId = "2868241", // реальный GBIF id Monstera deliciosa (сверено с ответом Pl@ntNet)
                ImageUrl = "/images/plants/monstera.jpg"
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
                CareFeatures = "Крайне устойчива к засухе, легко переносит пропуски полива; не любит переувлажнение.",
                GbifId = "2770610", // GBIF usageKey Sansevieria trifasciata (api.gbif.org/v1/species/match)
                ImageUrl = "/images/plants/sansevieria.jpg"
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
                CareFeatures = "Плохо переносит перестановку и сквозняки, может сбрасывать листья при стрессе.",
                GbifId = "5361899", // GBIF usageKey Ficus lyrata (api.gbif.org/v1/species/match)
                ImageUrl = "/images/plants/ficus-lyrata.jpg"
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
                CareFeatures = "Растёт в прозрачном горшке с корой, а не в грунте; полив погружением раз в 7–10 дней.",
                GbifId = "2804680", // GBIF usageKey Phalaenopsis (api.gbif.org/v1/species/match)
                ImageUrl = "/images/plants/phalaenopsis.jpg"
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
                CareFeatures = "Не опрыскивать и не лить воду в центр розетки — застой воды вызывает гниение.",
                GbifId = "5362052", // GBIF usageKey Echeveria (api.gbif.org/v1/species/match)
                ImageUrl = "/images/plants/echeveria.jpg"
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
                CareFeatures = "Чувствительна к холодному воздуху и сквознякам, любит стабильное тепло круглый год.",
                GbifId = "2866438", // сверено с ответом Pl@ntNet
                ImageUrl = "/images/plants/aglaonema.jpg"
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
                CareFeatures = "Не переносит сухой воздух и пересушку земляного кома — концы листьев быстро сохнут.",
                GbifId = "2651826", // сверено с ответом Pl@ntNet
                ImageUrl = "/images/plants/adiantum.jpg"
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
                CareFeatures = "Хорошо ветвится при регулярной прищипке верхушек побегов.",
                GbifId = "5670312", // сверено с ответом Pl@ntNet
                ImageUrl = "/images/plants/pilea.jpg"
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
                CareFeatures = "Сок раздражает кожу — при пересадке и обрезке лучше использовать перчатки.",
                GbifId = "2873057", // сверено с ответом Pl@ntNet
                ImageUrl = "/images/plants/anthurium.jpg"
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
                CareFeatures = "Явно показывает нехватку воды поникшими листьями и быстро восстанавливается после полива.",
                GbifId = "2869650", // сверено с ответом Pl@ntNet
                ImageUrl = "/images/plants/spathiphyllum.jpg"
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
                CareFeatures = "Держать вне досягаемости детей и животных — даже небольшой контакт с соком вызывает жжение.",
                GbifId = "2869345", // сверено с ответом Pl@ntNet
                ImageUrl = "/images/plants/dieffenbachia.jpg"
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
                CareFeatures = "Листья складываются вертикально к ночи и раскрываются утром (никтинастия).",
                GbifId = "2762406", // сверено с ответом Pl@ntNet
                ImageUrl = "/images/plants/ctenanthe.jpg"
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
                CareFeatures = "Чувствительна к фтору и хлору в воде — лучше поливать отстоянной или фильтрованной водой.",
                GbifId = "10818704", // GBIF usageKey Dracaena marginata (api.gbif.org/v1/species/match)
                ImageUrl = "/images/plants/dracaena.jpg"
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
                CareFeatures = "Активно образует розетки-детки на длинных побегах, которые легко укореняются в воде.",
                GbifId = "2774846", // GBIF usageKey Chlorophytum comosum (api.gbif.org/v1/species/match)
                ImageUrl = "/images/plants/chlorophytum.jpg"
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
                CareFeatures = "Мясистые листья запасают воду — легко переносит редкий полив, но не переносит перелив.",
                GbifId = "3086423", // GBIF usageKey Peperomia obtusifolia (api.gbif.org/v1/species/match)
                ImageUrl = "/images/plants/peperomia.jpg"
            },
            new Plant
            {
                Name = "Калатея Макоя",
                LatinName = "Goeppertia makoyana",
                Description = "Декоративное растение с узорчатыми листьями, чувствительное к сухому воздуху.",
                Light = "Рассеянный свет, без прямого солнца",
                WateringFrequencyDays = 5,
                RepottingFrequencyMonths = 18,
                Humidity = "Высокая",
                Temperature = "18–26 °C",
                Toxicity = "Нетоксична",
                Difficulty = "medium",
                CareFeatures = "Любит равномерно влажный грунт и мягкую воду; при сухом воздухе края листьев быстро сохнут.",
                GbifId = "2762358",
                ImageUrl = "/images/plants/calathea-makoyana.jpg"
            }
        };

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
    }
}
