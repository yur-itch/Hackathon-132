using Microsoft.AspNetCore.SignalR;

namespace PlantCare.Api.Hubs;

public class ChatHub : Hub
{
    public async Task JoinExchangeChat(string exchangeOfferId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"exchange_{exchangeOfferId}");
    }

    public async Task LeaveExchangeChat(string exchangeOfferId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"exchange_{exchangeOfferId}");
    }
}
