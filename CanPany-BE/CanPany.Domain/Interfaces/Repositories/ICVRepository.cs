using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for CV entity
/// </summary>
public interface ICVRepository
{
    Task<CV?> GetByIdAsync(string id);
    Task<IEnumerable<CV>> GetByUserIdAsync(string userId);
    Task<CV?> GetDefaultByUserIdAsync(string userId);
    Task<CV> AddAsync(CV cv);
    Task UpdateAsync(CV cv);
    Task DeleteAsync(string id);
    Task SetAsDefaultAsync(string cvId, string userId);
    Task<bool> ExistsAsync(string id);
}

