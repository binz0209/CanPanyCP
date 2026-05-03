using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for User entity
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetByRoleAsync(string role);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Atomically increment AiCvGenerationCount and return the NEW count.
    /// Uses MongoDB $inc to avoid race conditions.
    /// Returns -1 if user not found.
    /// </summary>
    Task<int> IncrementAiCvGenerationCountAsync(string userId);
}

