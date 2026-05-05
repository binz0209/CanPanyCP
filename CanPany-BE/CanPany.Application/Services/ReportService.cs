using CanPany.Application.DTOs;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Report service implementation
/// </summary>
public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepo;
    private readonly IUserRepository _userRepo;
    private readonly ICompanyRepository _companyRepo;
    private readonly IJobRepository _jobRepo;
    private readonly INotificationService _notificationService;
    private readonly IAdminService _adminService;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IReportRepository reportRepo,
        IUserRepository userRepo,
        ICompanyRepository companyRepo,
        IJobRepository jobRepo,
        INotificationService notificationService,
        IAdminService adminService,
        ILogger<ReportService> logger)
    {
        _reportRepo = reportRepo;
        _userRepo = userRepo;
        _companyRepo = companyRepo;
        _jobRepo = jobRepo;
        _notificationService = notificationService;
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<ReportDetailsDto> CreateReportAsync(string reporterId, CreateReportDto dto)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(dto.ReportedUserId) && dto.ReportedUserId == reporterId)
            {
                throw new ArgumentException("You cannot report yourself", nameof(dto.ReportedUserId));
            }

            // Validate report targets exist
            if (!string.IsNullOrWhiteSpace(dto.ReportedUserId))
            {
                var reportedUser = await _userRepo.GetByIdAsync(dto.ReportedUserId);
                if (reportedUser == null)
                    throw new ArgumentException("Reported user not found", nameof(dto.ReportedUserId));
            }

            if (!string.IsNullOrWhiteSpace(dto.ReportedCompanyId))
            {
                var reportedCompany = await _companyRepo.GetByIdAsync(dto.ReportedCompanyId);
                if (reportedCompany == null)
                    throw new ArgumentException("Reported company not found", nameof(dto.ReportedCompanyId));
            }

            if (!string.IsNullOrWhiteSpace(dto.ReportedJobId))
            {
                var reportedJob = await _jobRepo.GetByIdAsync(dto.ReportedJobId);
                if (reportedJob == null)
                    throw new ArgumentException("Reported job not found", nameof(dto.ReportedJobId));
            }

            // Create report entity
            var report = new Report
            {
                ReporterId = reporterId,
                ReportedUserId = dto.ReportedUserId,
                ReportedCompanyId = dto.ReportedCompanyId,
                ReportedJobId = dto.ReportedJobId,
                Reason = dto.Reason,
                Description = dto.Description,
                Evidence = dto.Evidence ?? new List<string>(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _reportRepo.AddAsync(report);

            // Send notification to all admins
            await SendAdminNotificationAsync(reporterId, dto.Reason);

            _logger.LogInformation(
                "Report created: {ReportId} by {ReporterId} against User:{ReportedUserId} Company:{ReportedCompanyId} Job:{ReportedJobId}",
                created.Id, reporterId, dto.ReportedUserId, dto.ReportedCompanyId, dto.ReportedJobId);

            return (await MapToReportDetailsDto(created))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating report");
            throw;
        }
    }

    public async Task<ReportDetailsDto?> GetReportByIdAsync(string id)
    {
        try
        {
            var report = await _reportRepo.GetByIdAsync(id);
            if (report == null) return null;
            return await MapToReportDetailsDto(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report by ID: {ReportId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ReportDetailsDto>> GetReportsByReporterIdAsync(string reporterId)
    {
        try
        {
            var reports = await _reportRepo.GetByReporterIdAsync(reporterId);
            var result = new List<ReportDetailsDto>();
            foreach (var report in reports)
            {
                var dto = await MapToReportDetailsDto(report);
                if (dto != null) result.Add(dto);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reports by reporter ID: {ReporterId}", reporterId);
            throw;
        }
    }

    public async Task<IEnumerable<ReportDetailsDto>> GetAllReportsAsync(ReportFilterDto? filter = null)
    {
        try
        {
            IEnumerable<Report> reports;

            if (filter != null && (filter.Status != null || filter.FromDate != null || filter.ToDate != null))
            {
                reports = await _reportRepo.GetWithFiltersAsync(filter.Status, filter.FromDate, filter.ToDate);
            }
            else
            {
                reports = await _reportRepo.GetAllAsync();
            }

            // Map to DTOs with user information
            var reportDetails = new List<ReportDetailsDto>();
            foreach (var report in reports)
            {
                var details = await MapToReportDetailsDto(report);
                if (details != null)
                {
                    reportDetails.Add(details);
                }
            }

            return reportDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all reports");
            throw;
        }
    }

    public async Task<ReportDetailsDto?> GetReportDetailsAsync(string id)
    {
        try
        {
            var report = await _reportRepo.GetByIdAsync(id);
            if (report == null)
                return null;

            return await MapToReportDetailsDto(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report details: {ReportId}", id);
            throw;
        }
    }

    public async Task<bool> ResolveReportAsync(string reportId, string adminId, string resolutionNote, bool banUser)
    {
        try
        {
            var report = await _reportRepo.GetByIdAsync(reportId);
            if (report == null || report.Status != "Pending")
            {
                _logger.LogWarning("Report not found or not pending: {ReportId}", reportId);
                return false;
            }

            // Ban user if violation confirmed
            if (banUser)
            {
                if (string.IsNullOrWhiteSpace(report.ReportedUserId))
                {
                    _logger.LogWarning(
                        "Skip ban for report {ReportId}: banUser=true but ReportedUserId is empty (report type may be company/job)",
                        reportId);
                }
                else
                {
                    var banned = await _adminService.BanUserAsync(report.ReportedUserId);
                    if (!banned)
                    {
                        _logger.LogWarning("Ban user failed for report {ReportId}, user: {UserId}", reportId, report.ReportedUserId);
                        return false;
                    }

                    _logger.LogInformation("User banned: {UserId} due to report: {ReportId}",
                        report.ReportedUserId, reportId);

                    // Send notification to reported user
                    await SendUserBannedNotificationAsync(report.ReportedUserId, resolutionNote);
                }
            }

            // Update report
            report.Status = "Resolved";
            report.ResolvedBy = adminId;
            report.ResolvedAt = DateTime.UtcNow;
            report.ResolutionNote = resolutionNote;
            await _reportRepo.UpdateAsync(report);

            // Send notification to reporter
            await SendReportResolvedNotificationToReporterAsync(report.ReporterId, resolutionNote);

            // Send notification to reported user (for user-report flow)
            if (!string.IsNullOrWhiteSpace(report.ReportedUserId))
            {
                await SendReportResolvedNotificationToReportedUserAsync(report.ReportedUserId, resolutionNote);
            }

            _logger.LogInformation("Report resolved: {ReportId} by admin: {AdminId}", reportId, adminId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving report: {ReportId}", reportId);
            throw;
        }
    }

    public async Task<bool> RejectReportAsync(string reportId, string adminId, string rejectionReason)
    {
        try
        {
            var report = await _reportRepo.GetByIdAsync(reportId);
            if (report == null || report.Status != "Pending")
            {
                _logger.LogWarning("Report not found or not pending: {ReportId}", reportId);
                return false;
            }

            // Update report
            report.Status = "Rejected";
            report.ResolvedBy = adminId;
            report.ResolvedAt = DateTime.UtcNow;
            report.ResolutionNote = rejectionReason;
            await _reportRepo.UpdateAsync(report);

            // Send notification to reporter
            await SendReportRejectedNotificationAsync(report.ReporterId, rejectionReason);

            _logger.LogInformation("Report rejected: {ReportId} by admin: {AdminId}", reportId, adminId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting report: {ReportId}", reportId);
            throw;
        }
    }

    // Helper methods
    private async Task<ReportDetailsDto?> MapToReportDetailsDto(Report report)
    {
        try
        {
            var reporter = await _userRepo.GetByIdAsync(report.ReporterId);
            if (reporter == null)
                return null;

            User? reportedUser = null;
            if (!string.IsNullOrEmpty(report.ReportedUserId))
            {
                reportedUser = await _userRepo.GetByIdAsync(report.ReportedUserId);
            }

            return new ReportDetailsDto(
                Id: report.Id,
                Reporter: new UserBasicInfo(reporter.Id, reporter.FullName, reporter.Email, reporter.AvatarUrl),
                ReportedUser: reportedUser != null
                    ? new UserBasicInfo(reportedUser.Id, reportedUser.FullName, reportedUser.Email, reportedUser.AvatarUrl)
                    : null,
                ReportedCompanyId: report.ReportedCompanyId,
                ReportedJobId: report.ReportedJobId,
                ReportType: !string.IsNullOrWhiteSpace(report.ReportedJobId)
                    ? "Job"
                    : !string.IsNullOrWhiteSpace(report.ReportedCompanyId)
                        ? "Company"
                        : "User",
                Reason: report.Reason,
                Description: report.Description,
                Evidence: report.Evidence,
                Status: report.Status,
                ResolutionNote: report.ResolutionNote,
                CreatedAt: report.CreatedAt,
                ResolvedAt: report.ResolvedAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping report to DTO: {ReportId}", report.Id);
            return null;
        }
    }

    private async Task SendAdminNotificationAsync(string reporterId, string reason)
    {
        try
        {
            var reporter = await _userRepo.GetByIdAsync(reporterId);
            var reporterName = reporter?.FullName ?? "Unknown User";

            // Get all admin users
            var admins = await _userRepo.GetByRoleAsync("Admin");

            foreach (var admin in admins)
            {
                var notification = new Notification
                {
                    UserId = admin.Id,
                    Title = "New Report Submitted",
                    Message = $"A new report has been submitted by {reporterName}. Reason: {reason}",
                    Type = "Admin",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationService.CreateAsync(notification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending admin notification");
            // Don't throw - notification failure shouldn't break report creation
        }
    }

    private async Task SendReportResolvedNotificationToReporterAsync(string reporterId, string resolutionNote)
    {
        try
        {
            var notification = new Notification
            {
                UserId = reporterId,
                Title = "Your Report Has Been Resolved",
                Message = $"Your report has been reviewed and resolved by admin. {resolutionNote}",
                Type = "ReportResolved",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationService.CreateAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reporter resolved notification");
        }
    }

    private async Task SendReportResolvedNotificationToReportedUserAsync(string targetUserId, string resolutionNote)
    {
        try
        {
            var notification = new Notification
            {
                UserId = targetUserId,
                Title = "Your account report has been resolved",
                Message = $"A report related to your account has been reviewed by admin. {resolutionNote}",
                Type = "ReportResolved",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationService.CreateAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reported-user resolved notification");
        }
    }

    private async Task SendReportRejectedNotificationAsync(string reporterId, string rejectionReason)
    {
        try
        {
            var notification = new Notification
            {
                UserId = reporterId,
                Title = "Your Report Has Been Rejected",
                Message = $"Your report has been reviewed and rejected. Reason: {rejectionReason}",
                Type = "ReportRejected",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationService.CreateAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending report rejected notification");
        }
    }

    private async Task SendUserBannedNotificationAsync(string userId, string reason)
    {
        try
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = "Account Suspended",
                Message = $"Your account has been suspended due to violation of our terms. Reason: {reason}",
                Type = "AccountBanned",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationService.CreateAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending user banned notification");
        }
    }
}
