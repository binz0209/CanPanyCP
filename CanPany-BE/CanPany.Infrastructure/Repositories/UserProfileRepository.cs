using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CanPany.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly IMongoCollection<UserProfile> _collection;
    private readonly ILogger<UserProfileRepository>? _logger;

    public UserProfileRepository(MongoDbContext context, ILogger<UserProfileRepository>? logger = null)
    {
        _collection = context.UserProfiles;
        _logger = logger;
    }

    public async Task<UserProfile?> GetByIdAsync(string id)
    {
        try
    {
        return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting user profile by ID: {ProfileId}", id);
            throw;
        }
    }

    public async Task<UserProfile?> GetByUserIdAsync(string userId)
    {
        try
    {
        return await _collection.Find(p => p.UserId == userId).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting user profile by UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserProfile> AddAsync(UserProfile profile)
    {
        return await _collection.InsertOneWithVerificationAsync(profile, _logger, "UserProfile");
    }

    public async Task UpdateAsync(UserProfile profile)
    {
            profile.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneWithVerificationAsync(
            Builders<UserProfile>.Filter.Eq(p => p.Id, profile.Id),
            profile,
            _logger,
            "UserProfile");
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneWithVerificationAsync(
            Builders<UserProfile>.Filter.Eq(p => p.Id, id),
            _logger,
            "UserProfile",
            id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        try
    {
        var count = await _collection.CountDocumentsAsync(p => p.Id == id);
        return count > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking if user profile exists. Id: {ProfileId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<UserProfile>> GetAllAsync()
    {
        try
        {
            return await _collection.Find(_ => true).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting all user profiles");
            throw;
        }
    }

    public async Task<IEnumerable<UserProfile>> GetProfilesCreatedAfterAsync(DateTime date)
    {
        try
        {
            return await _collection.Find(p => p.CreatedAt >= date)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting user profiles created after {Date}", date);
            throw;
        }
    }
    public async Task<IEnumerable<UserProfile>> SearchByFiltersAsync(
        string? keyword = null,
        List<string>? skillIds = null,
        string? location = null,
        string? experience = null,
        decimal? minHourlyRate = null,
        decimal? maxHourlyRate = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var builder = Builders<UserProfile>.Filter;
            var filters = new List<FilterDefinition<UserProfile>>();

            // Keyword + Skill search combined with OR
            // A candidate matches if keyword appears in text fields OR they have matching skills
            var keywordOrSkillFilters = new List<FilterDefinition<UserProfile>>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var escapedKeyword = System.Text.RegularExpressions.Regex.Escape(keyword);
                var regex = new BsonRegularExpression(escapedKeyword, "i");
                keywordOrSkillFilters.Add(builder.Or(
                    builder.Regex(p => p.Title, regex),
                    builder.Regex(p => p.Bio, regex),
                    builder.Regex(p => p.Experience, regex),
                    builder.Regex(p => p.Education, regex),
                    builder.Regex(p => p.Location, regex)
                ));
            }

            if (skillIds != null && skillIds.Any())
            {
                keywordOrSkillFilters.Add(builder.AnyIn(p => p.SkillIds, skillIds));
            }

            // Combine keyword + skills with OR (candidate matches if text OR skill matches)
            if (keywordOrSkillFilters.Any())
            {
                filters.Add(builder.Or(keywordOrSkillFilters));
            }

            // Location filter (ANDed with above)
            if (!string.IsNullOrWhiteSpace(location))
            {
                var locationRegex = new BsonRegularExpression(location, "i");
                filters.Add(builder.Or(
                    builder.Regex(p => p.Location, locationRegex),
                    builder.Regex(p => p.Address, locationRegex)
                ));
            }

            // Experience filter
            if (!string.IsNullOrWhiteSpace(experience))
            {
                var expRegex = new BsonRegularExpression(experience, "i");
                filters.Add(builder.Regex(p => p.Experience, expRegex));
            }

            // Hourly rate range
            if (minHourlyRate.HasValue)
            {
                filters.Add(builder.Gte(p => p.HourlyRate, minHourlyRate.Value));
            }
            if (maxHourlyRate.HasValue)
            {
                filters.Add(builder.Lte(p => p.HourlyRate, maxHourlyRate.Value));
            }

            var combinedFilter = filters.Any()
                ? builder.And(filters)
                : builder.Empty;

            var results = await _collection
                .Find(combinedFilter)
                .SortByDescending(p => p.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            _logger?.LogInformation("[DB_SEARCH] Filter search returned {Count} results (page={Page})", results.Count, page);

            return results;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[DB_SEARCH] Error searching profiles by filters");
            throw;
        }
    }

    public async Task<IEnumerable<UserProfile>> GetBySkillIdsAsync(List<string> skillIds, int limit = 20)
    {
        try
        {
            var filter = Builders<UserProfile>.Filter.AnyIn(p => p.SkillIds, skillIds);
            return await _collection.Find(filter).Limit(limit).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[DB_SEARCH] Error getting profiles by skill IDs");
            throw;
        }
    }

    public async Task<IEnumerable<(UserProfile Profile, double Score)>> SearchByVectorAsync(List<double> vector, int limit = 20, double minScore = 0.0)
    {
        try
        {
            var vectorSearch = new BsonDocument
            {
                { "$vectorSearch", new BsonDocument
                    {
                        { "index", "vector_index" },
                        { "path", "embedding" },
                        { "queryVector", new BsonArray(vector) },
                        { "numCandidates", limit * 10 },
                        { "limit", limit }
                    }
                }
            };

            var project = new BsonDocument
            {
                { "$project", new BsonDocument
                    {
                        { "score", new BsonDocument("$meta", "vectorSearchScore") },
                        { "root", "$$ROOT" }
                    }
                }
            };

            var pipeline = new[] { vectorSearch, project };
            
            var results = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            _logger?.LogInformation("[VECTOR_SEARCH] Atlas $vectorSearch returned {Count} results (queryDim={Dim})", 
                results.Count, vector.Count);

            return results.Select(doc => 
            {
                var score = doc["score"].AsDouble;
                var profile = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<UserProfile>(doc["root"].AsBsonDocument);
                return (profile, score);
            });
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "[VECTOR_SEARCH_FALLBACK] Atlas vector search failed. Using in-memory cosine similarity (queryDim={Dim})", vector.Count);
            
            // Fallback: Manual similarity calculation for local testing
            // This is ONLY for small datasets/local testing as it loads all profiles
            var allProfiles = await _collection.Find(p => p.Embedding != null).ToListAsync();
            
            _logger?.LogInformation("[VECTOR_SEARCH_FALLBACK] Found {Count} profiles with embeddings", allProfiles.Count);
            
            var results = allProfiles
                .Select(p => (Profile: p, Score: CalculateCosineSimilarity(vector, p.Embedding!)))
                .Where(x => x.Score >= minScore)
                .OrderByDescending(x => x.Score)
                .Take(limit)
                .ToList();

            _logger?.LogInformation("[VECTOR_SEARCH_FALLBACK] Returned {Count} results after cosine filter (minScore={MinScore})", 
                results.Count, minScore);
                
            return results;
        }
    }

    private double CalculateCosineSimilarity(List<double> v1, List<double> v2)
    {
        if (v1 == null || v2 == null || v1.Count != v2.Count) return 0;
        double dotProduct = 0;
        double mag1 = 0;
        double mag2 = 0;
        for (int i = 0; i < v1.Count; i++)
        {
            dotProduct += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }
        if (mag1 == 0 || mag2 == 0) return 0;
        return dotProduct / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
    }
}

