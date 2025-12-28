using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for ProjectSkill entity
/// </summary>
public interface IProjectSkillRepository
{
    Task<ProjectSkill?> GetByIdAsync(string id);
    Task<IEnumerable<ProjectSkill>> GetByProjectIdAsync(string projectId);
    Task<IEnumerable<ProjectSkill>> GetBySkillIdAsync(string skillId);
    Task<ProjectSkill> AddAsync(ProjectSkill projectSkill);
    Task UpdateAsync(ProjectSkill projectSkill);
    Task DeleteAsync(string id);
    Task DeleteByProjectIdAsync(string projectId);
    Task DeleteByProjectIdAndSkillIdAsync(string projectId, string skillId);
    Task<bool> ExistsAsync(string id);
}

