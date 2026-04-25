using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Hybrid Recommendation Service combining Semantic Search (content-based) and Collaborative Filtering.
/// 
/// Hybrid Score = α × SemanticScore + (1-α) × CfScore
/// 
/// Cold start handling via adaptive alpha:
/// - interactions < 10:  α = 1.0 (pure semantic)
/// - interactions < 50:  α = 0.7
/// - interactions < 100: α = 0.5
/// - interactions >= 100: α = 0.3
/// </summary>
public class HybridRecommendationService : IHybridRecommendationService
{
    private readonly IJobRepository _jobRepo;
    private readonly IUserProfileRepository _profileRepo;
    private readonly IApplicationRepository _applicationRepo;
    private readonly IGeminiService _geminiService;
    private readonly ICollaborativeFilteringService _cfService;
    private readonly IInteractionTrackingService _interactionService;
    private readonly ICVRepository _cvRepo;
    private readonly IGitHubAnalysisRepository _githubAnalysisRepo;
    private readonly IRecommendationLogRepository _recLogRepo;
    private readonly ILogger<HybridRecommendationService> _logger;

    public HybridRecommendationService(
        IJobRepository jobRepo,
        IUserProfileRepository profileRepo,
        IApplicationRepository applicationRepo,
        IGeminiService geminiService,
        ICollaborativeFilteringService cfService,
        IInteractionTrackingService interactionService,
        ICVRepository cvRepo,
        IGitHubAnalysisRepository githubAnalysisRepo,
        IRecommendationLogRepository recLogRepo,
        ILogger<HybridRecommendationService> logger)
    {
        _jobRepo = jobRepo;
        _profileRepo = profileRepo;
        _applicationRepo = applicationRepo;
        _geminiService = geminiService;
        _cfService = cfService;
        _interactionService = interactionService;
        _cvRepo = cvRepo;
        _githubAnalysisRepo = githubAnalysisRepo;
        _recLogRepo = recLogRepo;
        _logger = logger;
    }

    public async Task<IEnumerable<(Job Job, double HybridScore)>> GetRecommendedJobsAsync(string userId, int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Enumerable.Empty<(Job, double)>();

            // 1. Get user profile for semantic search
            var profile = await _profileRepo.GetByUserIdAsync(userId);
            if (profile == null)
            {
                _logger.LogWarning("No profile found for user {UserId}, cannot generate recommendations", userId);
                return Enumerable.Empty<(Job, double)>();
            }

            // 1.1. Aggregate skills from CV and GitHub analysis
            var allSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            // Add profile skills
            if (profile.SkillIds != null && profile.SkillIds.Any())
            {
                foreach (var skill in profile.SkillIds)
                {
                    if (!string.IsNullOrWhiteSpace(skill))
                        allSkills.Add(skill);
                }
            }

            // Add CV extracted skills
            try
            {
                var cvs = await _cvRepo.GetByUserIdAsync(userId);
                foreach (var cv in cvs)
                {
                    if (cv.ExtractedSkills != null && cv.ExtractedSkills.Any())
                    {
                        foreach (var skill in cv.ExtractedSkills)
                        {
                            if (!string.IsNullOrWhiteSpace(skill))
                                allSkills.Add(skill);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load CV skills for user {UserId}", userId);
            }

            // Add GitHub analysis skills
            try
            {
                var githubAnalysis = await _githubAnalysisRepo.GetLatestByUserIdAsync(userId);
                if (githubAnalysis != null && githubAnalysis.PrimarySkills != null && githubAnalysis.PrimarySkills.Any())
                {
                    foreach (var skill in githubAnalysis.PrimarySkills)
                    {
                        if (!string.IsNullOrWhiteSpace(skill))
                            allSkills.Add(skill);
                    }
                    
                    // Also add languages as skills
                    if (githubAnalysis.LanguagePercentages != null && githubAnalysis.LanguagePercentages.Any())
                    {
                        foreach (var lang in githubAnalysis.LanguagePercentages.Keys)
                        {
                            if (!string.IsNullOrWhiteSpace(lang))
                                allSkills.Add(lang);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load GitHub analysis skills for user {UserId}", userId);
            }

            var aggregatedSkills = allSkills.ToList();
            _logger.LogInformation(
                "User {UserId} aggregated skills: Profile={ProfileCount}, CV={CvCount}, GitHub={GitHubCount}, Total={TotalCount}",
                userId,
                profile.SkillIds?.Count ?? 0,
                aggregatedSkills.Count - (profile.SkillIds?.Count ?? 0),
                aggregatedSkills.Count);

            // 2. Get all open jobs
            var openJobs = (await _jobRepo.GetByStatusAsync("Open")).ToList();
            if (!openJobs.Any())
                return Enumerable.Empty<(Job, double)>();

            // 3. Get user's already-applied job IDs to exclude
            var applications = await _applicationRepo.GetByCandidateIdAsync(userId);
            var appliedJobIds = new HashSet<string>(applications.Select(a => a.JobId));

            // Filter out already-applied jobs
            var candidateJobs = openJobs.Where(j => !appliedJobIds.Contains(j.Id)).ToList();
            if (!candidateJobs.Any())
                return Enumerable.Empty<(Job, double)>();

            // 4. Compute semantic scores using embeddings
            List<double>? profileEmbedding = profile.Embedding;

            if (profileEmbedding == null || !profileEmbedding.Any())
            {
                var profileText = BuildProfileText(profile, aggregatedSkills);
                try
                {
                    profileEmbedding = await _geminiService.GenerateEmbeddingAsync(profileText);
                    
                    if (profileEmbedding != null && profileEmbedding.Any())
                    {
                        // Cache the newly generated embedding
                        profile.Embedding = profileEmbedding;
                        profile.UpdatedAt = DateTime.UtcNow;
                        await _profileRepo.UpdateAsync(profile);
                        _logger.LogInformation("Generated and cached new embedding for user {UserId} with {SkillCount} aggregated skills", userId, aggregatedSkills.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate embedding for user {UserId}, using CF only", userId);
                }
            }
            else
            {
                // Re-generate embedding if skills have changed significantly
                var currentSkillCount = profile.SkillIds?.Count ?? 0;
                if (aggregatedSkills.Count > currentSkillCount * 1.5) // 50% more skills from CV/GitHub
                {
                    _logger.LogInformation("Re-generating embedding for user {UserId} due to new skills from CV/GitHub", userId);
                    var profileText = BuildProfileText(profile, aggregatedSkills);
                    try
                    {
                        profileEmbedding = await _geminiService.GenerateEmbeddingAsync(profileText);
                        if (profileEmbedding != null && profileEmbedding.Any())
                        {
                            profile.Embedding = profileEmbedding;
                            profile.UpdatedAt = DateTime.UtcNow;
                            await _profileRepo.UpdateAsync(profile);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to re-generate embedding for user {UserId}", userId);
                    }
                }
            }

            // 5. Get user's interaction history to boost similar jobs
            var userInteractions = await _interactionService.GetUserInteractionsAsync(userId);
            var interactedJobIds = userInteractions.Select(i => i.JobId).Distinct().ToList();
            var interactedJobs = new List<Job>();
            
            if (interactedJobIds.Any())
            {
                foreach (var jobId in interactedJobIds)
                {
                    var job = await _jobRepo.GetByIdAsync(jobId);
                    if (job != null && job.Status == "Open")
                    {
                        interactedJobs.Add(job);
                    }
                }
            }
            
            // Extract preferred categories and skills from interacted jobs
            var preferredCategories = interactedJobs
                .Where(j => !string.IsNullOrWhiteSpace(j.CategoryId))
                .GroupBy(j => j.CategoryId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key!)
                .Take(3)
                .ToHashSet();
            
            var preferredSkills = interactedJobs
                .Where(j => j.SkillIds != null && j.SkillIds.Any())
                .SelectMany(j => j.SkillIds!)
                .GroupBy(s => s)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(10)
                .ToHashSet();
            
            // Build aggregated embedding from interacted jobs for better matching
            List<double>? aggregatedJobEmbedding = null;
            if (interactedJobs.Any())
            {
                try
                {
                    var jobTexts = interactedJobs
                        .Where(j => !string.IsNullOrWhiteSpace(j.Title) || !string.IsNullOrWhiteSpace(j.Description))
                        .Select(j => $"{j.Title} {j.Description}")
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Take(5); // Use top 5 most recent interactions
                    
                    if (jobTexts.Any())
                    {
                        var aggregatedText = string.Join(" ", jobTexts);
                        aggregatedJobEmbedding = await _geminiService.GenerateEmbeddingAsync(aggregatedText);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate aggregated embedding from user interactions");
                }
            }

            // 6. Get interaction count for adaptive alpha
            var interactionCount = await _interactionService.GetUserInteractionCountAsync(userId);
            var alpha = CalculateAlpha(interactionCount);

            _logger.LogInformation(
                "Generating recommendations for user {UserId}: {JobCount} candidate jobs, {InteractionCount} interactions, {InteractedJobsCount} interacted jobs, α={Alpha}",
                userId, candidateJobs.Count, interactionCount, interactedJobs.Count, alpha);
            
            if (preferredCategories.Any() || preferredSkills.Any())
            {
                _logger.LogInformation(
                    "User preferences: {CategoryCount} categories, {SkillCount} skills",
                    preferredCategories.Count, preferredSkills.Count);
            }

            // 7. Get CF scores in batch
            var jobIds = candidateJobs.Select(j => j.Id).ToList();
            var cfScores = await _cfService.GetCfScoresForJobsAsync(userId, jobIds);
            
            var cfScoresWithValues = cfScores.Where(kvp => kvp.Value > 0).ToList();
            _logger.LogInformation(
                "CF scores: {TotalJobs} jobs, {NonZeroCount} with non-zero CF scores (max: {MaxCfScore:F2})",
                cfScores.Count, cfScoresWithValues.Count, 
                cfScoresWithValues.Any() ? cfScoresWithValues.Max(kvp => kvp.Value) : 0);

            // 8. Compute hybrid scores with content-based boost
            var scoredJobs = new List<(Job Job, double HybridScore)>();
            int jobsWithSemanticScore = 0;
            int jobsWithCfScore = 0;

            foreach (var job in candidateJobs)
            {
                double semanticScore = 0;
                double contentBoost = 0; // Boost based on user's interaction history

                // Compute semantic score via cosine similarity of embeddings
                if (profileEmbedding != null && profileEmbedding.Any())
                {
                    // Generate embedding for job if missing (Forced refresh once)
                    List<double>? jobEmbedding = null; // job.SkillEmbedding;
                    
                    if (jobEmbedding == null || !jobEmbedding.Any())
                    {
                        try
                        {
                            // Build job text from title and skills (exclude description for cleaner semantic matching)
                            var jobTextParts = new List<string>();
                            if (!string.IsNullOrWhiteSpace(job.Title)) jobTextParts.Add($"Role: {job.Title}");
                            
                            if (job.SkillIds != null && job.SkillIds.Any())
                                jobTextParts.Add($"Skills: {string.Join(", ", job.SkillIds)}");
                            
                            var jobText = string.Join(" | ", jobTextParts);
                            if (!string.IsNullOrWhiteSpace(jobText))
                            {
                                jobEmbedding = await _geminiService.GenerateEmbeddingAsync(jobText);
                                
                                // Cache the embedding
                                if (jobEmbedding != null && jobEmbedding.Any())
                                {
                                    job.SkillEmbedding = jobEmbedding;
                                    await _jobRepo.UpdateAsync(job);
                                    _logger.LogDebug("Generated and cached embedding for job {JobId}", job.Id);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to generate embedding for job {JobId}", job.Id);
                        }
                    }
                    
                    // Calculate semantic score if we have both embeddings
                    if (jobEmbedding != null && jobEmbedding.Any())
                    {
                        // Primary: Profile-job similarity
                        var similarity = CosineSimilarity(profileEmbedding, jobEmbedding);
                        
                        // Normalize 0.5-1.0 to 0-100 (Gemini embeddings can sometimes be around 0.6 for somewhat related concepts)
                        semanticScore = similarity <= 0.5 ? 0 : (similarity - 0.5) / 0.5 * 100;
                        semanticScore = Math.Min(100, Math.Max(0, semanticScore));
                        
                        // VIP PRO Boost: If user has interacted with similar jobs, boost this job
                        if (aggregatedJobEmbedding != null && aggregatedJobEmbedding.Any())
                        {
                            var interactionSimilarity = CosineSimilarity(aggregatedJobEmbedding, jobEmbedding);
                            var interactionScore = interactionSimilarity <= 0.5 ? 0 : (interactionSimilarity - 0.5) / 0.5 * 100;
                            // Boost jobs similar to what user has viewed/bookmarked (weight: 15%)
                            contentBoost += interactionScore * 0.15;
                        }
                        
                        if (semanticScore > 0) jobsWithSemanticScore++;
                    }
                }
                
                // VIP PRO: Content-based boost from interaction history
                if (interactedJobs.Any())
                {
                    // Category match boost
                    if (!string.IsNullOrWhiteSpace(job.CategoryId) && preferredCategories.Contains(job.CategoryId))
                    {
                        contentBoost += 10.0;
                    }
                    
                    // Skills overlap boost
                    if (job.SkillIds != null && job.SkillIds.Any())
                    {
                        var matchingSkills = job.SkillIds.Intersect(aggregatedSkills, StringComparer.OrdinalIgnoreCase).Count();
                        var totalJobSkills = job.SkillIds.Count;
                        if (totalJobSkills > 0)
                        {
                            var skillMatchRatio = (double)matchingSkills / totalJobSkills;
                            contentBoost += skillMatchRatio * 10.0;
                        }
                        
                        if (preferredSkills.Any())
                        {
                            var preferredMatch = job.SkillIds.Intersect(preferredSkills, StringComparer.OrdinalIgnoreCase).Count();
                            var totalPreferredSkills = preferredSkills.Count;
                            if (totalPreferredSkills > 0)
                            {
                                var preferredMatchRatio = (double)preferredMatch / totalPreferredSkills;
                                contentBoost += preferredMatchRatio * 10.0;
                            }
                        }
                    }
                    
                    // Direct similarity with interacted jobs
                    if (job.SkillEmbedding != null && job.SkillEmbedding.Any())
                    {
                        foreach (var interactedJob in interactedJobs.Take(3))
                        {
                            if (interactedJob.SkillEmbedding != null && interactedJob.SkillEmbedding.Any())
                            {
                                var similarity = CosineSimilarity(interactedJob.SkillEmbedding, job.SkillEmbedding);
                                var intScore = similarity <= 0.5 ? 0 : (similarity - 0.5) / 0.5 * 100;
                                var interaction = userInteractions.FirstOrDefault(i => i.JobId == interactedJob.Id);
                                var weight = interaction?.Type switch
                                {
                                    InteractionType.Bookmark => 0.15,
                                    InteractionType.Click => 0.10,
                                    InteractionType.View => 0.05,
                                    _ => 0.05
                                };
                                contentBoost += intScore * weight;
                            }
                        }
                    }
                }
                
                // Fallback: Enhanced skill matching using aggregated skills
                if (semanticScore == 0 && aggregatedSkills.Any() && job.SkillIds != null && job.SkillIds.Any())
                {
                    var matchingSkills = aggregatedSkills.Intersect(job.SkillIds, StringComparer.OrdinalIgnoreCase).Count();
                    var totalSkills = Math.Max(aggregatedSkills.Count, job.SkillIds.Count);
                    if (totalSkills > 0)
                    {
                        var skillMatchRatio = (double)matchingSkills / totalSkills;
                        semanticScore = skillMatchRatio * 60; // Max 60 for basic missing semantic embeddings
                    }
                }

                var cfScore = cfScores.TryGetValue(job.Id, out var cs) ? cs : 0;
                if (cfScore > 0) jobsWithCfScore++;

                // Hybrid fusion
                var hybridScore = CalculateHybridScore(semanticScore, cfScore, alpha);
                
                // Content boost (Max 30%)
                contentBoost = Math.Min(contentBoost, 30.0);
                
                // Multiply the scale by content boost to improve separation (e.g. 1.0 -> 1.3 scale)
                hybridScore = hybridScore * (1.0 + contentBoost / 100.0);
                
                // Final cap at randomly slightly below 100 to feel natural if perfectly maxed out
                hybridScore = Math.Min(hybridScore, 99.8);
                
                // Ensure minimum score to avoid all zeros
                if (hybridScore == 0 && semanticScore == 0 && cfScore == 0 && contentBoost == 0)
                {
                    hybridScore = 1.0; // Small base score
                }
                
                // Log first few jobs for debugging
                if (scoredJobs.Count < 3)
                {
                    _logger.LogDebug(
                        "Job {JobId}: semantic={Semantic:F2}, cf={Cf:F2}, alpha={Alpha:F2}, hybrid={Hybrid:F2}",
                        job.Id, semanticScore, cfScore, alpha, hybridScore);
                }

                scoredJobs.Add((job, hybridScore));
            }
            
            _logger.LogInformation(
                "Score calculation: {SemanticCount}/{Total} jobs have semantic scores, {CfCount}/{Total} jobs have CF scores",
                jobsWithSemanticScore, candidateJobs.Count, jobsWithCfScore, candidateJobs.Count);

            // 8. Sort by hybrid score and return top-N
            var topJobs = scoredJobs
                .OrderByDescending(s => s.HybridScore)
                .Take(limit)
                .ToList();
            
            _logger.LogInformation(
                "Recommendations for user {UserId}: {Count} jobs returned, score range: {Min:F2}-{Max:F2}",
                userId, topJobs.Count,
                topJobs.Any() ? topJobs.Min(j => j.HybridScore) : 0,
                topJobs.Any() ? topJobs.Max(j => j.HybridScore) : 0);
            
            // Log recommendation for audit trail (UC-50)
            try
            {
                var recLog = new RecommendationLog
                {
                    UserId = userId,
                    RecommendationType = "hybrid",
                    RecommendedJobIds = topJobs.Select(j => j.Job.Id).ToList(),
                    Scores = topJobs.Select((j, idx) => new RecommendationScore
                    {
                        JobId = j.Job.Id,
                        HybridScore = j.HybridScore,
                        SemanticScore = null, // Individual scores not tracked at this level
                        CfScore = cfScores.TryGetValue(j.Job.Id, out var cs2) ? cs2 : null,
                        Rank = idx + 1
                    }).ToList(),
                    AlphaUsed = alpha,
                    InputContext = "profile",
                    TotalCandidateJobs = candidateJobs.Count,
                    InteractionCount = interactionCount,
                    CreatedAt = DateTime.UtcNow
                };
                await _recLogRepo.AddAsync(recLog);
            }
            catch (Exception logEx)
            {
                // Never fail the recommendation because of logging
                _logger.LogWarning(logEx, "Failed to save recommendation log for user {UserId}", userId);
            }

            return topJobs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendations for user: {UserId}", userId);
            return Enumerable.Empty<(Job, double)>();
        }
    }

    #region Private Helpers

    /// <summary>
    /// Calculate adaptive alpha based on interaction count (cold start handling)
    /// </summary>
    public static double CalculateAlpha(long interactionCount)
    {
        return interactionCount switch
        {
            < 10 => 1.0,   // Pure semantic (cold start)
            < 50 => 0.7,
            < 100 => 0.5,
            _ => 0.3        // CF-heavy
        };
    }

    /// <summary>
    /// Compute hybrid score: α × semantic + (1-α) × CF
    /// </summary>
    public static double CalculateHybridScore(double semanticScore, double cfScore, double alpha)
    {
        return alpha * semanticScore + (1 - alpha) * cfScore;
    }

    /// <summary>
    /// Build text representation of user profile for embedding generation
    /// Includes skills from Profile, CV analysis, and GitHub analysis
    /// </summary>
    private static string BuildProfileText(UserProfile profile, List<string> aggregatedSkills)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(profile.Title))
            parts.Add($"Role: {profile.Title}");
        
        // Removed Bio and Experience to prevent aggressive dilution of skill vectors
        
        // Use aggregated skills (CV + GitHub + Profile) instead of just profile skills
        if (aggregatedSkills.Any())
        {
            parts.Add($"Skills: {string.Join(", ", aggregatedSkills)}");
        }
        else if (profile.SkillIds != null && profile.SkillIds.Any())
        {
            parts.Add($"Skills: {string.Join(", ", profile.SkillIds)}");
        }

        // Add languages from profile
        if (profile.Languages != null && profile.Languages.Any())
        {
            parts.Add($"Languages: {string.Join(", ", profile.Languages)}");
        }

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Cosine similarity between two embedding vectors
    /// </summary>
    private static double CosineSimilarity(List<double> a, List<double> b)
    {
        if (a.Count != b.Count || a.Count == 0) return 0;

        double dotProduct = 0;
        double normA = 0;
        double normB = 0;

        for (int i = 0; i < a.Count; i++)
        {
            dotProduct += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        var denominator = Math.Sqrt(normA) * Math.Sqrt(normB);
        return denominator == 0 ? 0 : dotProduct / denominator;
    }

    /// <summary>
    /// Calculate hybrid scores for a list of jobs for sorting search results
    /// </summary>
    public async Task<Dictionary<string, double>> CalculateScoresForJobsAsync(string userId, IEnumerable<Job> jobs)
    {
        var result = new Dictionary<string, double>();
        
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return result;

            var jobList = jobs.ToList();
            if (!jobList.Any())
                return result;

            // Get user profile
            var profile = await _profileRepo.GetByUserIdAsync(userId);
            if (profile == null)
            {
                _logger.LogDebug("No profile found for user {UserId}, returning zero scores", userId);
                foreach (var job in jobList)
                    result[job.Id] = 0;
                return result;
            }

            // Aggregate skills from CV and GitHub
            var allSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (profile.SkillIds != null && profile.SkillIds.Any())
            {
                foreach (var skill in profile.SkillIds)
                    if (!string.IsNullOrWhiteSpace(skill))
                        allSkills.Add(skill);
            }

            try
            {
                var cvs = await _cvRepo.GetByUserIdAsync(userId);
                foreach (var cv in cvs)
                {
                    if (cv.ExtractedSkills != null && cv.ExtractedSkills.Any())
                    {
                        foreach (var skill in cv.ExtractedSkills)
                            if (!string.IsNullOrWhiteSpace(skill))
                                allSkills.Add(skill);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load CV skills for user {UserId}", userId);
            }

            try
            {
                var githubAnalysis = await _githubAnalysisRepo.GetLatestByUserIdAsync(userId);
                if (githubAnalysis != null && githubAnalysis.PrimarySkills != null && githubAnalysis.PrimarySkills.Any())
                {
                    foreach (var skill in githubAnalysis.PrimarySkills)
                        if (!string.IsNullOrWhiteSpace(skill))
                            allSkills.Add(skill);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load GitHub analysis skills for user {UserId}", userId);
            }

            var aggregatedSkills = allSkills.ToList();

            // Get profile embedding
            List<double>? profileEmbedding = profile.Embedding;
            if (profileEmbedding == null || !profileEmbedding.Any())
            {
                var profileText = BuildProfileText(profile, aggregatedSkills);
                try
                {
                    profileEmbedding = await _geminiService.GenerateEmbeddingAsync(profileText);
                    if (profileEmbedding != null && profileEmbedding.Any())
                    {
                        profile.Embedding = profileEmbedding;
                        profile.UpdatedAt = DateTime.UtcNow;
                        await _profileRepo.UpdateAsync(profile);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate embedding for user {UserId}", userId);
                }
            }

            // Get CF scores
            var jobIds = jobList.Select(j => j.Id).ToList();
            var cfScores = await _cfService.GetCfScoresForJobsAsync(userId, jobIds);

            // Get interaction count for alpha
            var interactionCount = await _interactionService.GetUserInteractionCountAsync(userId);
            var alpha = CalculateAlpha(interactionCount);

            // Calculate scores for each job
            foreach (var job in jobList)
            {
                double semanticScore = 0;

                if (profileEmbedding != null && profileEmbedding.Any())
                {
                    var jobEmbedding = job.SkillEmbedding;
                    if (jobEmbedding != null && jobEmbedding.Any())
                    {
                        semanticScore = CosineSimilarity(profileEmbedding, jobEmbedding) * 100;
                    }
                }

                // Fallback: Basic skill matching
                if (semanticScore == 0 && aggregatedSkills.Any() && job.SkillIds != null && job.SkillIds.Any())
                {
                    var matchingSkills = aggregatedSkills.Intersect(job.SkillIds, StringComparer.OrdinalIgnoreCase).Count();
                    var totalSkills = Math.Max(aggregatedSkills.Count, job.SkillIds.Count);
                    if (totalSkills > 0)
                    {
                        semanticScore = (double)matchingSkills / totalSkills * 50;
                    }
                }

                var cfScore = cfScores.TryGetValue(job.Id, out var cs) ? cs : 0;
                var hybridScore = CalculateHybridScore(semanticScore, cfScore, alpha);
                
                result[job.Id] = hybridScore;
            }

            _logger.LogDebug(
                "Calculated scores for {JobCount} jobs for user {UserId}: {NonZeroCount} with non-zero scores",
                jobList.Count, userId, result.Count(kvp => kvp.Value > 0));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating scores for jobs for user {UserId}", userId);
            // Return zero scores on error
            foreach (var job in jobs)
                result[job.Id] = 0;
        }

        return result;
    }

    #endregion
}
