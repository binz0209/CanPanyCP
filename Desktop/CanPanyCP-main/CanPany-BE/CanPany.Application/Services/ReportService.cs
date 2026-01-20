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
    private readonly INotificationService _notificationService;
    private readonly IAdminService _adminService;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IReportRepository reportRepo,
        IUserRepository userRepo,
        INotificationService notificationService,
        IAdminService adminService,
        ILogger<ReportService> logger)
    {
        _reportRepo = reportRepo;
        _userRepo = userRepo;
        _notificationService = notificationService;
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<Report> CreateReportAsync(string reporterId, CreateReportDto dto)
    {
        try
        {
            // Validate reported user exists
            var reportedUser = await _userRepo.GetByIdAsync(dto.ReportedUserId);
            if (reportedUser == null)
            {
                throw new ArgumentException("Reported user not found", nameof(dto.ReportedUserId));
            }

            // Create report entity
            var report = new Report
            {
                ReporterId = reporterId,
                ReportedUserId = dto.ReportedUserId,
                Reason = dto.Reason,
                Description = dto.Description,
                Evidence = dto.Evidence ?? new List<string>(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _reportRepo.AddAsync(report);

            // Send notification to all admins
            await SendAdminNotificationAsync(reporterId, dto.Reason);

            _logger.LogInformation("Report created: {ReportId} by {ReporterId} against {ReportedUserId}", 
                created.Id, reporterId, dto.ReportedUserId);

            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating report");
            throw;
        }
    }

    public async Task<Report?> GetReportByIdAsync(string id)
    {
        try
        {
            return await _reportRepo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report by ID: {ReportId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Report>> GetReportsByReporterIdAsync(string reporterId)
    {
        try
        {
            return await _reportRepo.GetByReporterIdAsync(reporterId);
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
            if (banUser && !string.IsNullOrEmpty(report.ReportedUserId))
            {
                var banned = await _adminService.BanUserAsync(report.ReportedUserId);
                if (banned)
                {
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
            await SendReportResolvedNotificationAsync(report.ReporterId, report.ReportedUserId ?? "Unknown", resolutionNote);

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
            var allUsers = await _userRepo.GetAllAsync();
            var admins = allUsers.Where(u => u.Role == "Admin");

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

    private async Task SendReportResolvedNotificationAsync(string reporterId, string reportedUserName, string resolutionNote)
    {
        try
        {
            var notification = new Notification
            {
                UserId = reporterId,
                Title = "Your Report Has Been Resolved",
                Message = $"Your report against {reportedUserName} has been reviewed and resolved. {resolutionNote}",
                Type = "ReportResolved",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationService.CreateAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending report resolved notification");
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
