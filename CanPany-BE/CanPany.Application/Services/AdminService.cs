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
    private readonly IEmailService? _emailService;
    private readonly ICascadeDeleteService _cascadeDeleteService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUserRepository userRepo,
        ICompanyRepository companyRepo,
        IJobRepository jobRepo,
        IPaymentRepository paymentRepo,
        INotificationRepository notificationRepo,
        IAuditLogRepository auditLogRepo,
        ILogger<AdminService> logger,
        ICascadeDeleteService cascadeDeleteService,
        IEmailService? emailService = null)
    {
        _userRepo = userRepo;
        _companyRepo = companyRepo;
        _jobRepo = jobRepo;
        _paymentRepo = paymentRepo;
        _notificationRepo = notificationRepo;
        _auditLogRepo = auditLogRepo;
        _logger = logger;
        _cascadeDeleteService = cascadeDeleteService;
        _emailService = emailService;
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

    // ─────────────────────────────────────────────────────────────────────────
    // UC-44: Moderate User Accounts
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>GET /admin/users/{id}</summary>
    public async Task<User?> GetUserByIdAsync(string userId)
    {
        try
        {
            return await _userRepo.GetByIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by id: {UserId}", userId);
            throw;
        }
    }

    /// <summary>GET /admin/users?search=&amp;role=&amp;status=&amp;page=&amp;pageSize=</summary>
    public async Task<IEnumerable<User>> SearchUsersAsync(string? search, string? role, string? status, int page, int pageSize)
    {
        try
        {
            // Fetch filtered by role first (indexed), then narrow down in memory
            IEnumerable<User> users = string.IsNullOrWhiteSpace(role)
                ? await _userRepo.GetAllAsync()
                : await _userRepo.GetByRoleAsync(role);

            // Filter by status (isLocked)
            if (!string.IsNullOrWhiteSpace(status))
            {
                var isLocked = string.Equals(status, "banned", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(status, "locked", StringComparison.OrdinalIgnoreCase);
                users = users.Where(u => u.IsLocked == isLocked);
            }

            // Full-text search across email / fullName
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim();
                users = users.Where(u =>
                    u.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(q, StringComparison.OrdinalIgnoreCase));
            }

            // Pagination (1-based page)
            var safePage = Math.Max(1, page);
            var safeSize = Math.Clamp(pageSize, 1, 200);
            return users.Skip((safePage - 1) * safeSize).Take(safeSize).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users");
            throw;
        }
    }

    /// <summary>DELETE /admin/users/{id} — cascade deletes all related data</summary>
    public async Task<bool> DeleteUserAsync(string userId)
    {
        try
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;

            // 1. If user is Company, cascade delete company + its jobs/applications
            if (user.Role == "Company")
            {
                var company = await _companyRepo.GetByUserIdAsync(userId);
                if (company != null)
                {
                    await _cascadeDeleteService.CascadeDeleteCompanyDataAsync(company.Id);
                    await _companyRepo.DeleteAsync(company.Id);
                }
            }

            // 2. Cascade delete all user-level data
            await _cascadeDeleteService.CascadeDeleteUserDataAsync(userId);

            // 3. Delete user document
            await _userRepo.DeleteAsync(userId);
            _logger.LogInformation("Admin permanently deleted user {UserId} and all related data", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently deleting user: {UserId}", userId);
            throw;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UC-45: Process Verification Requests
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>GET /admin/companies/verification-requests — fix stub</summary>
    public async Task<IEnumerable<Company>> GetVerificationRequestsAsync()
    {
        try
        {
            return await _companyRepo.GetPendingVerificationsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification requests");
            throw;
        }
    }

    /// <summary>GET /admin/companies?status=</summary>
    public async Task<IEnumerable<Company>> GetAllCompaniesAsync(string? status)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(status))
                return await _companyRepo.GetByVerificationStatusAsync(status);

            return await _companyRepo.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all companies");
            throw;
        }
    }

    /// <summary>GET /admin/companies/{id}</summary>
    public async Task<Company?> GetCompanyByIdAsync(string companyId)
    {
        try
        {
            return await _companyRepo.GetByIdAsync(companyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company by id: {CompanyId}", companyId);
            throw;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UC-46: Moderate Job Content
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>GET /admin/jobs?status=</summary>
    public async Task<IEnumerable<Job>> GetAllJobsAsync(string? status)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(status))
                return await _jobRepo.GetByStatusAsync(status);

            return await _jobRepo.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all jobs");
            throw;
        }
    }

    /// <summary>GET /admin/jobs/{id}</summary>
    public async Task<Job?> GetJobByIdAsync(string jobId)
    {
        try
        {
            return await _jobRepo.GetByIdAsync(jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job by id: {JobId}", jobId);
            throw;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UC-48: Audit Logs & Export
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>GET /admin/audit-logs/export?format=csv</summary>
    public async Task<byte[]> ExportAuditLogsCsvAsync(string? userId = null, string? entityType = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var logs = await GetAuditLogsAsync(userId, entityType, fromDate, toDate);

            var sb = new System.Text.StringBuilder();
            // RFC-4180 header
            sb.AppendLine("Id,UserId,UserEmail,Action,EntityType,EntityId,HttpMethod,Endpoint,ResponseStatusCode,IpAddress,Duration,ErrorMessage,CreatedAt");

            foreach (var log in logs)
            {
                sb.AppendLine(string.Join(",",
                    CsvEscape(log.Id),
                    CsvEscape(log.UserId),
                    CsvEscape(log.UserEmail),
                    CsvEscape(log.Action),
                    CsvEscape(log.EntityType),
                    CsvEscape(log.EntityId),
                    CsvEscape(log.HttpMethod),
                    CsvEscape(log.Endpoint),
                    log.ResponseStatusCode?.ToString() ?? string.Empty,
                    CsvEscape(log.IpAddress),
                    log.Duration?.ToString() ?? string.Empty,
                    CsvEscape(log.ErrorMessage),
                    log.CreatedAt.ToString("o")));
            }

            return System.Text.Encoding.UTF8.GetPreamble()
                .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit logs");
            throw;
        }
    }

    private static string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Payment Oversight
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>GET /admin/payments?status= — fix stub</summary>
    public async Task<IEnumerable<Payment>> GetAllPaymentsAsync(string? status)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(status))
                return await _paymentRepo.GetByStatusAsync(status);

            return await _paymentRepo.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all payments");
            throw;
        }
    }

    /// <summary>GET /admin/payments/{id}</summary>
    public async Task<Payment?> GetPaymentByIdAsync(string paymentId)
    {
        try
        {
            return await _paymentRepo.GetByIdAsync(paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment by id: {PaymentId}", paymentId);
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

