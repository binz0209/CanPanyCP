using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Category service interface
/// </summary>
public interface ICategoryService
{
    Task<Category?> GetByIdAsync(string id);
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category> CreateAsync(Category category);
    Task<bool> UpdateAsync(string id, Category category);
    Task<bool> DeleteAsync(string id);
}


