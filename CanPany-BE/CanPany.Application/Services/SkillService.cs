using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Skill service implementation
/// </summary>
public class SkillService : ISkillService
{
    private readonly ISkillRepository _repo;
    private readonly ILogger<SkillService> _logger;

    public SkillService(
        ISkillRepository repo,
        ILogger<SkillService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Skill?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Skill ID cannot be null or empty", nameof(id));

            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting skill by ID: {SkillId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Skill>> GetAllAsync()
    {
        try
        {
            return await _repo.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all skills");
            throw;
        }
    }

    public async Task<IEnumerable<Skill>> GetByCategoryIdAsync(string categoryId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(categoryId))
                throw new ArgumentException("Category ID cannot be null or empty", nameof(categoryId));

            return await _repo.GetByCategoryIdAsync(categoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting skills by category ID: {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<Skill> CreateAsync(Skill skill)
    {
        try
        {
            if (skill == null)
                throw new ArgumentNullException(nameof(skill));

            skill.CreatedAt = DateTime.UtcNow;
            return await _repo.AddAsync(skill);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating skill");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(string id, Skill skill)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Skill ID cannot be null or empty", nameof(id));
            if (skill == null)
                throw new ArgumentNullException(nameof(skill));

            skill.Id = id;
            skill.MarkAsUpdated();
            await _repo.UpdateAsync(skill);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating skill: {SkillId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Skill ID cannot be null or empty", nameof(id));

            await _repo.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting skill: {SkillId}", id);
            throw;
        }
    }
}


