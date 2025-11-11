using Microsoft.AspNetCore.SignalR;

namespace TaskMaster.API.Hubs;

/// <summary>
/// SignalR Hub for real-time task synchronization
/// </summary>
public class SyncHub : Hub
{
    /// <summary>
    /// Notifies all connected clients that tasks have been updated
    /// </summary>
    public async Task TasksUpdated()
    {
        await Clients.All.SendAsync("TasksUpdated");
    }
}

