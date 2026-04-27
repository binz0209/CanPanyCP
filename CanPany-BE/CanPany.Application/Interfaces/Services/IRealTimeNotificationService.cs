namespace CanPany.Application.Interfaces.Services;

public interface IRealTimeNotificationService
{
    Task SendNotificationAsync(string userId, object payload);
}