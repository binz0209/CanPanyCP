using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// User service implementation
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IHashService _hashService;
    private readonly IWalletService _walletService;
    private readonly IUserProfileService _profileService;
    private readonly ICompanyService _companyService;
    private readonly IBackgroundEmailService _backgroundEmailService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository repo,
        IHashService hashService,
        IWalletService walletService,
        IUserProfileService profileService,
        ICompanyService companyService,
        IBackgroundEmailService backgroundEmailService,
        ILogger<UserService> logger)
    {
        _repo = repo;
        _hashService = hashService;
        _walletService = walletService;
        _profileService = profileService;
        _companyService = companyService;
        _backgroundEmailService = backgroundEmailService;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("User ID cannot be null or empty", nameof(id));

            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            return await _repo.GetByEmailAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<User> RegisterAsync(string fullName, string email, string password, string role = "Candidate")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name is required", nameof(fullName));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required", nameof(password));

            var existing = await _repo.GetByEmailAsync(email);
            if (existing != null)
                throw new BusinessRuleViolationException("EmailExists", $"Email {email} already exists");

            // Tạo user
            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = _hashService.HashPassword(password),
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _repo.AddAsync(user);
            _logger.LogInformation("User registered successfully: {Email}, {UserId}", email, createdUser.Id);

            // Danh sách các entities đã tạo để rollback nếu cần
            var createdEntities = new List<(string Type, string Id)>();

            try
            {
                // 1. Tạo wallet cho user mới (BẮT BUỘC - tất cả user đều phải có wallet)
                var wallet = await _walletService.EnsureAsync(createdUser.Id);
                createdEntities.Add(("Wallet", wallet.Id));
                _logger.LogInformation("Wallet created for user: {UserId}, WalletId: {WalletId}", createdUser.Id, wallet.Id);

                // 2. Tạo user profile cho TẤT CẢ user (BẮT BUỘC - chứa thông tin cá nhân)
                var existingProfile = await _profileService.GetByUserIdAsync(createdUser.Id);
                if (existingProfile == null)
                {
                    var profile = new UserProfile
                    {
                        UserId = createdUser.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    var createdProfile = await _profileService.CreateAsync(profile);
                    createdEntities.Add(("UserProfile", createdProfile.Id));
                    _logger.LogInformation("User profile created for user: {UserId}, ProfileId: {ProfileId}", createdUser.Id, createdProfile.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating required entities for user: {UserId}", createdUser.Id);
                
                // Rollback: Xóa các entities đã tạo theo thứ tự ngược lại
                foreach (var (entityType, entityId) in createdEntities.AsEnumerable().Reverse())
                {
                    try
                    {
                        if (entityType == "Wallet")
                        {
                            // TODO: Implement DeleteAsync in WalletService if needed
                            _logger.LogWarning("Rollback: Cannot delete wallet {WalletId} (delete method not implemented)", entityId);
                        }
                        else if (entityType == "UserProfile")
                        {
                            // TODO: Implement DeleteAsync in UserProfileService if needed
                            _logger.LogWarning("Rollback: Cannot delete profile {ProfileId} (delete method not implemented)", entityId);
                        }
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.LogError(rollbackEx, "Error during rollback of {EntityType} {EntityId}", entityType, entityId);
                    }
                }

                // Xóa user cuối cùng
                try
                {
                    await _repo.DeleteAsync(createdUser.Id);
                    _logger.LogInformation("Rollback: User {UserId} deleted", createdUser.Id);
                }
                catch (Exception deleteEx)
                {
                    _logger.LogError(deleteEx, "Error deleting user during rollback: {UserId}", createdUser.Id);
                }

                throw new System.InvalidOperationException("Failed to create required entities (wallet, profile) for new user", ex);
            }

            // Tạo company nếu là Company role
            if (role == "Company")
            {
                try
                {
                    var existingCompany = await _companyService.GetByUserIdAsync(createdUser.Id);
                    if (existingCompany == null)
                    {
                        var company = new Company
                        {
                            UserId = createdUser.Id,
                            Name = fullName, // Tạm thời dùng fullName, user có thể update sau
                            VerificationStatus = "Pending",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _companyService.CreateAsync(company);
                        _logger.LogInformation("Company created for user: {UserId}", createdUser.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating company for user: {UserId}", createdUser.Id);
                    // Company không bắt buộc ngay lập tức, user có thể tạo sau
                    _logger.LogWarning("Company creation failed but continuing registration: {UserId}", createdUser.Id);
                }
            }

            // Queue welcome email asynchronously
            _backgroundEmailService.QueueWelcomeEmail(email, fullName);
            _logger.LogInformation("Welcome email queued for {Email}", email);

            return createdUser;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user with email: {Email}", email);
            throw new System.InvalidOperationException($"Failed to register user: {ex.Message}", ex);
        }
    }

    public async Task<User?> ValidateUserAsync(string email, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required", nameof(password));

            var user = await _repo.GetByEmailAsync(email);
            if (user == null) return null;

            try
            {
                var isValid = _hashService.VerifyPassword(password, user.PasswordHash);
                return isValid ? user : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error verifying password for user: {Email}", email);
                return null;
            }
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user with email: {Email}", email);
            throw new System.InvalidOperationException($"Failed to validate user: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateAsync(string id, User user)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("User ID cannot be null or empty", nameof(id));
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.Id = id;
            user.MarkAsUpdated();
            await _repo.UpdateAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("User ID cannot be null or empty", nameof(id));

            await _repo.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            return await _repo.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        try
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null)
                return (false, new[] { "User not found" });

            if (!_hashService.VerifyPassword(oldPassword, user.PasswordHash))
                return (false, new[] { "Old password is incorrect" });

            user.PasswordHash = _hashService.HashPassword(newPassword);
            user.MarkAsUpdated();
            await _repo.UpdateAsync(user);
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return (false, new[] { ex.Message });
        }
    }

    public async Task UpdatePasswordAsync(string userId, string newPassword)
    {
        try
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            user.PasswordHash = _hashService.HashPassword(newPassword);
            user.MarkAsUpdated();
            await _repo.UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user: {UserId}", userId);
            throw;
        }
    }
}

