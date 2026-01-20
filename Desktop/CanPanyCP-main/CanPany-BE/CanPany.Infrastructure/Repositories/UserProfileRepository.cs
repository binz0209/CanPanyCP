using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly IMongoCollection<UserProfile> _collection;
    private readonly ILogger<UserProfileRepository>? _logger;

    public UserProfileRepository(MongoDbContext context, ILogger<UserProfileRepository>? logger = null)
    {
        _collection = context.UserProfiles;
        _logger = logger;
    }

    public async Task<UserProfile?> GetByIdAsync(string id)
    {
        try
    {
        return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting user profile by ID: {ProfileId}", id);
            throw;
        }
    }

    public async Task<UserProfile?> GetByUserIdAsync(string userId)
    {
        try
    {
        return await _collection.Find(p => p.UserId == userId).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting user profile by UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserProfile> AddAsync(UserProfile profile)
    {
        return await _collection.InsertOneWithVerificationAsync(profile, _logger, "UserProfile");
    }

    public async Task UpdateAsync(UserProfile profile)
    {
            profile.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneWithVerificationAsync(
            Builders<UserProfile>.Filter.Eq(p => p.Id, profile.Id),
            profile,
            _logger,
            "UserProfile");
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneWithVerificationAsync(
            Builders<UserProfile>.Filter.Eq(p => p.Id, id),
            _logger,
            "UserProfile",
            id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        try
    {
        var count = await _collection.CountDocumentsAsync(p => p.Id == id);
        return count > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking if user profile exists. Id: {ProfileId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<UserProfile>> GetAllAsync()
    {
        try
    {
        return await _collection.Find(_ => true).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting all user profiles");
            throw;
        }
    }
}

