using System.Text.Json;
using Microsoft.Extensions.Options;
using PlantCare.Api.Services.PlantNet.Models;

namespace PlantCare.Api.Services.PlantNet;

public class PlantNetClient : IPlantNetClient
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _http;
    private readonly PlantNetOptions _opts;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<PlantNetClient> _log;

    public PlantNetClient(
        HttpClient http, IOptions<PlantNetOptions> opts,
        IWebHostEnvironment env, ILogger<PlantNetClient> log)
    {
        _http = http;
        _opts = opts.Value;
        _env = env;
        _log = log;
    }

    // Мокаем, если явно попросили ИЛИ ключа нет — чтобы всё работало из коробки без ключа.
    private bool UseMock => _opts.UseMock || string.IsNullOrWhiteSpace(_opts.ApiKey);

    public async Task<PlantNetResponse> IdentifyAsync(
        Stream image, string fileName, string organ, string? scenario, CancellationToken ct)
    {
        if (UseMock)
        {
            // Отсутствующий ключ — это не то же самое, что осознанный демо-мок
            // (UseMock=true): без ключа результат подменяется фикстурой незаметно
            // для того, кто ждёт реального распознавания. Предупреждаем громче.
            if (string.IsNullOrWhiteSpace(_opts.ApiKey))
            {
                _log.LogWarning(
                    "PlantNet:ApiKey не задан — распознавание работает на фикстурах, " +
                    "реальные фото не отправляются в Pl@ntNet.");
            }

            return await LoadFixtureAsync(scenario, ct);
        }

        var url = $"{_opts.BaseUrl}/{_opts.Project}?api-key={_opts.ApiKey}";

        using var form = new MultipartFormDataContent();
        var imageContent = new StreamContent(image);
        imageContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        form.Add(imageContent, "images", fileName);
        form.Add(new StringContent(string.IsNullOrWhiteSpace(organ) ? "auto" : organ), "organs");

        var resp = await _http.PostAsync(url, form, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<PlantNetResponse>(json, JsonOpts) ?? new PlantNetResponse();
    }

    private async Task<PlantNetResponse> LoadFixtureAsync(string? scenario, CancellationToken ct)
    {
        var name = string.IsNullOrWhiteSpace(scenario) ? "monstera" : scenario;
        var path = Path.Combine(_env.ContentRootPath, "Fixtures", $"plantnet-{name}.json");

        if (!File.Exists(path))
        {
            _log.LogWarning("Фикстура {Path} не найдена — возвращаю пустой результат", path);
            return new PlantNetResponse();
        }

        _log.LogInformation("Pl@ntNet MOCK: сценарий '{Scenario}'", name);
        var json = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<PlantNetResponse>(json, JsonOpts) ?? new PlantNetResponse();
    }
}
