using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class ProposalRepository : IProposalRepository
{
    private readonly IMongoCollection<Proposal> _collection;

    public ProposalRepository(MongoDbContext context)
    {
        _collection = context.Proposals;
    }

    public async Task<Proposal?> GetByIdAsync(string id)
    {
        return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Proposal>> GetByProjectIdAsync(string projectId)
    {
        return await _collection.Find(p => p.ProjectId == projectId).ToListAsync();
    }

    public async Task<IEnumerable<Proposal>> GetByFreelancerIdAsync(string freelancerId)
    {
        return await _collection.Find(p => p.FreelancerId == freelancerId).ToListAsync();
    }

    public async Task<Proposal?> GetByProjectAndFreelancerAsync(string projectId, string freelancerId)
    {
        return await _collection.Find(p => p.ProjectId == projectId && p.FreelancerId == freelancerId).FirstOrDefaultAsync();
    }

    public async Task<Proposal> AddAsync(Proposal proposal)
    {
        await _collection.InsertOneAsync(proposal);
        return proposal;
    }

    public async Task UpdateAsync(Proposal proposal)
    {
        proposal.MarkAsUpdated();
        await _collection.ReplaceOneAsync(p => p.Id == proposal.Id, proposal);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(p => p.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(p => p.Id == id);
        return count > 0;
    }
}


