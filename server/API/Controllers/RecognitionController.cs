using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Services.Recognition;
using PlantCare.Api.Services.Recognition.Models;

namespace PlantCare.Api.Controllers;

/// <summary>Распознавание растения по фото (проксируем Pl@ntNet, ключ не утекает на фронт).</summary>
[ApiController]
[Route("api/[controller]")]
public class RecognitionController : ControllerBase
{
    private static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/jpg" };
    private const long MaxBytes = 5 * 1024 * 1024; // 5 МБ

    private readonly IRecognitionService _service;
    public RecognitionController(IRecognitionService service) => _service = service;

    /// <summary>
    /// Определить растение по загруженному фото.
    /// multipart/form-data: image (файл), organ (leaf|flower|fruit|bark|auto, опц.).
    /// scenario (query) — только в мок-режиме: monstera|secondmatch|unknown|lowconfidence.
    /// </summary>
    [HttpPost("identify")]
    public async Task<ActionResult<RecognitionResult>> Identify(
        IFormFile image,
        [FromForm] string? organ,
        [FromQuery] string? scenario,
        CancellationToken ct)
    {
        if (image is null || image.Length == 0)
            return BadRequest("Файл изображения не передан.");
        if (image.Length > MaxBytes)
            return BadRequest("Файл больше 5 МБ.");
        if (!AllowedTypes.Contains(image.ContentType))
            return BadRequest("Поддерживаются только JPEG и PNG.");

        await using var stream = image.OpenReadStream();
        var result = await _service.IdentifyAsync(
            stream, image.FileName, organ ?? "auto", scenario, ct);

        return Ok(result);
    }
}
