using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Location entity
/// </summary>
public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(string id);
    Task<IEnumerable<Location>> GetAllAsync();
    Task<Location> AddAsync(Location location);
    Task UpdateAsync(Location location);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}
