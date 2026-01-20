using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Banner entity
/// </summary>
public interface IBannerRepository
{
    Task<Banner?> GetByIdAsync(string id);
    Task<IEnumerable<Banner>> GetAllAsync();
    Task<IEnumerable<Banner>> GetActiveBannersAsync();
    Task<Banner> AddAsync(Banner banner);
    Task UpdateAsync(Banner banner);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}


