using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Admin service interface
/// </summary>
public interface IAdminService
{
    Task<bool> BanUserAsync(string userId);
    Task<bool> UnbanUserAsync(string userId);
    Task<bool> ApproveCompanyVerificationAsync(string companyId);
    Task<bool> RejectCompanyVerificationAsync(string companyId, string reason);
    Task<bool> HideJobAsync(string jobId, string reason);
    Task<bool> DeleteJobAsync(string jobId);
    Task<bool> ApprovePaymentRequestAsync(string paymentId);
    Task<bool> RejectPaymentRequestAsync(string paymentId, string reason);
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? userId = null, string? entityType = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<bool> SendBroadcastNotificationAsync(string title, string message, string? targetRole = null);
}


