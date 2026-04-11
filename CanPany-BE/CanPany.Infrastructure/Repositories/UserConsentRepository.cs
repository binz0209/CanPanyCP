using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class UserConsentRepository : IUserConsentRepository
{
    private readonly IMongoCollection<UserConsent> _collection;

    public UserConsentRepository(MongoDbContext context)
    {
        _collection = context.UserConsents;
    }

    public async Task<UserConsent?> GetByIdAsync(string id)
    {
        return await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UserConsent>> GetByUserIdAsync(string userId)
    {
        return await _collection
            .Find(c => c.UserId == userId)
            .SortByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserConsent?> GetByUserAndTypeAsync(string userId, string consentType)
    {
        return await _collection
            .Find(c => c.UserId == userId && c.ConsentType == consentType)
            .FirstOrDefaultAsync();
    }

    public async Task<UserConsent> AddAsync(UserConsent consent)
    {
        await _collection.InsertOneAsync(consent);
        return consent;
    }

    public async Task UpdateAsync(UserConsent consent)
    {
        consent.MarkAsUpdated();
        await _collection.ReplaceOneAsync(c => c.Id == consent.Id, consent);
    }

    public async Task<bool> HasConsentAsync(string userId, string consentType)
    {
        var consent = await GetByUserAndTypeAsync(userId, consentType);
        return consent is { IsGranted: true };
    }
}
