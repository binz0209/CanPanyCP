using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Admin service interface
/// </summary>
public interface IAdminService
{
    // ── UC-44: Moderate Users ────────────────────────────────────────────────
    Task<bool> BanUserAsync(string userId);
    Task<bool> UnbanUserAsync(string userId);
    Task<User?> GetUserByIdAsync(string userId);
    Task<IEnumerable<User>> SearchUsersAsync(string? search, string? role, string? status, int page, int pageSize);
    Task<bool> DeleteUserAsync(string userId);

    // ── UC-45: Company Verification ──────────────────────────────────────────
    Task<bool> ApproveCompanyVerificationAsync(string companyId);
    Task<bool> RejectCompanyVerificationAsync(string companyId, string reason);
    Task<IEnumerable<Company>> GetVerificationRequestsAsync();
    Task<IEnumerable<Company>> GetAllCompaniesAsync(string? status);
    Task<Company?> GetCompanyByIdAsync(string companyId);

    // ── UC-46: Moderate Jobs ─────────────────────────────────────────────────
    Task<bool> HideJobAsync(string jobId, string reason);
    Task<bool> DeleteJobAsync(string jobId);
    Task<IEnumerable<Job>> GetAllJobsAsync(string? status);
    Task<Job?> GetJobByIdAsync(string jobId);

    // ── UC-48: Audit Logs & Export ───────────────────────────────────────────
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? userId = null, string? entityType = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> ExportAuditLogsCsvAsync(string? userId = null, string? entityType = null, DateTime? fromDate = null, DateTime? toDate = null);

    // ── Payment Oversight ────────────────────────────────────────────────────
    Task<bool> ApprovePaymentRequestAsync(string paymentId);
    Task<bool> RejectPaymentRequestAsync(string paymentId, string reason);
    Task<IEnumerable<Payment>> GetAllPaymentsAsync(string? status);
    Task<Payment?> GetPaymentByIdAsync(string paymentId);

    // ── Broadcast ────────────────────────────────────────────────────────────
    Task<bool> SendBroadcastNotificationAsync(string title, string message, string? targetRole = null);
}



