using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace PlantCare.Api.Hubs;

public class ChatHub : Hub
{
    public async Task JoinChat(string offerId, string otherUserId)
    {
        var currentUserId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
        {
            throw new HubException("User is not authenticated.");
        }

        var groupName = GetGroupName(offerId, currentUserId, otherUserId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveChat(string offerId, string otherUserId)
    {
        var currentUserId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
        {
            return;
        }

        var groupName = GetGroupName(offerId, currentUserId, otherUserId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public static string GetGroupName(string offerId, string user1Id, string user2Id)
    {
        var sortedUsers = new[] { user1Id, user2Id }.OrderBy(u => u);
        return $"chat_{offerId}_{string.Join("_", sortedUsers)}";
    }
}
