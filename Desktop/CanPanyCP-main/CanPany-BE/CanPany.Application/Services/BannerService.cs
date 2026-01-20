using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Banner service implementation
/// </summary>
public class BannerService : IBannerService
{
    private readonly IBannerRepository _repo;
    private readonly ILogger<BannerService> _logger;

    public BannerService(
        IBannerRepository repo,
        ILogger<BannerService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Banner?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Banner ID cannot be null or empty", nameof(id));

            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting banner by ID: {BannerId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Banner>> GetAllAsync()
    {
        try
        {
            return await _repo.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all banners");
            throw;
        }
    }

    public async Task<IEnumerable<Banner>> GetActiveBannersAsync()
    {
        try
        {
            return await _repo.GetActiveBannersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active banners");
            throw;
        }
    }

    public async Task<Banner> CreateAsync(Banner banner)
    {
        try
        {
            if (banner == null)
                throw new ArgumentNullException(nameof(banner));

            banner.CreatedAt = DateTime.UtcNow;
            return await _repo.AddAsync(banner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating banner");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(string id, Banner banner)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Banner ID cannot be null or empty", nameof(id));
            if (banner == null)
                throw new ArgumentNullException(nameof(banner));

            banner.Id = id;
            banner.MarkAsUpdated();
            await _repo.UpdateAsync(banner);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating banner: {BannerId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Banner ID cannot be null or empty", nameof(id));

            await _repo.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting banner: {BannerId}", id);
            throw;
        }
    }
}


