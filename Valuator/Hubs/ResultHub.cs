using Microsoft.AspNetCore.SignalR;

namespace Valuator.Hubs;
public class ResultHub : Hub
{
    public async Task JoinTextGroup(string id)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, id);
    }
}

