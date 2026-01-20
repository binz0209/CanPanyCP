using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;
using DomainApplication = CanPany.Domain.Entities.Application;

namespace CanPany.Infrastructure.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly IMongoCollection<DomainApplication> _collection;

    public ApplicationRepository(MongoDbContext context)
    {
        _collection = context.Applications;
    }

    public async Task<DomainApplication?> GetByIdAsync(string id)
    {
        return await _collection.Find(a => a.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<DomainApplication>> GetByJobIdAsync(string jobId)
    {
        return await _collection.Find(a => a.JobId == jobId).ToListAsync();
    }

    public async Task<IEnumerable<DomainApplication>> GetByCandidateIdAsync(string candidateId)
    {
        return await _collection.Find(a => a.CandidateId == candidateId).ToListAsync();
    }

    public async Task<DomainApplication?> GetByJobAndCandidateAsync(string jobId, string candidateId)
    {
        return await _collection.Find(a => a.JobId == jobId && a.CandidateId == candidateId).FirstOrDefaultAsync();
    }

    public async Task<DomainApplication> AddAsync(DomainApplication application)
    {
        await _collection.InsertOneAsync(application);
        return application;
    }

    public async Task UpdateAsync(DomainApplication application)
    {
        application.MarkAsUpdated();
        await _collection.ReplaceOneAsync(a => a.Id == application.Id, application);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(a => a.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(a => a.Id == id);
        return count > 0;
    }

    public async Task<bool> HasAppliedAsync(string jobId, string candidateId)
    {
        var count = await _collection.CountDocumentsAsync(a => a.JobId == jobId && a.CandidateId == candidateId);
        return count > 0;
    }

    public async Task<IEnumerable<DomainApplication>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }
}

