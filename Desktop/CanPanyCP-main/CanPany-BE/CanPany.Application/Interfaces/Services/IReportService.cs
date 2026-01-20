using CanPany.Application.DTOs;
using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Service interface for report management
/// </summary>
public interface IReportService
{
    Task<Report> CreateReportAsync(string reporterId, CreateReportDto dto);
    Task<Report?> GetReportByIdAsync(string id);
    Task<IEnumerable<Report>> GetReportsByReporterIdAsync(string reporterId);
    Task<IEnumerable<ReportDetailsDto>> GetAllReportsAsync(ReportFilterDto? filter = null);
    Task<ReportDetailsDto?> GetReportDetailsAsync(string id);
    Task<bool> ResolveReportAsync(string reportId, string adminId, string resolutionNote, bool banUser);
    Task<bool> RejectReportAsync(string reportId, string adminId, string rejectionReason);
}
