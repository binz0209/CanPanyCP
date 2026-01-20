using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class CandidateAlertRepository : ICandidateAlertRepository
{
    private readonly IMongoCollection<CandidateAlert> _collection;

    public CandidateAlertRepository(MongoDbContext context)
    {
        _collection = context.CandidateAlerts;
    }

    public async Task<CandidateAlert?> GetByIdAsync(string id)
    {
        return await _collection.Find(ca => ca.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CandidateAlert>> GetByCompanyIdAsync(string companyId)
    {
        return await _collection.Find(ca => ca.CompanyId == companyId).ToListAsync();
    }

    public async Task<IEnumerable<CandidateAlert>> GetActiveAlertsAsync(string companyId)
    {
        return await _collection.Find(ca => ca.CompanyId == companyId && ca.IsActive == true).ToListAsync();
    }

    public async Task<CandidateAlert> AddAsync(CandidateAlert candidateAlert)
    {
        await _collection.InsertOneAsync(candidateAlert);
        return candidateAlert;
    }

    public async Task UpdateAsync(CandidateAlert candidateAlert)
    {
        candidateAlert.MarkAsUpdated();
        await _collection.ReplaceOneAsync(ca => ca.Id == candidateAlert.Id, candidateAlert);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(ca => ca.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(ca => ca.Id == id);
        return count > 0;
    }
}

