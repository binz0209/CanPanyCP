using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Category service implementation
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository repo,
        ILogger<CategoryService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Category?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Category ID cannot be null or empty", nameof(id));

            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by ID: {CategoryId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        try
        {
            return await _repo.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            throw;
        }
    }

    public async Task<Category> CreateAsync(Category category)
    {
        try
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            category.CreatedAt = DateTime.UtcNow;
            return await _repo.AddAsync(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(string id, Category category)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Category ID cannot be null or empty", nameof(id));
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            category.Id = id;
            category.MarkAsUpdated();
            await _repo.UpdateAsync(category);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category: {CategoryId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Category ID cannot be null or empty", nameof(id));

            await _repo.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
            throw;
        }
    }
}


