using CanPany.Application.Interfaces;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _repository;
    private readonly ILogger<LocationService> _logger;

    public LocationService(ILocationRepository repository, ILogger<LocationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Location?> GetByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Location>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Location> CreateAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Location name cannot be empty", nameof(name));

        var location = new Location
        {
            Name = name.Trim()
        };

        var created = await _repository.AddAsync(location);
        _logger.LogInformation("Location created: {Id} - {Name}", created.Id, created.Name);
        return created;
    }

    public async Task UpdateAsync(string id, string name)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new Exception("Location not found");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Location name cannot be empty", nameof(name));

        existing.Name = name.Trim();
        await _repository.UpdateAsync(existing);
        _logger.LogInformation("Location updated: {Id} - {Name}", existing.Id, existing.Name);
    }

    public async Task DeleteAsync(string id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new Exception("Location not found");

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Location deleted: {Id}", id);
    }
}
