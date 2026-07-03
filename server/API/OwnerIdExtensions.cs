using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PlantCare.Api.Controllers;

/// <summary>
/// Владелец коллекции: если пользователь залогинен (валидный JWT в куке access_token) —
/// его реальный id, иначе — заголовок X-User-Id, иначе — "local" (анонимный режим из ТЗ).
/// </summary>
public static class OwnerIdExtensions
{
    public static string GetOwnerId(this ControllerBase controller)
    {
        var userId = controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
            return userId;

        return controller.Request.Headers.TryGetValue("X-User-Id", out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : "local";
    }
}
