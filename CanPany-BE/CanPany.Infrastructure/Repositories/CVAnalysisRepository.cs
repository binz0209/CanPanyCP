using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class CVAnalysisRepository : ICVAnalysisRepository
{
    private readonly IMongoCollection<CVAnalysis> _collection;

    public CVAnalysisRepository(MongoDbContext context)
    {
        _collection = context.CVAnalyses;
    }

    public async Task<CVAnalysis?> GetByIdAsync(string id)
    {
        return await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<CVAnalysis?> GetByCVIdAsync(string cvId)
    {
        return await _collection.Find(c => c.CVId == cvId).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CVAnalysis>> GetByCandidateIdAsync(string candidateId)
    {
        return await _collection.Find(c => c.CandidateId == candidateId).ToListAsync();
    }

    public async Task<CVAnalysis> AddAsync(CVAnalysis cvAnalysis)
    {
        await _collection.InsertOneAsync(cvAnalysis);
        return cvAnalysis;
    }

    public async Task UpdateAsync(CVAnalysis cvAnalysis)
    {
        cvAnalysis.MarkAsUpdated();
        await _collection.ReplaceOneAsync(c => c.Id == cvAnalysis.Id, cvAnalysis);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(c => c.Id == id);
        return count > 0;
    }
}

