using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Collaborative Filtering Service using Memory-based User-User kNN.
/// 
/// Algorithm:
/// 1. Build user-item matrix from interaction data
/// 2. Compute cosine similarity between target user and all other users
/// 3. Select k nearest neighbors (k=20)
/// 4. Predict score for target user-job pair using weighted avg of neighbors' ratings
/// 5. Normalize output to 0-100 scale
/// </summary>
public class CollaborativeFilteringService : ICollaborativeFilteringService
{
    private readonly IUserJobInteractionRepository _interactionRepo;
    private readonly ILogger<CollaborativeFilteringService> _logger;

    private const int K_NEIGHBORS = 20;

    public CollaborativeFilteringService(
        IUserJobInteractionRepository interactionRepo,
        ILogger<CollaborativeFilteringService> logger)
    {
        _interactionRepo = interactionRepo;
        _logger = logger;
    }

    public async Task<double> GetCfScoreAsync(string userId, string jobId)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(jobId))
            {
                _logger.LogWarning("GetCfScoreAsync called with empty userId or jobId: UserId={UserId}, JobId={JobId}", userId, jobId);
                return 0;
            }

            // 1. Get all interactions to build the user-item matrix
            var allInteractions = (await _interactionRepo.GetAllAsync()).ToList();

            if (!allInteractions.Any())
                return 0;

            // 2. Build user-item matrix: Dictionary<userId, Dictionary<jobId, maxScore>>
            var userItemMatrix = BuildUserItemMatrix(allInteractions);

            // If target user has no interactions, return 0 (cold start)
            if (!userItemMatrix.ContainsKey(userId))
                return 0;

            // 3. Get all unique job IDs for vector dimension (filter out null/empty)
            var allJobIds = allInteractions
                .Where(i => !string.IsNullOrWhiteSpace(i.JobId))
                .Select(i => i.JobId)
                .Distinct()
                .ToList();

            if (!allJobIds.Any())
                return 0;

            // 4. Find k nearest neighbors using cosine similarity
            var targetVector = BuildUserVector(userItemMatrix[userId], allJobIds);
            var neighbors = new List<(string UserId, double Similarity)>();

            foreach (var (otherUserId, otherRatings) in userItemMatrix)
            {
                if (otherUserId == userId || string.IsNullOrWhiteSpace(otherUserId)) continue;

                var otherVector = BuildUserVector(otherRatings, allJobIds);
                var similarity = CosineSimilarity(targetVector, otherVector);

                if (similarity > 0)
                    neighbors.Add((otherUserId, similarity));
            }

            // Sort by similarity and take top-K
            neighbors = neighbors
                .OrderByDescending(n => n.Similarity)
                .Take(K_NEIGHBORS)
                .ToList();

            if (!neighbors.Any())
                return 0;

            // 5. Predict score using weighted average
            double weightedSum = 0;
            double similaritySum = 0;

            foreach (var (neighborId, similarity) in neighbors)
            {
                if (userItemMatrix.TryGetValue(neighborId, out var neighborRatings) &&
                    neighborRatings.TryGetValue(jobId, out var neighborScore))
                {
                    weightedSum += similarity * neighborScore;
                    similaritySum += Math.Abs(similarity);
                }
            }

            if (similaritySum == 0)
                return 0;

            // Raw predicted score (in the scale of implicit scores: 1-5)
            var predictedScore = weightedSum / similaritySum;

            // Normalize to 0-100 scale (max implicit score is 5.0)
            return Math.Min(Math.Max(predictedScore / 5.0 * 100.0, 0), 100);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing CF score: {UserId} - {JobId}", userId, jobId);
            return 0;
        }
    }

    public async Task<Dictionary<string, double>> GetCfScoresForJobsAsync(string userId, IEnumerable<string> jobIds)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("GetCfScoresForJobsAsync called with empty userId");
                return new Dictionary<string, double>();
            }

            // Filter out null/empty jobIds and materialize the list
            var jobIdList = jobIds?
                .Where(j => !string.IsNullOrWhiteSpace(j))
                .Distinct()
                .ToList() ?? new List<string>();

            if (!jobIdList.Any())
            {
                _logger.LogWarning("GetCfScoresForJobsAsync called with no valid jobIds for user: {UserId}", userId);
                return new Dictionary<string, double>();
            }

            var result = new Dictionary<string, double>();

            // Get all interactions once for efficiency
            var allInteractions = (await _interactionRepo.GetAllAsync()).ToList();

            if (!allInteractions.Any())
            {
                foreach (var jobId in jobIdList)
                    result[jobId] = 0;
                return result;
            }

            // Build user-item matrix once
            var userItemMatrix = BuildUserItemMatrix(allInteractions);

            if (!userItemMatrix.ContainsKey(userId))
            {
                foreach (var jobId in jobIdList)
                    result[jobId] = 0;
                return result;
            }

            // Get all unique job IDs for vector dimension (filter out null/empty)
            var allJobIdsInMatrix = allInteractions
                .Where(i => !string.IsNullOrWhiteSpace(i.JobId))
                .Select(i => i.JobId)
                .Distinct()
                .ToList();

            if (!allJobIdsInMatrix.Any())
            {
                foreach (var jobId in jobIdList)
                    result[jobId] = 0;
                return result;
            }

            // Find neighbors once (shared across all job predictions)
            var targetVector = BuildUserVector(userItemMatrix[userId], allJobIdsInMatrix);
            var neighbors = new List<(string UserId, double Similarity)>();

            foreach (var (otherUserId, otherRatings) in userItemMatrix)
            {
                if (otherUserId == userId || string.IsNullOrWhiteSpace(otherUserId)) continue;

                var otherVector = BuildUserVector(otherRatings, allJobIdsInMatrix);
                var similarity = CosineSimilarity(targetVector, otherVector);

                if (similarity > 0)
                    neighbors.Add((otherUserId, similarity));
            }

            neighbors = neighbors
                .OrderByDescending(n => n.Similarity)
                .Take(K_NEIGHBORS)
                .ToList();

            _logger.LogInformation(
                "CF: User {UserId} has {NeighborCount} neighbors (from {TotalUsers} total users). Target user has {UserJobCount} job interactions. Total interactions in DB: {TotalInteractions}",
                userId, neighbors.Count, userItemMatrix.Count, userItemMatrix[userId].Count, allInteractions.Count);
            
            if (neighbors.Any())
            {
                _logger.LogInformation(
                    "CF: Top neighbors for user {UserId}: {TopNeighbors}",
                    userId, string.Join(", ", neighbors.Take(5).Select(n => $"{n.UserId}({n.Similarity:F3})")));
            }
            else
            {
                _logger.LogWarning(
                    "CF: User {UserId} has NO neighbors! This means no other users have similar interaction patterns. CF scores will be 0.",
                    userId);
            }

            // Predict for each job
            foreach (var jobId in jobIdList)
            {
                if (string.IsNullOrWhiteSpace(jobId))
                {
                    result[jobId] = 0;
                    continue;
                }

                if (!neighbors.Any())
                {
                    result[jobId] = 0;
                    continue;
                }

                double weightedSum = 0;
                double similaritySum = 0;

                foreach (var (neighborId, similarity) in neighbors)
                {
                    if (userItemMatrix.TryGetValue(neighborId, out var neighborRatings) &&
                        neighborRatings.TryGetValue(jobId, out var neighborScore))
                    {
                        weightedSum += similarity * neighborScore;
                        similaritySum += Math.Abs(similarity);
                    }
                }

                var predictedScore = similaritySum > 0 ? weightedSum / similaritySum : 0;
                var normalizedScore = Math.Min(Math.Max(predictedScore / 5.0 * 100.0, 0), 100);
                result[jobId] = normalizedScore;
                
                if (normalizedScore > 0)
                {
                    _logger.LogDebug(
                        "CF: User {UserId} - Job {JobId}: predicted={Predicted:F2}, normalized={Normalized:F2}, neighbors_with_job={NeighborsWithJob}",
                        userId, jobId, predictedScore, normalizedScore, 
                        neighbors.Count(n => userItemMatrix.TryGetValue(n.UserId, out var r) && r.ContainsKey(jobId)));
                }
            }

            _logger.LogInformation(
                "CF: Computed scores for user {UserId}: {JobCount} jobs, {NonZeroCount} with non-zero scores",
                userId, result.Count, result.Count(kvp => kvp.Value > 0));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing batch CF scores for user: {UserId}", userId);

            // Use materialized list instead of original IEnumerable to avoid potential enumeration issues
            var fallback = new Dictionary<string, double>();
            var jobIdList = jobIds?
                .Where(j => !string.IsNullOrWhiteSpace(j))
                .Distinct()
                .ToList() ?? new List<string>();
            
            foreach (var jobId in jobIdList)
                fallback[jobId] = 0;
            
            return fallback;
        }
    }

    #region Private Helpers

    /// <summary>
    /// Build user-item matrix. For multiple interactions of same user-job, keep max score.
    /// </summary>
    private static Dictionary<string, Dictionary<string, double>> BuildUserItemMatrix(
        List<UserJobInteraction> interactions)
    {
        var matrix = new Dictionary<string, Dictionary<string, double>>();

        foreach (var interaction in interactions)
        {
            // Skip interactions with null/empty userId or jobId
            if (string.IsNullOrWhiteSpace(interaction.UserId) || string.IsNullOrWhiteSpace(interaction.JobId))
                continue;

            if (!matrix.ContainsKey(interaction.UserId))
                matrix[interaction.UserId] = new Dictionary<string, double>();

            var userRatings = matrix[interaction.UserId];

            // Keep the max score for the same user-job pair
            if (!userRatings.ContainsKey(interaction.JobId) || userRatings[interaction.JobId] < interaction.Score)
                userRatings[interaction.JobId] = interaction.Score;
        }

        return matrix;
    }

    /// <summary>
    /// Build a user vector over all known job IDs (sparse → dense)
    /// </summary>
    private static double[] BuildUserVector(Dictionary<string, double> userRatings, List<string> allJobIds)
    {
        var vector = new double[allJobIds.Count];
        for (int i = 0; i < allJobIds.Count; i++)
        {
            vector[i] = userRatings.TryGetValue(allJobIds[i], out var score) ? score : 0;
        }
        return vector;
    }

    /// <summary>
    /// Compute cosine similarity between two vectors
    /// </summary>
    private static double CosineSimilarity(double[] a, double[] b)
    {
        if (a.Length != b.Length || a.Length == 0) return 0;

        double dotProduct = 0;
        double normA = 0;
        double normB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        var denominator = Math.Sqrt(normA) * Math.Sqrt(normB);
        return denominator == 0 ? 0 : dotProduct / denominator;
    }

    #endregion
}
