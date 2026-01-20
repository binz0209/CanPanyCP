using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace CanPany.Infrastructure.Repositories;

/// <summary>
/// Extension methods for MongoDB operations with error handling and logging
/// </summary>
public static class MongoRepositoryExtensions
{
    /// <summary>
    /// Insert entity with error handling, Id verification, and logging
    /// </summary>
    public static async Task<T> InsertOneWithVerificationAsync<T>(
        this IMongoCollection<T> collection,
        T entity,
        ILogger? logger = null,
        string? entityName = null)
    {
        try
        {
            // Get Id property using reflection
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
            {
                throw new InvalidOperationException($"Entity {typeof(T).Name} does not have an Id property");
            }

            // Ensure Id is empty for MongoDB to auto-generate ObjectId
            var currentId = idProperty.GetValue(entity) as string;
            if (string.IsNullOrWhiteSpace(currentId))
            {
                idProperty.SetValue(entity, string.Empty);
            }

            await collection.InsertOneAsync(entity);
            
            // Verify that Id was set by MongoDB
            var newId = idProperty.GetValue(entity) as string;
            if (string.IsNullOrWhiteSpace(newId))
            {
                var name = entityName ?? typeof(T).Name;
                logger?.LogError("Entity {EntityName} Id was not set after MongoDB insert", name);
                throw new InvalidOperationException($"Failed to save {name} to MongoDB: Id was not generated");
            }

            logger?.LogInformation("{EntityName} saved to MongoDB successfully. Id: {EntityId}", 
                entityName ?? typeof(T).Name, newId);
            return entity;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            var name = entityName ?? typeof(T).Name;
            var idProperty = typeof(T).GetProperty("Id");
            var entityId = idProperty?.GetValue(entity)?.ToString() ?? "unknown";
            logger?.LogError(ex, "Duplicate key error when inserting {EntityName}. Id: {EntityId}", name, entityId);
            throw new InvalidOperationException($"{name} with this key already exists", ex);
        }
        catch (MongoConnectionException ex)
        {
            var name = entityName ?? typeof(T).Name;
            logger?.LogError(ex, "MongoDB connection error when inserting {EntityName}", name);
            throw new InvalidOperationException($"Failed to connect to MongoDB: {ex.Message}", ex);
        }
        catch (MongoServerException ex)
        {
            var name = entityName ?? typeof(T).Name;
            logger?.LogError(ex, "MongoDB server error when inserting {EntityName}", name);
            throw new InvalidOperationException($"MongoDB server error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            var name = entityName ?? typeof(T).Name;
            var idProperty = typeof(T).GetProperty("Id");
            var entityId = idProperty?.GetValue(entity)?.ToString() ?? "unknown";
            logger?.LogError(ex, "Error saving {EntityName} to MongoDB. Id: {EntityId}", name, entityId);
            throw;
        }
    }

    /// <summary>
    /// Replace entity with error handling and logging
    /// </summary>
    public static async Task ReplaceOneWithVerificationAsync<T>(
        this IMongoCollection<T> collection,
        FilterDefinition<T> filter,
        T entity,
        ILogger? logger = null,
        string? entityName = null)
    {
        try
        {
            // Try to call MarkAsUpdated if available
            var markAsUpdatedMethod = typeof(T).GetMethod("MarkAsUpdated", BindingFlags.Public | BindingFlags.Instance);
            markAsUpdatedMethod?.Invoke(entity, null);

            var result = await collection.ReplaceOneAsync(filter, entity);
            
            if (result.MatchedCount == 0)
            {
                var name = entityName ?? typeof(T).Name;
                var idProperty = typeof(T).GetProperty("Id");
                var entityId = idProperty?.GetValue(entity)?.ToString() ?? "unknown";
                logger?.LogWarning("{EntityName} not found for update. Id: {EntityId}", name, entityId);
                throw new InvalidOperationException($"{name} with Id {entityId} not found");
            }

            var name2 = entityName ?? typeof(T).Name;
            var idProperty2 = typeof(T).GetProperty("Id");
            var entityId2 = idProperty2?.GetValue(entity)?.ToString() ?? "unknown";
            logger?.LogInformation("{EntityName} updated in MongoDB. Id: {EntityId}", name2, entityId2);
        }
        catch (MongoConnectionException ex)
        {
            var name = entityName ?? typeof(T).Name;
            logger?.LogError(ex, "MongoDB connection error when updating {EntityName}", name);
            throw new InvalidOperationException($"Failed to connect to MongoDB: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            var name = entityName ?? typeof(T).Name;
            var idProperty = typeof(T).GetProperty("Id");
            var entityId = idProperty?.GetValue(entity)?.ToString() ?? "unknown";
            logger?.LogError(ex, "Error updating {EntityName}. Id: {EntityId}", name, entityId);
            throw;
        }
    }

    /// <summary>
    /// Delete entity with error handling and logging
    /// </summary>
    public static async Task DeleteOneWithVerificationAsync<T>(
        this IMongoCollection<T> collection,
        FilterDefinition<T> filter,
        ILogger? logger = null,
        string? entityName = null,
        string? entityId = null)
    {
        try
        {
            var result = await collection.DeleteOneAsync(filter);
            
            if (result.DeletedCount == 0)
            {
                var name = entityName ?? typeof(T).Name;
                logger?.LogWarning("{EntityName} not found for deletion. Id: {EntityId}", name, entityId ?? "unknown");
            }
            else
            {
                var name = entityName ?? typeof(T).Name;
                logger?.LogInformation("{EntityName} deleted from MongoDB. Id: {EntityId}", name, entityId ?? "unknown");
            }
        }
        catch (MongoConnectionException ex)
        {
            var name = entityName ?? typeof(T).Name;
            logger?.LogError(ex, "MongoDB connection error when deleting {EntityName}", name);
            throw new InvalidOperationException($"Failed to connect to MongoDB: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            var name = entityName ?? typeof(T).Name;
            logger?.LogError(ex, "Error deleting {EntityName}. Id: {EntityId}", name, entityId ?? "unknown");
            throw;
        }
    }
}
