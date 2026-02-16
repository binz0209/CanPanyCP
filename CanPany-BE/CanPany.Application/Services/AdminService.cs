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
    private readonly IEmailService? _emailService;  // Add this
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUserRepository userRepo,
        ICompanyRepository companyRepo,
        IJobRepository jobRepo,
        IPaymentRepository paymentRepo,
        INotificationRepository notificationRepo,
        IAuditLogRepository auditLogRepo,
        ILogger<AdminService> logger,
        IEmailService? emailService = null)  // Add this parameter as optional
    {
        _userRepo = userRepo;
        _companyRepo = companyRepo;
        _jobRepo = jobRepo;
        _paymentRepo = paymentRepo;
        _notificationRepo = notificationRepo;
        _auditLogRepo = auditLogRepo;
        _logger = logger;
        _emailService = emailService;  // Add this
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
            // Get target users based on role
            var users = targetRole == null 
                ? await _userRepo.GetAllAsync() 
                : await _userRepo.GetByRoleAsync(targetRole);

            var usersList = users.ToList();
            
            if (!usersList.Any())
            {
                _logger.LogWarning("No users found for broadcast notification. Role: {Role}", targetRole ?? "All");
                return false;
            }

            // Create notifications for each user
            var notificationTasks = new List<Task>();
            
            foreach (var user in usersList)
            {
                var notification = new Notification
                {
                    UserId = user.Id,
                    Type = "Broadcast",
                    Title = title,
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                
                notificationTasks.Add(_notificationRepo.AddAsync(notification));
                
                // Optionally send email if configured
                if (_emailService != null && !string.IsNullOrEmpty(user.Email))
                {
                    notificationTasks.Add(SendEmailSafelyAsync(user.Email, title, message));
                }
            }

            // Execute all tasks in parallel
            await Task.WhenAll(notificationTasks);

            _logger.LogInformation(
                "Broadcast notification sent successfully. Title: '{Title}', Recipients: {Count}, Role: {Role}", 
                title, 
                usersList.Count, 
                targetRole ?? "All");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending broadcast notification. Title: '{Title}', Role: {Role}", title, targetRole);
            throw;
        }
    }

    private async Task SendEmailSafelyAsync(string email, string title, string message)
    {
        try
        {
            if (_emailService != null)
            {
                await _emailService.SendNotificationEmailAsync(email, title, message);
            }
        }
        catch (Exception ex)
        {
            // Log but don't throw - email failure shouldn't fail the entire broadcast
            _logger.LogWarning(ex, "Failed to send email notification to {Email}", email);
        }
    }
}

