using CanPany.Infrastructure.Data;
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

    public SeedController(MongoDbContext context, ILogger<SeedController> logger)
    {
        _context = context;
        _logger = logger;
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

