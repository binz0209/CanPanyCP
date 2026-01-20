using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Admin dashboard service implementation
/// </summary>
public class AdminDashboardService : IAdminDashboardService
{
    private readonly IUserRepository _userRepo;
    private readonly IJobRepository _jobRepo;
    private readonly IApplicationRepository _applicationRepo;
    private readonly ICompanyRepository _companyRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly ILogger<AdminDashboardService> _logger;

    public AdminDashboardService(
        IUserRepository userRepo,
        IJobRepository jobRepo,
        IApplicationRepository applicationRepo,
        ICompanyRepository companyRepo,
        IPaymentRepository paymentRepo,
        ILogger<AdminDashboardService> logger)
    {
        _userRepo = userRepo;
        _jobRepo = jobRepo;
        _applicationRepo = applicationRepo;
        _companyRepo = companyRepo;
        _paymentRepo = paymentRepo;
        _logger = logger;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        try
        {
            var users = (await _userRepo.GetAllAsync()).ToList();
            var jobs = (await _jobRepo.GetAllAsync()).ToList();
            var applications = (await _applicationRepo.GetAllAsync()).ToList();
            var companies = (await _companyRepo.GetAllAsync()).ToList();
            var payments = (await _paymentRepo.GetByStatusAsync("Pending")).ToList();

            var verifiedCompanies = companies.Where(c => c.VerificationStatus == "Pending").ToList();

            var totalRevenue = (await _paymentRepo.GetByStatusAsync("Paid"))
                .Sum(p => (decimal)p.Amount / 100); // Convert from minor units to VND

            return new DashboardStatsDto
            {
                TotalUsers = users.Count(),
                TotalJobs = jobs.Count(),
                TotalApplications = applications.Count(),
                TotalCompanies = companies.Count(),
                PendingVerifications = verifiedCompanies.Count(),
                PendingPayments = payments.Count(),
                TotalRevenue = totalRevenue
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            throw;
        }
    }
}

