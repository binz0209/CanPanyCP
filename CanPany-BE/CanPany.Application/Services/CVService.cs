using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// CV service implementation
/// </summary>
public class CVService : ICVService
{
    private readonly ICVRepository _repo;
    private readonly ILogger<CVService> _logger;

    public CVService(
        ICVRepository repo,
        ILogger<CVService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<CV?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("CV ID cannot be null or empty", nameof(id));

            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CV by ID: {CVId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<CV>> GetByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _repo.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CVs by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<CV?> GetDefaultByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _repo.GetDefaultByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default CV by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<CV> CreateAsync(CV cv)
    {
        try
        {
            if (cv == null)
                throw new ArgumentNullException(nameof(cv));

            cv.CreatedAt = DateTime.UtcNow;
            return await _repo.AddAsync(cv);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating CV");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(string id, CV cv)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("CV ID cannot be null or empty", nameof(id));
            if (cv == null)
                throw new ArgumentNullException(nameof(cv));

            // Id is already set, just update
            cv.MarkAsUpdated();
            await _repo.UpdateAsync(cv);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating CV: {CVId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("CV ID cannot be null or empty", nameof(id));

            await _repo.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting CV: {CVId}", id);
            throw;
        }
    }

    public async Task SetAsDefaultAsync(string cvId, string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cvId))
                throw new ArgumentException("CV ID cannot be null or empty", nameof(cvId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            await _repo.SetAsDefaultAsync(cvId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting CV as default: {CVId}, {UserId}", cvId, userId);
            throw;
        }
    }
}


