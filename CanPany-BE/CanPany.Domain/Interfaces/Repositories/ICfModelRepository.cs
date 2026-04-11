using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for CfModel entity
/// </summary>
public interface ICfModelRepository
{
    Task<CfModel?> GetByIdAsync(string id);
    Task<CfModel?> GetActiveModelAsync();
    Task<CfModel?> GetLatestByTypeAsync(string modelType);
    Task<IEnumerable<CfModel>> GetAllAsync();
    Task<int> GetNextVersionAsync();
    Task<CfModel> AddAsync(CfModel model);
    Task UpdateAsync(CfModel model);
    Task ArchiveAllActiveAsync();
}
