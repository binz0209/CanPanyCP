using CanPany.Api.Hubs;
using CanPany.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CanPany.Api.Services;

public class RealTimeNotificationService : IRealTimeNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public RealTimeNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(string userId, object payload)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("ReceiveNotification", payload);
    }
}