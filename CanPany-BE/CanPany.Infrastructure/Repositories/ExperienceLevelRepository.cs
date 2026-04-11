using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class ExperienceLevelRepository : IExperienceLevelRepository
{
    private readonly IMongoCollection<ExperienceLevel> _collection;
    private readonly ILogger<ExperienceLevelRepository> _logger;

    public ExperienceLevelRepository(MongoDbContext context, ILogger<ExperienceLevelRepository> logger)
    {
        _collection = context.ExperienceLevels;
        _logger = logger;
    }

    public async Task<ExperienceLevel?> GetByIdAsync(string id)
    {
        try
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting experience level with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ExperienceLevel>> GetAllAsync()
    {
        try
        {
            // Sort by Order for natural experience progression
            return await _collection.Find(_ => true)
                .SortBy(x => x.Order)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all experience levels");
            throw;
        }
    }

    public async Task<ExperienceLevel> AddAsync(ExperienceLevel level)
    {
        try
        {
            await _collection.InsertOneAsync(level);
            return level;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding experience level");
            throw;
        }
    }

    public async Task UpdateAsync(ExperienceLevel level)
    {
        try
        {
            await _collection.ReplaceOneAsync(x => x.Id == level.Id, level);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating experience level {Id}", level.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting experience level {Id}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        try
        {
            var count = await _collection.CountDocumentsAsync(x => x.Id == id);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking experience level existence {Id}", id);
            throw;
        }
    }
}
