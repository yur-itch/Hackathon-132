using PlantCare.Api.Services.PlantNet.Models;

namespace PlantCare.Api.Services.PlantNet;

public interface IPlantNetClient
{
    /// <summary>
    /// Один вызов распознавания. Возвращает сырой список кандидатов (без матчинга).
    /// В мок-режиме отдаёт фикстуру plantnet-{scenario}.json (по умолчанию "monstera").
    /// </summary>
    Task<PlantNetResponse> IdentifyAsync(
        Stream image, string fileName, string organ, string? scenario, CancellationToken ct);
}
