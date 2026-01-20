namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Admin dashboard service interface
/// </summary>
public interface IAdminDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalJobs { get; set; }
    public int TotalApplications { get; set; }
    public int TotalCompanies { get; set; }
    public int PendingVerifications { get; set; }
    public int PendingPayments { get; set; }
    public decimal TotalRevenue { get; set; }
}


