namespace CanPany.Worker.Models.Payloads;

/// <summary>
/// Payload for the CV generation background job.
/// </summary>
public class CVGenerationPayload
{
    public string UserId { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>
    /// Optional: If provided, the CV is tailored specifically for this job's description.
    /// </summary>
    public string? JobId { get; set; }
}
