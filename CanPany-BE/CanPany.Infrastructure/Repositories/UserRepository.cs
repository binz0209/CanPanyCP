using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _collection;

    public UserRepository(MongoDbContext context)
    {
        _collection = context.Users;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _collection.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(string role)
    {
        return await _collection.Find(u => u.Role == role).ToListAsync();
    }

    public async Task<User> AddAsync(User user)
    {
        await _collection.InsertOneAsync(user);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        user.MarkAsUpdated();
        await _collection.ReplaceOneAsync(u => u.Id == user.Id, user);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(u => u.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(u => u.Id == id);
        return count > 0;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var count = await _collection.CountDocumentsAsync(u => u.Email == email);
        return count > 0;
    }
}

