using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

/// <summary>
/// Repository for GitHub Analysis Results - RAG Layer Implementation
/// </summary>
public class GitHubAnalysisRepository : IGitHubAnalysisRepository
{
    private readonly IMongoCollection<GitHubAnalysisResult> _collection;
    private readonly ILogger<GitHubAnalysisRepository> _logger;

    public GitHubAnalysisRepository(
        MongoDbContext context,
        ILogger<GitHubAnalysisRepository> logger)
    {
        _collection = context.Database.GetCollection<GitHubAnalysisResult>("GitHubAnalysisResults");
        _logger = logger;

        // Create indexes for efficient queries
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        try
        {
            // Index on userId for fast user lookups
            var userIdIndex = Builders<GitHubAnalysisResult>.IndexKeys
                .Ascending(x => x.UserId);
            _collection.Indexes.CreateOne(new CreateIndexModel<GitHubAnalysisResult>(userIdIndex));

            // Index on gitHubUsername
            var usernameIndex = Builders<GitHubAnalysisResult>.IndexKeys
                .Ascending(x => x.GitHubUsername);
            _collection.Indexes.CreateOne(new CreateIndexModel<GitHubAnalysisResult>(usernameIndex));

            // Compound index for userId + isActive + analyzedAt
            var activeIndex = Builders<GitHubAnalysisResult>.IndexKeys
                .Ascending(x => x.UserId)
                .Descending(x => x.IsActive)
                .Descending(x => x.AnalyzedAt);
            _collection.Indexes.CreateOne(new CreateIndexModel<GitHubAnalysisResult>(activeIndex));

            // Text index on primarySkills for skill search
            var skillIndex = Builders<GitHubAnalysisResult>.IndexKeys
                .Text(x => x.PrimarySkills);
            _collection.Indexes.CreateOne(new CreateIndexModel<GitHubAnalysisResult>(skillIndex));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[GITHUB_ANALYSIS_REPO] Failed to create indexes (may already exist)");
        }
    }

    public async Task<GitHubAnalysisResult?> GetByIdAsync(string id)
    {
        try
        {
            return await _collection
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_ANALYSIS_REPO] Error getting analysis by ID: {Id}", id);
            throw;
        }
    }

    public async Task<GitHubAnalysisResult?> GetLatestByUserIdAsync(string userId)
    {
        try
        {
            return await _collection
                .Find(x => x.UserId == userId && x.IsActive)
                .SortByDescending(x => x.AnalyzedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_ANALYSIS_REPO] Error getting latest analysis for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<GitHubAnalysisResult>> GetByUserIdAsync(string userId, int limit = 10)
    {
        try
        {
            return await _collection
                .Find(x => x.UserId == userId)
                .SortByDescending(x => x.AnalyzedAt)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_ANALYSIS_REPO] Error getting analysis history for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<GitHubAnalysisResult?> GetByGitHubUsernameAsync(string gitHubUsername)
    {
        try
        {
            return await _collection
                .Find(x => x.GitHubUsername == gitHubUsername && x.IsActive)
                .SortByDescending(x => x.AnalyzedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_ANALYSIS_REPO] Error getting analysis by username: {Username}", gitHubUsername);
            throw;
        }
    }

    public async Task<GitHubAnalysisResult> AddAsync(GitHubAnalysisResult analysisResult)
    {
        try
        {
            analysisResult.CreatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(analysisResult);

            _logger.LogInformation(
                "[GITHUB_ANALYSIS_REPO] Added analysis: {Id} for user: {UserId}",
                analysisResult.Id,
                analysisResult.UserId);

            return analysisResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_ANALYSIS_REPO] Error adding analysis for user: {UserId}", analysisResult.UserId);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(GitHubAnalysisResult analysisResult)
    {
        try
        {
            analysisResult.UpdatedAt = DateTime.UtcNow;

            var result = await _collection.ReplaceOneAsync(
                x => x.Id == analysisResult.Id,
                analysisResult);

            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_ANALYSIS_REPO] Error updating analysis: {Id}", analysisResult.Id);
            throw;
        }
    }

    public async Task<bool> SetActiveAnalysisAsync(string userId, string newAnalysisId)
    {
        try
        {
            // Deactivate all existing analyses for this user
            var deactivateFilter = Builders<GitHubAnalysisResult>.Filter.Eq(x => x.UserId, userId);
            var deactivateUpdate = Builders<GitHubAnalysisResult>.Update
                .Set(x => x.IsActive, false)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            await _collection.UpdateManyAsync(deactivateFilter, deactivateUpdate);

            // Activate the new analysis
            var activateFilter = Builders<GitHubAnalysisResult>.Filter.Eq(x => x.Id, newAnalysisId);
            var activateUpdate = Builders<GitHubAnalysisResult>.Update
                .Set(x => x.IsActive, true)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(activateFilter, activateUpdate);

            _logger.LogInformation(
                "[GITHUB_ANALYSIS_REPO] Set active analysis: {AnalysisId} for user: {UserId}",
                newAnalysisId,
                userId);

            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_ANALYSIS_REPO] Error setting active analysis: {Id}", newAnalysisId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var result = await _collection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_ANALYSIS_REPO] Error deleting analysis: {Id}", id);
            throw;
        }
    }

    public async Task<List<GitHubAnalysisResult>> GetBySkillAsync(string skill, int limit = 50)
    {
        try
        {
            // Use text search on primarySkills
            var filter = Builders<GitHubAnalysisResult>.Filter.And(
                Builders<GitHubAnalysisResult>.Filter.Text(skill),
                Builders<GitHubAnalysisResult>.Filter.Eq(x => x.IsActive, true)
            );

            return await _collection
                .Find(filter)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_ANALYSIS_REPO] Error searching by skill: {Skill}", skill);
            return new List<GitHubAnalysisResult>();
        }
    }
}
