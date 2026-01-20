using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly IMongoCollection<Project> _collection;

    public ProjectRepository(MongoDbContext context)
    {
        _collection = context.Projects;
    }

    public async Task<Project?> GetByIdAsync(string id)
    {
        return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByOwnerIdAsync(string ownerId)
    {
        return await _collection.Find(p => p.OwnerId == ownerId).ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByStatusAsync(string status)
    {
        return await _collection.Find(p => p.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Project>> SearchAsync(string? keyword, string? categoryId, List<string>? skillIds, decimal? minBudget, decimal? maxBudget)
    {
        var filterBuilder = Builders<Project>.Filter;
        var filters = new List<FilterDefinition<Project>>();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filters.Add(filterBuilder.Or(
                filterBuilder.Regex(p => p.Title, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                filterBuilder.Regex(p => p.Description, new MongoDB.Bson.BsonRegularExpression(keyword, "i"))
            ));
        }

        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            filters.Add(filterBuilder.Eq(p => p.CategoryId, categoryId));
        }

        if (skillIds != null && skillIds.Any())
        {
            filters.Add(filterBuilder.AnyIn(p => p.SkillIds, skillIds));
        }

        if (minBudget.HasValue)
        {
            filters.Add(filterBuilder.Gte(p => p.BudgetAmount, minBudget.Value));
        }

        if (maxBudget.HasValue)
        {
            filters.Add(filterBuilder.Lte(p => p.BudgetAmount, maxBudget.Value));
        }

        var filter = filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<Project> AddAsync(Project project)
    {
        await _collection.InsertOneAsync(project);
        return project;
    }

    public async Task UpdateAsync(Project project)
    {
        project.MarkAsUpdated();
        await _collection.ReplaceOneAsync(p => p.Id == project.Id, project);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(p => p.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(p => p.Id == id);
        return count > 0;
    }
}


