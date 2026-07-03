using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlantCare.Api.Services.PlantNet.Models;

// DTO ответа Pl@ntNet (v2/identify). Разбираем только нужные поля.
public class PlantNetResponse
{
    public List<PlantNetResult> Results { get; set; } = new();
}

public class PlantNetResult
{
    public double Score { get; set; }
    public PlantNetSpecies Species { get; set; } = new();
    public PlantNetRef? Gbif { get; set; }
    public PlantNetRef? Powo { get; set; }
}

public class PlantNetSpecies
{
    /// <summary>Главное поле для матчинга: "Monstera deliciosa" без автора.</summary>
    public string? ScientificNameWithoutAuthor { get; set; }
    public string? ScientificName { get; set; }
    public List<string> CommonNames { get; set; } = new();
}

public class PlantNetRef
{
    // Pl@ntNet может отдавать id строкой или числом — берём терпимо.
    [JsonConverter(typeof(StringOrNumberConverter))]
    public string? Id { get; set; }
}

/// <summary>Читает id как строку независимо от того, строка это или число в JSON.</summary>
public class StringOrNumberConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type t, JsonSerializerOptions o) =>
        reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.TryGetInt64(out var l)
                ? l.ToString()
                : reader.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
            _ => null
        };

    public override void Write(Utf8JsonWriter w, string? v, JsonSerializerOptions o) =>
        w.WriteStringValue(v);
}
