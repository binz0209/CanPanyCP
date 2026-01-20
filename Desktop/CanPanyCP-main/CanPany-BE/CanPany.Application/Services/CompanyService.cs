using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Company service implementation
/// </summary>
public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _repo;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(
        ICompanyRepository repo,
        ILogger<CompanyService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Company?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Company ID cannot be null or empty", nameof(id));

            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company by ID: {CompanyId}", id);
            throw;
        }
    }

    public async Task<Company?> GetByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _repo.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        try
        {
            return await _repo.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all companies");
            throw;
        }
    }

    public async Task<Company> CreateAsync(Company company)
    {
        try
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            company.CreatedAt = DateTime.UtcNow;
            return await _repo.AddAsync(company);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(string id, Company company)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Company ID cannot be null or empty", nameof(id));
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            // Id is already set, just update
            company.MarkAsUpdated();
            await _repo.UpdateAsync(company);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company: {CompanyId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Company ID cannot be null or empty", nameof(id));

            await _repo.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company: {CompanyId}", id);
            throw;
        }
    }
}


