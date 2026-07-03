namespace PlantCare.Api.Services.Recognition;

/// <summary>
/// Приводит латинские имена к сравнимому виду: "Monstera deliciosa Liebm." → "monstera deliciosa".
/// Обе стороны сравнения (кандидат из API и имя в справочнике) нормализуем одинаково.
/// </summary>
public static class LatinNameNormalizer
{
    public static string Normalize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "";

        var tokens = name.Trim().ToLowerInvariant()
            .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        // род + видовой эпитет, автор/подвид отбрасываем
        return string.Join(' ', tokens.Take(2));
    }
}
