namespace PlantCare.Api.Services.PlantNet;

public class PlantNetOptions
{
    public const string SectionName = "PlantNet";

    /// <summary>Ключ API. Хранить в user-secrets / переменной окружения, НЕ в git.</summary>
    public string ApiKey { get; set; } = "";

    /// <summary>Проект/флора: "all" или региональный (напр. "weurope").</summary>
    public string Project { get; set; } = "all";

    public string BaseUrl { get; set; } = "https://my-api.plantnet.org/v2/identify";

    /// <summary>Принудительно использовать фикстуры вместо реального API (демо/офлайн).</summary>
    public bool UseMock { get; set; }

    /// <summary>Порог уверенности топ-кандидата. Ниже — LowConfidence.</summary>
    public double ConfidenceThreshold { get; set; } = 0.3;
}
