using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// CV service interface
/// </summary>
public interface ICVService
{
    Task<CV?> GetByIdAsync(string id);
    Task<IEnumerable<CV>> GetByUserIdAsync(string userId);
    Task<CV?> GetDefaultByUserIdAsync(string userId);
    Task<CV> CreateAsync(CV cv);
    Task<bool> UpdateAsync(string id, CV cv);
    Task<bool> DeleteAsync(string id);
    Task SetAsDefaultAsync(string cvId, string userId);
}


