using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class UnlockRecordRepository : IUnlockRecordRepository
{
    private readonly IMongoCollection<UnlockRecord> _collection;

    public UnlockRecordRepository(MongoDbContext context)
    {
        _collection = context.UnlockRecords;
    }

    public async Task<UnlockRecord> AddAsync(UnlockRecord unlockRecord)
    {
        await _collection.InsertOneAsync(unlockRecord);
        return unlockRecord;
    }

    public async Task<bool> HasUnlockedAsync(string companyId, string candidateId)
    {
        var count = await _collection.CountDocumentsAsync(ur => ur.CompanyId == companyId && ur.CandidateId == candidateId);
        return count > 0;
    }

    public async Task<IEnumerable<UnlockRecord>> GetByCompanyIdAsync(string companyId, int page, int pageSize)
    {
        return await _collection.Find(ur => ur.CompanyId == companyId)
            .SortByDescending(ur => ur.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<UnlockRecord>> GetByCandidateIdAsync(string candidateId)
    {
        return await _collection.Find(ur => ur.CandidateId == candidateId).ToListAsync();
    }
}
