using CanPany.Application.Interfaces;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

public class ExperienceLevelService : IExperienceLevelService
{
    private readonly IExperienceLevelRepository _repository;
    private readonly ILogger<ExperienceLevelService> _logger;

    public ExperienceLevelService(IExperienceLevelRepository repository, ILogger<ExperienceLevelService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ExperienceLevel?> GetByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<ExperienceLevel>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<ExperienceLevel> CreateAsync(string name, int order)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("ExperienceLevel name cannot be empty", nameof(name));

        var level = new ExperienceLevel
        {
            Name = name.Trim(),
            Order = order
        };

        var created = await _repository.AddAsync(level);
        _logger.LogInformation("ExperienceLevel created: {Id} - {Name} (Order: {Order})", created.Id, created.Name, created.Order);
        return created;
    }

    public async Task UpdateAsync(string id, string name, int order)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new Exception("ExperienceLevel not found");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("ExperienceLevel name cannot be empty", nameof(name));

        existing.Name = name.Trim();
        existing.Order = order;
        await _repository.UpdateAsync(existing);
        _logger.LogInformation("ExperienceLevel updated: {Id} - {Name} (Order: {Order})", existing.Id, existing.Name, existing.Order);
    }

    public async Task DeleteAsync(string id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new Exception("ExperienceLevel not found");

        await _repository.DeleteAsync(id);
        _logger.LogInformation("ExperienceLevel deleted: {Id}", id);
    }
}
