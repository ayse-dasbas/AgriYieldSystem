// SignalR hub — istemcilerin sera bazlı gruplara katılmasını/ayrılmasını yönetir; gerçek zamanlı telemetri yayını bu hub üzerinden yapılır.
using Microsoft.AspNetCore.SignalR;

namespace AgriYield.Api.Hubs;

public class AgriHub : Hub
{
    // İstemci belirli bir sera grubuna katılır; sadece o seraya özel mesajlar alabilir.
    public async Task JoinGreenhouseGroup(string greenhouseId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Greenhouse_{greenhouseId}");
    }

    // İstemci sera grubundan ayrılır.
    public async Task LeaveGreenhouseGroup(string greenhouseId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Greenhouse_{greenhouseId}");
    }
}
