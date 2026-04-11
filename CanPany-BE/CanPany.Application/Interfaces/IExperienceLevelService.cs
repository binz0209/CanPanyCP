using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces;

public interface IExperienceLevelService
{
    Task<ExperienceLevel?> GetByIdAsync(string id);
    Task<IEnumerable<ExperienceLevel>> GetAllAsync();
    Task<ExperienceLevel> CreateAsync(string name, int order);
    Task UpdateAsync(string id, string name, int order);
    Task DeleteAsync(string id);
}
