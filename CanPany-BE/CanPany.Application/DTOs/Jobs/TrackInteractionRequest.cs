using CanPany.Domain.Entities;

namespace CanPany.Application.DTOs.Jobs;

/// <summary>
/// Request DTO for tracking user-job interactions
/// </summary>
public class TrackInteractionRequest
{
    /// <summary>
    /// Type of interaction: View (1), Click (2), Bookmark (3), Apply (4)
    /// </summary>
    public InteractionType Type { get; set; }
}
