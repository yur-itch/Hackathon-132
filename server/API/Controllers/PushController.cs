using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.Dtos;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Controllers;

[ApiController]
[Route("api/push")]
public sealed class PushController : ControllerBase
{
    private readonly IPushService _pushService;

    public PushController(IPushService pushService)
    {
        _pushService = pushService;
    }

    [HttpGet("vapid-public-key")]
    public ActionResult<VapidPublicKeyDto> GetVapidPublicKey()
        => Ok(new VapidPublicKeyDto(_pushService.GetPublicKey()));

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscribeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Endpoint) || string.IsNullOrWhiteSpace(dto.Keys?.P256dh) || string.IsNullOrWhiteSpace(dto.Keys?.Auth))
            return BadRequest("Endpoint и keys обязательны.");

        await _pushService.SubscribeAsync(this.GetOwnerId(), dto.Endpoint, dto.Keys.P256dh, dto.Keys.Auth);
        return NoContent();
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] PushUnsubscribeDto dto)
    {
        await _pushService.UnsubscribeAsync(this.GetOwnerId(), dto.Endpoint);
        return NoContent();
    }
}
