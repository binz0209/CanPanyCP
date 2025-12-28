using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using CanPany.Domain.Entities;
using CanPany.Shared.Common.Base;
using DomainApplication = CanPany.Domain.Entities.Application;

namespace CanPany.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    static MongoDbContext()
    {
        // Configure base class properties to be ignored for all entities
        if (!BsonClassMap.IsClassMapRegistered(typeof(EntityBase)))
        {
            BsonClassMap.RegisterClassMap<EntityBase>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.UnmapMember(c => c.Id);
                cm.UnmapMember(c => c.CreatedAt);
                cm.UnmapMember(c => c.UpdatedAt);
            });
        }
    }

    public MongoDbContext(IOptions<MongoOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        _database = client.GetDatabase(options.Value.DatabaseName);
    }

    // Collections
    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<UserProfile> UserProfiles => _database.GetCollection<UserProfile>("user_profiles");
    public IMongoCollection<UserSettings> UserSettings => _database.GetCollection<UserSettings>("user_settings");
    public IMongoCollection<Company> Companies => _database.GetCollection<Company>("companies");
    public IMongoCollection<Job> Jobs => _database.GetCollection<Job>("jobs");
    public IMongoCollection<Project> Projects => _database.GetCollection<Project>("projects");
    public IMongoCollection<Proposal> Proposals => _database.GetCollection<Proposal>("proposals");
    public IMongoCollection<Contract> Contracts => _database.GetCollection<Contract>("contracts");
    public IMongoCollection<CV> CVs => _database.GetCollection<CV>("cvs");
    public IMongoCollection<CVAnalysis> CVAnalyses => _database.GetCollection<CVAnalysis>("cv_analyses");
    public IMongoCollection<DomainApplication> Applications => _database.GetCollection<DomainApplication>("applications");
    public IMongoCollection<Message> Messages => _database.GetCollection<Message>("messages");
    public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("notifications");
    public IMongoCollection<Payment> Payments => _database.GetCollection<Payment>("payments");
    public IMongoCollection<Wallet> Wallets => _database.GetCollection<Wallet>("wallets");
    public IMongoCollection<WalletTransaction> WalletTransactions => _database.GetCollection<WalletTransaction>("wallet_transactions");
    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("categories");
    public IMongoCollection<Skill> Skills => _database.GetCollection<Skill>("skills");
    public IMongoCollection<ProjectSkill> ProjectSkills => _database.GetCollection<ProjectSkill>("project_skills");
    public IMongoCollection<Review> Reviews => _database.GetCollection<Review>("reviews");
    public IMongoCollection<Banner> Banners => _database.GetCollection<Banner>("banners");
    public IMongoCollection<AuditLog> AuditLogs => _database.GetCollection<AuditLog>("audit_logs");
    public IMongoCollection<PremiumPackage> PremiumPackages => _database.GetCollection<PremiumPackage>("premium_packages");
    public IMongoCollection<JobBookmark> JobBookmarks => _database.GetCollection<JobBookmark>("job_bookmarks");
    public IMongoCollection<Report> Reports => _database.GetCollection<Report>("reports");
    public IMongoCollection<JobAlert> JobAlerts => _database.GetCollection<JobAlert>("job_alerts");
    public IMongoCollection<CandidateAlert> CandidateAlerts => _database.GetCollection<CandidateAlert>("candidate_alerts");
    public IMongoCollection<FilterPreset> FilterPresets => _database.GetCollection<FilterPreset>("filter_presets");
}

public class MongoOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "CanPany";
}

