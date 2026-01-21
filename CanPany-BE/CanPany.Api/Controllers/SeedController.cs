using CanPany.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly ILogger<SeedController> _logger;
    private readonly IOptions<MongoOptions> _mongoOptions;

    public SeedController(MongoDbContext context, IOptions<MongoOptions> mongoOptions, ILogger<SeedController> logger)
    {
        _context = context;
        _mongoOptions = mongoOptions;
        _logger = logger;
    }

    /// <summary>
    /// Seed database with initial data
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SeedDatabase([FromQuery] bool reset = false)
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            if (reset)
            {
                // Drop the whole database so seeding can run from a clean slate.
                // This avoids partial data preventing other seed steps (many seeders short-circuit if any data exists).
                var options = _mongoOptions.Value;
                if (string.IsNullOrWhiteSpace(options.ConnectionString))
                {
                    return StatusCode(500, new
                    {
                        Success = false,
                        Message = "MongoDB configuration is missing (MongoDB:ConnectionString)",
                        Timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogWarning("Reset requested: dropping MongoDB database {DatabaseName}", options.DatabaseName);
                var client = new MongoClient(options.ConnectionString);
                await client.DropDatabaseAsync(options.DatabaseName);
            }

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

