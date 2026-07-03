using PlantCare.Api.Services.Recognition.Models;

namespace PlantCare.Api.Services.Recognition;

public interface IRecognitionService
{
    Task<RecognitionResult> IdentifyAsync(
        Stream image, string fileName, string organ, string? scenario, CancellationToken ct);
}
