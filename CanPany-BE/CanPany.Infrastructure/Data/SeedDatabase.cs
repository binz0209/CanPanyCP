using Microsoft.Extensions.Configuration;

namespace CanPany.Infrastructure.Data;

/// <summary>
/// Standalone database seeder console application
/// </summary>
public class SeedDatabase
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== CanPany Database Seeder ===");
        Console.WriteLine();

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration["MongoDB:ConnectionString"];
        var databaseName = configuration["MongoDB:DatabaseName"];

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("ERROR: MongoDB connection string not found in configuration!");
            Console.WriteLine("Please check appsettings.json");
            return;
        }

        if (string.IsNullOrEmpty(databaseName))
        {
            databaseName = "CanPany";
        }

        Console.WriteLine($"Connection String: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
        Console.WriteLine($"Database Name: {databaseName}");
        Console.WriteLine();

        try
        {
            var seeder = DatabaseSeeder.Create(connectionString, databaseName);
            await seeder.SeedAsync();
            
            Console.WriteLine();
            Console.WriteLine("✅ Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"❌ ERROR: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }
}

