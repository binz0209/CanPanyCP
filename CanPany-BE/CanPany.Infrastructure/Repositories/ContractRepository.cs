using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly IMongoCollection<Contract> _collection;

    public ContractRepository(MongoDbContext context)
    {
        _collection = context.Contracts;
    }

    public async Task<Contract?> GetByIdAsync(string id)
    {
        return await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Contract>> GetByJobIdAsync(string jobId)
    {
        return await _collection.Find(c => c.JobId == jobId).ToListAsync();
    }

    public async Task<IEnumerable<Contract>> GetByCompanyIdAsync(string companyId)
    {
        return await _collection.Find(c => c.CompanyId == companyId).ToListAsync();
    }

    public async Task<IEnumerable<Contract>> GetByCandidateIdAsync(string candidateId)
    {
        return await _collection.Find(c => c.CandidateId == candidateId).ToListAsync();
    }

    public async Task<Contract?> GetByApplicationIdAsync(string applicationId)
    {
        return await _collection.Find(c => c.ApplicationId == applicationId).FirstOrDefaultAsync();
    }

    public async Task<Contract> AddAsync(Contract contract)
    {
        await _collection.InsertOneAsync(contract);
        return contract;
    }

    public async Task UpdateAsync(Contract contract)
    {
        contract.MarkAsUpdated();
        await _collection.ReplaceOneAsync(c => c.Id == contract.Id, contract);
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


