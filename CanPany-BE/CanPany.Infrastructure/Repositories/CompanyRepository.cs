using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly IMongoCollection<Company> _collection;

    public CompanyRepository(MongoDbContext context)
    {
        _collection = context.Companies;
    }

    public async Task<Company?> GetByIdAsync(string id)
    {
        return await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Company?> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(c => c.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<Company>> GetByVerificationStatusAsync(string status)
    {
        return await _collection.Find(c => c.VerificationStatus == status).ToListAsync();
    }

    public async Task<Company> AddAsync(Company company)
    {
        await _collection.InsertOneAsync(company);
        return company;
    }

    public async Task UpdateAsync(Company company)
    {
        company.MarkAsUpdated();
        await _collection.ReplaceOneAsync(c => c.Id == company.Id, company);
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

