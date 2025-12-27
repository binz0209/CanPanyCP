using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Admin service implementation
/// </summary>
public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepo;
    private readonly ICompanyRepository _companyRepo;
    private readonly IJobRepository _jobRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly INotificationRepository _notificationRepo;
    private readonly IAuditLogRepository _auditLogRepo;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUserRepository userRepo,
        ICompanyRepository companyRepo,
        IJobRepository jobRepo,
        IPaymentRepository paymentRepo,
        INotificationRepository notificationRepo,
        IAuditLogRepository auditLogRepo,
        ILogger<AdminService> logger)
    {
        _userRepo = userRepo;
        _companyRepo = companyRepo;
        _jobRepo = jobRepo;
        _paymentRepo = paymentRepo;
        _notificationRepo = notificationRepo;
        _auditLogRepo = auditLogRepo;
        _logger = logger;
    }

    public async Task<bool> BanUserAsync(string userId)
    {
        try
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.IsLocked = true;
            user.LockedUntil = DateTime.UtcNow.AddYears(100); // Permanent ban
            user.MarkAsUpdated();
            await _userRepo.UpdateAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error banning user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UnbanUserAsync(string userId)
    {
        try
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.IsLocked = false;
            user.LockedUntil = null;
            user.MarkAsUpdated();
            await _userRepo.UpdateAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unbanning user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ApproveCompanyVerificationAsync(string companyId)
    {
        try
        {
            var company = await _companyRepo.GetByIdAsync(companyId);
            if (company == null)
                return false;

            company.IsVerified = true;
            company.VerificationStatus = "Approved";
            company.VerifiedAt = DateTime.UtcNow;
            company.MarkAsUpdated();
            await _companyRepo.UpdateAsync(company);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving company verification: {CompanyId}", companyId);
            throw;
        }
    }

    public async Task<bool> RejectCompanyVerificationAsync(string companyId, string reason)
    {
        try
        {
            var company = await _companyRepo.GetByIdAsync(companyId);
            if (company == null)
                return false;

            company.IsVerified = false;
            company.VerificationStatus = "Rejected";
            company.MarkAsUpdated();
            await _companyRepo.UpdateAsync(company);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting company verification: {CompanyId}", companyId);
            throw;
        }
    }

    public async Task<bool> HideJobAsync(string jobId, string reason)
    {
        try
        {
            var job = await _jobRepo.GetByIdAsync(jobId);
            if (job == null)
                return false;

            job.Status = "Hidden";
            job.MarkAsUpdated();
            await _jobRepo.UpdateAsync(job);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hiding job: {JobId}", jobId);
            throw;
        }
    }

    public async Task<bool> DeleteJobAsync(string jobId)
    {
        try
        {
            await _jobRepo.DeleteAsync(jobId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job: {JobId}", jobId);
            throw;
        }
    }

    public async Task<bool> ApprovePaymentRequestAsync(string paymentId)
    {
        try
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            payment.Status = "Paid";
            payment.PaidAt = DateTime.UtcNow;
            payment.MarkAsUpdated();
            await _paymentRepo.UpdateAsync(payment);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving payment request: {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<bool> RejectPaymentRequestAsync(string paymentId, string reason)
    {
        try
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            payment.Status = "Failed";
            payment.MarkAsUpdated();
            await _paymentRepo.UpdateAsync(payment);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting payment request: {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? userId = null, string? entityType = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                return await _auditLogRepo.GetByUserIdAsync(userId);
            }

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                return await _auditLogRepo.GetByEntityTypeAsync(entityType);
            }

            if (fromDate.HasValue && toDate.HasValue)
            {
                return await _auditLogRepo.GetByDateRangeAsync(fromDate.Value, toDate.Value);
            }

            return await _auditLogRepo.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs");
            throw;
        }
    }

    public async Task<bool> SendBroadcastNotificationAsync(string title, string message, string? targetRole = null)
    {
        try
        {
            // TODO: Implement broadcast notification
            // This should send notification to all users or users with specific role
            _logger.LogInformation("Broadcast notification: {Title}, Role: {Role}", title, targetRole ?? "All");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending broadcast notification");
            throw;
        }
    }
}

