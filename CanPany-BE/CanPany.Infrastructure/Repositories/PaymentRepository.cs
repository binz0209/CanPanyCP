using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly IMongoCollection<Payment> _collection;

    public PaymentRepository(MongoDbContext context)
    {
        _collection = context.Payments;
    }

    public async Task<Payment?> GetByIdAsync(string id)
    {
        return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Payment>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(p => p.UserId == userId)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(string status)
    {
        return await _collection.Find(p => p.Status == status).ToListAsync();
    }

    public async Task<Payment?> GetByVnpTxnRefAsync(string vnpTxnRef)
    {
        return await _collection.Find(p => p.Vnp_TxnRef == vnpTxnRef).FirstOrDefaultAsync();
    }

    public async Task<Payment> AddAsync(Payment payment)
    {
        await _collection.InsertOneAsync(payment);
        return payment;
    }

    public async Task UpdateAsync(Payment payment)
    {
        payment.MarkAsUpdated();
        await _collection.ReplaceOneAsync(p => p.Id == payment.Id, payment);
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

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }
}

