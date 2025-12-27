using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Premium package service interface
/// </summary>
public interface IPremiumPackageService
{
    Task<PremiumPackage?> GetByIdAsync(string id);
    Task<IEnumerable<PremiumPackage>> GetAllAsync();
    Task<PremiumPackage> CreateAsync(PremiumPackage package);
    Task<bool> UpdateAsync(string id, PremiumPackage package);
    Task<bool> UpdatePriceAsync(string id, long newPrice);
    Task<bool> DeleteAsync(string id);
}


