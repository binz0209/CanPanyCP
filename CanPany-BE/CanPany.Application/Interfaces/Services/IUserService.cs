using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// User service interface
/// </summary>
public interface IUserService
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> RegisterAsync(string fullName, string email, string password, string role = "Candidate");
    Task<User?> ValidateUserAsync(string email, string password);
    Task<bool> UpdateAsync(string id, User user);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<(bool Succeeded, string[] Errors)> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
    Task UpdatePasswordAsync(string userId, string newPassword);
}


