using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Skill entity
/// </summary>
public interface ISkillRepository
{
    Task<Skill?> GetByIdAsync(string id);
    Task<IEnumerable<Skill>> GetAllAsync();
    Task<IEnumerable<Skill>> GetByCategoryIdAsync(string categoryId);
    Task<Skill> AddAsync(Skill skill);
    Task UpdateAsync(Skill skill);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}


