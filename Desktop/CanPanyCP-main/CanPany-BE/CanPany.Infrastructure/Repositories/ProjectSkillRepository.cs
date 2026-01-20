using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class ProjectSkillRepository : IProjectSkillRepository
{
    private readonly IMongoCollection<ProjectSkill> _collection;

    public ProjectSkillRepository(MongoDbContext context)
    {
        _collection = context.ProjectSkills;
    }

    public async Task<ProjectSkill?> GetByIdAsync(string id)
    {
        return await _collection.Find(ps => ps.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ProjectSkill>> GetByProjectIdAsync(string projectId)
    {
        return await _collection.Find(ps => ps.ProjectId == projectId).ToListAsync();
    }

    public async Task<IEnumerable<ProjectSkill>> GetBySkillIdAsync(string skillId)
    {
        return await _collection.Find(ps => ps.SkillId == skillId).ToListAsync();
    }

    public async Task<ProjectSkill> AddAsync(ProjectSkill projectSkill)
    {
        await _collection.InsertOneAsync(projectSkill);
        return projectSkill;
    }

    public async Task UpdateAsync(ProjectSkill projectSkill)
    {
        projectSkill.MarkAsUpdated();
        await _collection.ReplaceOneAsync(ps => ps.Id == projectSkill.Id, projectSkill);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(ps => ps.Id == id);
    }

    public async Task DeleteByProjectIdAsync(string projectId)
    {
        await _collection.DeleteManyAsync(ps => ps.ProjectId == projectId);
    }

    public async Task DeleteByProjectIdAndSkillIdAsync(string projectId, string skillId)
    {
        await _collection.DeleteOneAsync(ps => ps.ProjectId == projectId && ps.SkillId == skillId);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(ps => ps.Id == id);
        return count > 0;
    }
}

