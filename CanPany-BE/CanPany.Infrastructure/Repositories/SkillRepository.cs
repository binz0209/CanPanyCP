using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class SkillRepository : ISkillRepository
{
    private readonly IMongoCollection<Skill> _collection;

    public SkillRepository(MongoDbContext context)
    {
        _collection = context.Skills;
    }

    public async Task<Skill?> GetByIdAsync(string id)
    {
        return await _collection.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Skill>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<Skill>> GetByCategoryIdAsync(string categoryId)
    {
        return await _collection.Find(s => s.CategoryId == categoryId).ToListAsync();
    }

    public async Task<Skill> AddAsync(Skill skill)
    {
        await _collection.InsertOneAsync(skill);
        return skill;
    }

    public async Task UpdateAsync(Skill skill)
    {
        skill.MarkAsUpdated();
        await _collection.ReplaceOneAsync(s => s.Id == skill.Id, skill);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(s => s.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(s => s.Id == id);
        return count > 0;
    }
}


