using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CanPany.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications (Job alerts, Application updates, etc.)
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    private string GetUserId()
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new HubException("Unauthorized: missing user identity.");
        return userId;
    }

    public override async Task OnConnectedAsync()
    {
        try 
        {
            var userId = GetUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("✅ User {UserId} connected to NotificationHub. ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("⚠ Connection attempt without valid user identity: {Message}", ex.Message);
            // We can log claims here to debug if needed
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                foreach (var claim in Context.User.Claims)
                {
                    _logger.LogDebug("Claim: {Type} = {Value}", claim.Type, claim.Value);
                }
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try 
        {
            var userId = GetUserId();
            _logger.LogInformation("❌ User {UserId} disconnected from NotificationHub", userId);
        }
        catch
        {
            _logger.LogInformation("❌ Anonymous/Invalid connection {ConnectionId} disconnected", Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
