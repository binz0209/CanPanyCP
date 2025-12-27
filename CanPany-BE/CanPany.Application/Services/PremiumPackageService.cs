using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Premium package service implementation
/// </summary>
public class PremiumPackageService : IPremiumPackageService
{
    private readonly IPremiumPackageRepository _repo;
    private readonly ILogger<PremiumPackageService> _logger;

    public PremiumPackageService(
        IPremiumPackageRepository repo,
        ILogger<PremiumPackageService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<PremiumPackage?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Package ID cannot be null or empty", nameof(id));

            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting package by ID: {PackageId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<PremiumPackage>> GetAllAsync()
    {
        try
        {
            return await _repo.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all packages");
            throw;
        }
    }

    public async Task<PremiumPackage> CreateAsync(PremiumPackage package)
    {
        try
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            package.CreatedAt = DateTime.UtcNow;
            return await _repo.AddAsync(package);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating package");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(string id, PremiumPackage package)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Package ID cannot be null or empty", nameof(id));
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            package.Id = id;
            package.MarkAsUpdated();
            await _repo.UpdateAsync(package);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating package: {PackageId}", id);
            throw;
        }
    }

    public async Task<bool> UpdatePriceAsync(string id, long newPrice)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Package ID cannot be null or empty", nameof(id));
            if (newPrice <= 0)
                throw new ArgumentException("Price must be greater than 0", nameof(newPrice));

            var package = await _repo.GetByIdAsync(id);
            if (package == null)
                return false;

            package.Price = newPrice;
            package.MarkAsUpdated();
            await _repo.UpdateAsync(package);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating package price: {PackageId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Package ID cannot be null or empty", nameof(id));

            await _repo.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting package: {PackageId}", id);
            throw;
        }
    }
}


