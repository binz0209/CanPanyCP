using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Company entity
/// </summary>
public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(string id);
    Task<Company?> GetByUserIdAsync(string userId);
    Task<IEnumerable<Company>> GetAllAsync();
    Task<IEnumerable<Company>> GetByVerificationStatusAsync(string status);
    Task<Company> AddAsync(Company company);
    Task UpdateAsync(Company company);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

