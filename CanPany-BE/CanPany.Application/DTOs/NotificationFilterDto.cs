namespace CanPany.Application.DTOs;

/// <summary>
/// DTO for filtering notifications
/// </summary>
public class NotificationFilterDto
{
    /// <summary>
    /// Filter by read status (null = all, true = read only, false = unread only)
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>
    /// Filter by notification type (e.g., "ProposalAccepted", "NewMessage", "JobMatch", "PaymentConfirmation")
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Filter notifications created after this date
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter notifications created before this date
    /// </summary>
    public DateTime? ToDate { get; set; }
}
