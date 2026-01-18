using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace CanPany.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly IMongoCollection<UserProfile> _collection;

    public UserProfileRepository(MongoDbContext context)
    {
        _collection = context.UserProfiles;
    }

    public async Task<UserProfile?> GetByIdAsync(string id)
    {
        return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<UserProfile?> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(p => p.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<UserProfile> AddAsync(UserProfile profile)
    {
        await _collection.InsertOneAsync(profile);
        return profile;
    }

    public async Task UpdateAsync(UserProfile profile)
    {
            profile.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(p => p.Id == profile.Id, profile);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(p => p.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(p => p.Id == id);
        return count > 0;
    }

    public async Task<IEnumerable<UserProfile>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<(UserProfile Profile, double Score)>> SearchByVectorAsync(List<double> vector, int limit = 20, double minScore = 0.5)
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
        
        // Use BsonDocument result mapping first
        var results = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

        return results.Select(doc => 
        {
            var score = doc["score"].AsDouble;
            // Deserialize the root document back to UserProfile
            var profile = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<UserProfile>(doc["root"].AsBsonDocument);
            return (profile, score);
        });
    }
}

