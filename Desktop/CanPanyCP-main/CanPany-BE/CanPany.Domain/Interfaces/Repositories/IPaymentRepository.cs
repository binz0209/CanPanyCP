using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Payment entity
/// </summary>
public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(string id);
    Task<IEnumerable<Payment>> GetByUserIdAsync(string userId);
    Task<IEnumerable<Payment>> GetByStatusAsync(string status);
    Task<Payment?> GetByVnpTxnRefAsync(string vnpTxnRef);
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<Payment> AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

