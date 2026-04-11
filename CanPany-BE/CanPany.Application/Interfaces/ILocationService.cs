using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces;

public interface ILocationService
{
    Task<Location?> GetByIdAsync(string id);
    Task<IEnumerable<Location>> GetAllAsync();
    Task<Location> CreateAsync(string name);
    Task UpdateAsync(string id, string name);
    Task DeleteAsync(string id);
}
