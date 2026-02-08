namespace CanPany.Application.DTOs;

/// <summary>
/// DTO for notification response
/// </summary>
public class NotificationResponseDto
{
    /// <summary>
    /// Notification ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Notification type (e.g., "ProposalAccepted", "NewMessage", "JobMatch", "PaymentConfirmation")
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Notification title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification content/message
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Notification timestamp (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Whether the notification has been read
    /// </summary>
    public bool IsRead { get; set; }
}
