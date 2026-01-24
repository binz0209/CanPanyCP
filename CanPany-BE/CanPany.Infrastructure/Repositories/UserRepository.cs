using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _collection;
    private readonly ILogger<UserRepository>? _logger;

    public UserRepository(MongoDbContext context, ILogger<UserRepository>? logger = null)
    {
        _collection = context.Users;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        try
        {
            return await _collection.Find(u => u.Id == id).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting user by ID: {UserId}", id);
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            return await _collection.Find(_ => true).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting all users");
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(string role)
    {
        try
        {
            return await _collection.Find(u => u.Role == role).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting users by role: {Role}", role);
            throw;
        }
    }

    public async Task<User> AddAsync(User user)
    {
        return await _collection.InsertOneWithVerificationAsync(user, _logger, "User");
    }

    public async Task UpdateAsync(User user)
    {
        await _collection.ReplaceOneWithVerificationAsync(
            Builders<User>.Filter.Eq(u => u.Id, user.Id),
            user,
            _logger,
            "User");
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneWithVerificationAsync(
            Builders<User>.Filter.Eq(u => u.Id, id),
            _logger,
            "User",
            id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        try
        {
            var count = await _collection.CountDocumentsAsync(u => u.Id == id);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking if user exists. Id: {UserId}", id);
            throw;
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            var count = await _collection.CountDocumentsAsync(u => u.Email == email);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking if email exists. Email: {Email}", email);
            throw;
        }
    }
}

