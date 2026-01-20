using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Company service interface
/// </summary>
public interface ICompanyService
{
    Task<Company?> GetByIdAsync(string id);
    Task<Company?> GetByUserIdAsync(string userId);
    Task<IEnumerable<Company>> GetAllAsync();
    Task<Company> CreateAsync(Company company);
    Task<bool> UpdateAsync(string id, Company company);
    Task<bool> DeleteAsync(string id);
}


