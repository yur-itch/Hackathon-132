using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PlantCare.Api.Data;
using PlantCare.Api.Services.PlantNet;
using PlantCare.Api.Services.PlantNet.Models;
using PlantCare.Api.Services.Recognition.Models;

namespace PlantCare.Api.Services.Recognition;

public class RecognitionService : IRecognitionService
{
    private readonly IPlantNetClient _client;
    private readonly AppDbContext _db;
    private readonly PlantNetOptions _opts;
    private readonly ILogger<RecognitionService> _log;

    public RecognitionService(
        IPlantNetClient client, AppDbContext db,
        IOptions<PlantNetOptions> opts, ILogger<RecognitionService> log)
    {
        _client = client;
        _db = db;
        _opts = opts.Value;
        _log = log;
    }

    public async Task<RecognitionResult> IdentifyAsync(
        Stream image, string fileName, string organ, string? scenario, CancellationToken ct)
    {
        PlantNetResponse resp;
        try
        {
            resp = await _client.IdentifyAsync(image, fileName, organ, scenario, ct);
        }
        catch (Exception ex)
        {
            // Фаза 5 — единственное место «фейла», и он мягкий (не 500).
            _log.LogWarning(ex, "Ошибка вызова Pl@ntNet");
            return new RecognitionResult
            {
                Status = MatchStatus.Failed,
                Message = "Сервис распознавания недоступен. Попробуйте позже или добавьте растение вручную."
            };
        }

        var candidates = resp.Results
            .Select(r => new CandidateDto
            {
                LatinName = r.Species.ScientificNameWithoutAuthor ?? r.Species.ScientificName,
                CommonName = r.Species.CommonNames.FirstOrDefault(),
                Score = r.Score,
                GbifId = r.Gbif?.Id
            })
            .ToList();

        var top = resp.Results.FirstOrDefault();

        // Слой 1 — порог уверенности.
        if (top is null || top.Score < _opts.ConfidenceThreshold)
        {
            return new RecognitionResult
            {
                Status = MatchStatus.LowConfidence,
                RecognizedLatinName = top?.Species.ScientificNameWithoutAuthor,
                TopScore = top?.Score,
                Candidates = candidates,
                Message = "Не удалось уверенно распознать. Попробуйте другое фото (чётче, крупнее лист/цветок)."
            };
        }

        // Справочник целиком (десятки записей) — грузим один раз для матчинга в памяти.
        var plants = await _db.Plants.ToListAsync(ct);

        // Слой 2 — идём по кандидатам сверху вниз, а не только по [0].
        foreach (var r in resp.Results)
        {
            var normalized = LatinNameNormalizer.Normalize(
                r.Species.ScientificNameWithoutAuthor ?? r.Species.ScientificName);

            var card = plants.FirstOrDefault(p =>
                // Слой 3 — сперва матч по GBIF id (стабильнее строки)
                (!string.IsNullOrEmpty(r.Gbif?.Id) && r.Gbif!.Id == p.GbifId)
                // затем по нормализованному латинскому имени
                || (normalized.Length > 0 && LatinNameNormalizer.Normalize(p.LatinName) == normalized));

            if (card is not null)
            {
                return new RecognitionResult
                {
                    Status = MatchStatus.Matched,
                    RecognizedLatinName = r.Species.ScientificNameWithoutAuthor,
                    RecognizedCommonName = r.Species.CommonNames.FirstOrDefault(),
                    TopScore = top.Score,
                    MatchedCard = card,
                    Candidates = candidates,
                    Message = $"Определено: {card.Name}."
                };
            }
        }

        // Слой 4 — честный fallback: вид определён, карточки нет.
        var topName = top.Species.ScientificNameWithoutAuthor ?? top.Species.ScientificName;
        return new RecognitionResult
        {
            Status = MatchStatus.RecognizedButNoCard,
            RecognizedLatinName = topName,
            RecognizedCommonName = top.Species.CommonNames.FirstOrDefault(),
            TopScore = top.Score,
            Candidates = candidates,
            Message = $"Похоже на «{topName}», но карточки в справочнике пока нет."
        };
    }
}
