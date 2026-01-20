using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Skill service interface
/// </summary>
public interface ISkillService
{
    Task<Skill?> GetByIdAsync(string id);
    Task<IEnumerable<Skill>> GetAllAsync();
    Task<IEnumerable<Skill>> GetByCategoryIdAsync(string categoryId);
    Task<Skill> CreateAsync(Skill skill);
    Task<bool> UpdateAsync(string id, Skill skill);
    Task<bool> DeleteAsync(string id);
}


