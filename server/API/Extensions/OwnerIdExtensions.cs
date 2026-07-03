using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PlantCare.Api.Controllers;

public static class OwnerIdExtensions
{
    public static string GetOwnerId(this ControllerBase controller)
    {
        var userId = controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            return userId;
        }

        throw new InvalidOperationException("Authenticated user id claim is missing.");
    }
}
