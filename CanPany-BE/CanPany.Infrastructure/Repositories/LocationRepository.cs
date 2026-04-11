using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly IMongoCollection<Location> _collection;
    private readonly ILogger<LocationRepository> _logger;

    public LocationRepository(MongoDbContext context, ILogger<LocationRepository> logger)
    {
        _collection = context.Locations;
        _logger = logger;
    }

    public async Task<Location?> GetByIdAsync(string id)
    {
        try
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Location>> GetAllAsync()
    {
        try
        {
            // Sort alphabetically by Name
            return await _collection.Find(_ => true)
                .SortBy(x => x.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all locations");
            throw;
        }
    }

    public async Task<Location> AddAsync(Location location)
    {
        try
        {
            await _collection.InsertOneAsync(location);
            return location;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding location");
            throw;
        }
    }

    public async Task UpdateAsync(Location location)
    {
        try
        {
            await _collection.ReplaceOneAsync(x => x.Id == location.Id, location);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating location {Id}", location.Id);
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
            _logger.LogError(ex, "Error deleting location {Id}", id);
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
            _logger.LogError(ex, "Error checking location existence {Id}", id);
            throw;
        }
    }
}
