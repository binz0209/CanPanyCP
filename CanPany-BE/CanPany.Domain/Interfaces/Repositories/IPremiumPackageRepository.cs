using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for PremiumPackage entity
/// </summary>
public interface IPremiumPackageRepository
{
    Task<PremiumPackage?> GetByIdAsync(string id);
    Task<IEnumerable<PremiumPackage>> GetAllAsync();
    Task<IEnumerable<PremiumPackage>> GetActivePackagesAsync();
    Task<PremiumPackage> AddAsync(PremiumPackage package);
    Task UpdateAsync(PremiumPackage package);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}


