using CanPany.Infrastructure.Data;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly ILogger<SeedController> _logger;
    private readonly IJobRepository _jobRepo;
    private readonly IGeminiService _geminiService;
    private readonly IUserJobInteractionRepository _interactionRepo;
    private readonly IUserRepository _userRepo;

    public SeedController(
        MongoDbContext context, 
        ILogger<SeedController> logger,
        IJobRepository jobRepo,
        IGeminiService geminiService,
        IUserJobInteractionRepository interactionRepo,
        IUserRepository userRepo)
    {
        _context = context;
        _logger = logger;
        _jobRepo = jobRepo;
        _geminiService = geminiService;
        _interactionRepo = interactionRepo;
        _userRepo = userRepo;
    }

    /// <summary>
    /// Seed database with initial data
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SeedDatabase()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");
            var seeder = new DatabaseSeeder(_context);
            await seeder.SeedAsync();

            _logger.LogInformation("Database seeding completed successfully");
            
            // Get database info
            var database = _context.Users.Database;
            var collections = await database.ListCollectionNames().ToListAsync();
            var collectionCounts = new Dictionary<string, long>();
            
            foreach (var collectionName in collections)
            {
                var collection = database.GetCollection<MongoDB.Bson.BsonDocument>(collectionName);
                var count = await collection.CountDocumentsAsync(MongoDB.Bson.BsonDocument.Parse("{}"));
                collectionCounts[collectionName] = count;
            }
            
            return Ok(new
            {
                Success = true,
                Message = "Database seeded successfully",
                DatabaseName = database.DatabaseNamespace.DatabaseName,
                Collections = collectionCounts,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding database: {Message}", ex.Message);
            return StatusCode(500, new
            {
                Success = false,
                Message = "Error seeding database",
                Error = ex.Message,
                StackTrace = ex.StackTrace,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Generate embeddings for all jobs that don't have SkillEmbedding
    /// </summary>
    [HttpPost("generate-job-embeddings")]
    public async Task<IActionResult> GenerateJobEmbeddings()
    {
        try
        {
            _logger.LogInformation("Starting job embedding generation...");
            
            // Get all jobs without SkillEmbedding
            var allJobs = await _jobRepo.GetByStatusAsync("Open");
            var jobsWithoutEmbedding = allJobs
                .Where(j => j.SkillEmbedding == null || !j.SkillEmbedding.Any())
                .ToList();
            
            if (!jobsWithoutEmbedding.Any())
            {
                return Ok(new
                {
                    Success = true,
                    Message = "All jobs already have embeddings",
                    Processed = 0,
                    Failed = 0
                });
            }
            
            _logger.LogInformation("Found {Count} jobs without embeddings", jobsWithoutEmbedding.Count);
            
            int processed = 0;
            int failed = 0;
            
            foreach (var job in jobsWithoutEmbedding)
            {
                try
                {
                    // Build job text from title, description, and skills
                    var jobTextParts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(job.Title)) jobTextParts.Add(job.Title);
                    if (!string.IsNullOrWhiteSpace(job.Description)) jobTextParts.Add(job.Description);
                    if (job.SkillIds != null && job.SkillIds.Any())
                        jobTextParts.Add(string.Join(" ", job.SkillIds));
                    
                    var jobText = string.Join(" ", jobTextParts);
                    if (string.IsNullOrWhiteSpace(jobText))
                    {
                        _logger.LogWarning("Job {JobId} has no text to generate embedding", job.Id);
                        failed++;
                        continue;
                    }
                    
                    var embedding = await _geminiService.GenerateEmbeddingAsync(jobText);
                    
                    if (embedding != null && embedding.Any())
                    {
                        job.SkillEmbedding = embedding;
                        await _jobRepo.UpdateAsync(job);
                        processed++;
                        
                        if (processed % 10 == 0)
                        {
                            _logger.LogInformation("Processed {Processed}/{Total} jobs", processed, jobsWithoutEmbedding.Count);
                        }
                    }
                    else
                    {
                        failed++;
                        _logger.LogWarning("Failed to generate embedding for job {JobId}", job.Id);
                    }
                    
                    // Small delay to avoid rate limiting
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    failed++;
                    _logger.LogError(ex, "Error generating embedding for job {JobId}", job.Id);
                }
            }
            
            _logger.LogInformation("Job embedding generation completed. Processed: {Processed}, Failed: {Failed}", processed, failed);
            
            return Ok(new
            {
                Success = true,
                Message = $"Generated embeddings for {processed} jobs",
                Processed = processed,
                Failed = failed,
                Total = jobsWithoutEmbedding.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating job embeddings");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Error generating job embeddings",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Seed interactions for testing CF recommendations
    /// Creates interactions for multiple users to enable CF to find neighbors
    /// </summary>
    [HttpPost("seed-interactions")]
    public async Task<IActionResult> SeedInteractions()
    {
        try
        {
            _logger.LogInformation("Starting interaction seeding for CF testing...");
            
            // Get all candidates and jobs
            var candidates = await _userRepo.GetByRoleAsync("Candidate");
            var jobs = await _jobRepo.GetByStatusAsync("Open");
            
            if (!candidates.Any() || !jobs.Any())
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Need at least 1 candidate and 1 job to seed interactions"
                });
            }
            
            var candidateList = candidates.ToList();
            var jobList = jobs.ToList();
            var random = new Random();
            int created = 0;
            
            // Create interactions: each candidate interacts with 5-15 random jobs
            foreach (var candidate in candidateList)
            {
                var jobsToInteract = jobList
                    .OrderBy(_ => random.Next())
                    .Take(random.Next(5, 16))
                    .ToList();
                
                foreach (var job in jobsToInteract)
                {
                    // Random interaction type (View, Click, Bookmark)
                    var interactionType = (InteractionType)random.Next(1, 4);
                    
                    // Check if interaction already exists
                    var existing = await _interactionRepo.GetByUserJobAndTypeAsync(candidate.Id, job.Id, interactionType);
                    if (existing != null) continue;
                    
                    var interaction = new UserJobInteraction
                    {
                        UserId = candidate.Id,
                        JobId = job.Id,
                        Type = interactionType,
                        Score = interactionType switch
                        {
                            InteractionType.View => 1.0,
                            InteractionType.Click => 2.0,
                            InteractionType.Bookmark => 3.0,
                            InteractionType.Apply => 5.0,
                            _ => 1.0
                        },
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30))
                    };
                    
                    await _interactionRepo.AddAsync(interaction);
                    created++;
                }
            }
            
            _logger.LogInformation("Created {Count} interactions for {UserCount} users", created, candidateList.Count);
            
            return Ok(new
            {
                Success = true,
                Message = $"Created {created} interactions for {candidateList.Count} users",
                InteractionsCreated = created,
                Users = candidateList.Count,
                Jobs = jobList.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding interactions");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Error seeding interactions",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Check database connection
    /// </summary>
    [HttpGet("check")]
    public async Task<IActionResult> CheckConnection()
    {
        try
        {
            // Try to access database
            var database = _context.Users.Database;
            var collections = await database.ListCollectionNames().ToListAsync();

            return Ok(new
            {
                Success = true,
                Message = "Database connection successful",
                DatabaseName = database.DatabaseNamespace.DatabaseName,
                Collections = collections,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking database connection");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Database connection failed",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

