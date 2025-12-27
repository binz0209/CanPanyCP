using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Banner service interface
/// </summary>
public interface IBannerService
{
    Task<Banner?> GetByIdAsync(string id);
    Task<IEnumerable<Banner>> GetAllAsync();
    Task<IEnumerable<Banner>> GetActiveBannersAsync();
    Task<Banner> CreateAsync(Banner banner);
    Task<bool> UpdateAsync(string id, Banner banner);
    Task<bool> DeleteAsync(string id);
}


