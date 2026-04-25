using CanPany.Domain.Entities;
using CanPany.Domain.Models;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly IMongoCollection<Job> _collection;

    public JobRepository(MongoDbContext context)
    {
        _collection = context.Jobs;
    }

    public async Task<Job?> GetByIdAsync(string id)
    {
        return await _collection.Find(j => j.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Job>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<Job>> GetByCompanyIdAsync(string companyId)
    {
        return await _collection.Find(j => j.CompanyId == companyId).ToListAsync();
    }

    public async Task<IEnumerable<Job>> GetByStatusAsync(string status)
    {
        return await _collection.Find(j => j.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Job>> SearchAsync(string? keyword, string? categoryId, List<string>? skillIds, decimal? minBudget, decimal? maxBudget)
    {
        var filterBuilder = Builders<Job>.Filter;
        var filters = new List<FilterDefinition<Job>>();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filters.Add(filterBuilder.Or(
                filterBuilder.Regex(j => j.Title, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                filterBuilder.Regex(j => j.Description, new MongoDB.Bson.BsonRegularExpression(keyword, "i"))
            ));
        }

        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            // Validate that categoryId is a valid ObjectId before using it
            if (ObjectId.TryParse(categoryId, out _))
            {
                filters.Add(filterBuilder.Eq(j => j.CategoryId, categoryId));
            }
            // If categoryId is not a valid ObjectId, skip this filter (don't throw error)
        }

        if (skillIds != null && skillIds.Any())
        {
            // Filter out invalid ObjectIds from skillIds
            var validSkillIds = skillIds.Where(id => !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _)).ToList();
            if (validSkillIds.Any())
            {
                filters.Add(filterBuilder.AnyIn(j => j.SkillIds, validSkillIds));
            }
        }

        if (minBudget.HasValue)
        {
            filters.Add(filterBuilder.Gte(j => j.BudgetAmount, minBudget.Value));
        }

        if (maxBudget.HasValue)
        {
            filters.Add(filterBuilder.Lte(j => j.BudgetAmount, maxBudget.Value));
        }

        var filter = filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<(long TotalCount, IEnumerable<Job> Jobs)> SearchPagedAsync(JobSearchParameters parameters)
    {
        var filterBuilder = Builders<Job>.Filter;
        var filters = new List<FilterDefinition<Job>>();

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            filters.Add(filterBuilder.Or(
                filterBuilder.Regex(j => j.Title, new MongoDB.Bson.BsonRegularExpression(parameters.Keyword, "i")),
                filterBuilder.Regex(j => j.Description, new MongoDB.Bson.BsonRegularExpression(parameters.Keyword, "i"))
            ));
        }

        if (!string.IsNullOrWhiteSpace(parameters.CategoryId))
        {
            if (ObjectId.TryParse(parameters.CategoryId, out _))
            {
                filters.Add(filterBuilder.Eq(j => j.CategoryId, parameters.CategoryId));
            }
        }

        if (parameters.SkillIds != null && parameters.SkillIds.Any())
        {
            var validSkillIds = parameters.SkillIds.Where(id => !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _)).ToList();
            if (validSkillIds.Any())
            {
                filters.Add(filterBuilder.AnyIn(j => j.SkillIds, validSkillIds));
            }
        }

        if (parameters.MinBudget.HasValue)
        {
            filters.Add(filterBuilder.Gte(j => j.BudgetAmount, parameters.MinBudget.Value));
        }

        if (parameters.MaxBudget.HasValue)
        {
            filters.Add(filterBuilder.Lte(j => j.BudgetAmount, parameters.MaxBudget.Value));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Level))
        {
            filters.Add(filterBuilder.Eq(j => j.Level, parameters.Level));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Location))
        {
            filters.Add(filterBuilder.Regex(j => j.Location, new MongoDB.Bson.BsonRegularExpression(parameters.Location, "i")));
        }

        if (!string.IsNullOrWhiteSpace(parameters.BudgetType))
        {
            filters.Add(filterBuilder.Eq(j => j.BudgetType, parameters.BudgetType));
        }

        if (parameters.IsRemote.HasValue)
        {
            filters.Add(filterBuilder.Eq(j => j.IsRemote, parameters.IsRemote.Value));
        }

        var filter = filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;

        var totalCount = await _collection.CountDocumentsAsync(filter);
        var jobs = await _collection.Find(filter)
            .SortByDescending(j => j.CreatedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Limit(parameters.PageSize)
            .ToListAsync();

        return (totalCount, jobs);
    }

    public async Task<Job> AddAsync(Job job)
    {
        await _collection.InsertOneAsync(job);
        return job;
    }

    public async Task UpdateAsync(Job job)
    {
        job.MarkAsUpdated();
        await _collection.ReplaceOneAsync(j => j.Id == job.Id, job);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(j => j.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(j => j.Id == id);
        return count > 0;
    }

    public async Task<IEnumerable<Job>> GetJobsCreatedAfterAsync(DateTime date)
    {
        return await _collection.Find(j => j.CreatedAt >= date)
            .SortByDescending(j => j.CreatedAt)
            .ToListAsync();
    }
}

