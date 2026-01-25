using CanPany.Infrastructure.Data;
using CanPany.Infrastructure.Repositories;
using CanPany.Infrastructure.Security.Encryption;
using CanPany.Infrastructure.Security.Hashing;
using CanPany.Infrastructure.Services;
using CanPany.Infrastructure.Jobs;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Services;
using CanPany.Application.Validators;
using CanPany.Infrastructure.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;
using Serilog;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger, dispose: true);

// Add services to the container
builder.Services.AddControllers();

// Register FluentValidation (using new non-deprecated API)
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CanPany API",
        Version = "v1",
        Description = "AI-powered Recruitment Platform API"
    });
});

// Configure MongoDB
builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDbContext>();

// Configure Redis for Background Jobs
var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
{
    var config = StackExchange.Redis.ConfigurationOptions.Parse(redisConnection);
    config.AbortOnConnectFail = false;
    config.ConnectTimeout = 10000;
    config.SyncTimeout = 10000;
    return StackExchange.Redis.ConnectionMultiplexer.Connect(config);
});

// Register Job Producer (for enqueueing background jobs)
builder.Services.AddSingleton<CanPany.Worker.Infrastructure.Queue.IJobProducer, CanPany.Worker.Infrastructure.Queue.RedisJobProducer>();

// Register Job Progress Tracker (for querying job status)
builder.Services.AddSingleton<CanPany.Worker.Infrastructure.Progress.IJobProgressTracker, CanPany.Worker.Infrastructure.Progress.RedisJobProgressTracker>();

// Verify MongoDB connection on startup
var mongoOptions = builder.Configuration.GetSection("MongoDB").Get<MongoOptions>();
string? mongoHost = null;
if (mongoOptions != null && !string.IsNullOrEmpty(mongoOptions.ConnectionString))
{
    try
    {
        // Extract MongoDB host from connection string (mask password for security)
        var connectionString = mongoOptions.ConnectionString;
        if (connectionString.StartsWith("mongodb+srv://"))
        {
            // Format: mongodb+srv://user:password@host/database?options
            var atIndex = connectionString.IndexOf('@');
            var slashIndex = connectionString.IndexOf('/', atIndex > 0 ? atIndex : 0);
            if (atIndex > 0 && slashIndex > atIndex)
            {
                mongoHost = connectionString.Substring(0, atIndex + 1) + "***@" + connectionString.Substring(slashIndex);
            }
            else
            {
                mongoHost = connectionString.Substring(0, Math.Min(30, connectionString.Length)) + "...";
            }
        }
        else if (connectionString.StartsWith("mongodb://"))
        {
            // Format: mongodb://user:password@host:port/database?options
            var atIndex = connectionString.IndexOf('@');
            var slashIndex = connectionString.IndexOf('/', atIndex > 0 ? atIndex : 0);
            if (atIndex > 0 && slashIndex > atIndex)
            {
                mongoHost = connectionString.Substring(0, atIndex + 1) + "***@" + connectionString.Substring(slashIndex);
            }
            else
            {
                mongoHost = connectionString.Substring(0, Math.Min(30, connectionString.Length)) + "...";
            }
        }
        else
        {
            mongoHost = connectionString.Substring(0, Math.Min(30, connectionString.Length)) + "...";
        }

        var testClient = new MongoClient(mongoOptions.ConnectionString);
        var testDatabase = testClient.GetDatabase(mongoOptions.DatabaseName);
        // Ping the database to verify connection
        testClient.StartSession();
        Log.Information("✓ MongoDB connection verified successfully | Host: {MongoHost} | Database: {DatabaseName}", 
            mongoHost, mongoOptions.DatabaseName);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "✗ Failed to connect to MongoDB | Host: {MongoHost} | Database: {DatabaseName}", 
            mongoHost ?? "Unknown", mongoOptions.DatabaseName);
        throw;
    }
}
else
{
    Log.Warning("✗ MongoDB configuration is missing or incomplete");
}

// Configure Hangfire with MongoDB storage
if (mongoOptions != null && !string.IsNullOrEmpty(mongoOptions.ConnectionString))
{
    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseMongoStorage(mongoOptions.ConnectionString, mongoOptions.DatabaseName, new MongoStorageOptions
        {
            MigrationOptions = new MongoMigrationOptions
            {
                MigrationStrategy = new MigrateMongoMigrationStrategy(),
                BackupStrategy = new CollectionMongoBackupStrategy()
            },
            Prefix = "hangfire",
            CheckConnection = true,
            CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
        }));

    // Add Hangfire server if enabled
    var hangfireConfig = builder.Configuration.GetSection("Hangfire");
    if (hangfireConfig.GetValue<bool>("ServerEnabled"))
    {
        var workerCount = hangfireConfig.GetValue<int>("WorkerCount", 5);
        var queues = hangfireConfig.GetSection("Queues").Get<string[]>() ?? new[] { "default" };
        
        builder.Services.AddHangfireServer(options =>
        {
            options.WorkerCount = workerCount;
            options.Queues = queues;
        });
    }
}
else
{
    Log.Warning("⚠️  Hangfire not configured (MongoDB required)");
}

// Register Repositories
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IUserRepository, CanPany.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IUserProfileRepository, CanPany.Infrastructure.Repositories.UserProfileRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.ICompanyRepository, CanPany.Infrastructure.Repositories.CompanyRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IJobRepository, CanPany.Infrastructure.Repositories.JobRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IProjectRepository, CanPany.Infrastructure.Repositories.ProjectRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IProposalRepository, CanPany.Infrastructure.Repositories.ProposalRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IContractRepository, CanPany.Infrastructure.Repositories.ContractRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.ICVRepository, CanPany.Infrastructure.Repositories.CVRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IApplicationRepository, CanPany.Infrastructure.Repositories.ApplicationRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IMessageRepository, CanPany.Infrastructure.Repositories.MessageRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.INotificationRepository, CanPany.Infrastructure.Repositories.NotificationRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IPaymentRepository, CanPany.Infrastructure.Repositories.PaymentRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IWalletRepository, CanPany.Infrastructure.Repositories.WalletRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IWalletTransactionRepository, CanPany.Infrastructure.Repositories.WalletTransactionRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IJobBookmarkRepository, CanPany.Infrastructure.Repositories.JobBookmarkRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.ICategoryRepository, CanPany.Infrastructure.Repositories.CategoryRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.ISkillRepository, CanPany.Infrastructure.Repositories.SkillRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IBannerRepository, CanPany.Infrastructure.Repositories.BannerRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IPremiumPackageRepository, CanPany.Infrastructure.Repositories.PremiumPackageRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IAuditLogRepository, CanPany.Infrastructure.Repositories.AuditLogRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.ICVAnalysisRepository, CanPany.Infrastructure.Repositories.CVAnalysisRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IReviewRepository, CanPany.Infrastructure.Repositories.ReviewRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IProjectSkillRepository, CanPany.Infrastructure.Repositories.ProjectSkillRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IUserSettingsRepository, CanPany.Infrastructure.Repositories.UserSettingsRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IReportRepository, CanPany.Infrastructure.Repositories.ReportRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IJobAlertRepository, CanPany.Infrastructure.Repositories.JobAlertRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.ICandidateAlertRepository, CanPany.Infrastructure.Repositories.CandidateAlertRepository>();
builder.Services.AddScoped<CanPany.Domain.Interfaces.Repositories.IFilterPresetRepository, CanPany.Infrastructure.Repositories.FilterPresetRepository>();

// Register Security Services
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IHashService, HashService>();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
    };
});

// Configure Email Options
builder.Services.Configure<CanPany.Domain.Entities.EmailOptions>(builder.Configuration.GetSection("SendGrid"));

// Register Application Services
builder.Services.AddServiceWithInterceptor<IUserService, UserService>();
builder.Services.AddServiceWithInterceptor<IJobService, JobService>();
builder.Services.AddServiceWithInterceptor<ICVService, CVService>();
builder.Services.AddServiceWithInterceptor<ICompanyService, CompanyService>();
builder.Services.AddServiceWithInterceptor<IApplicationService, ApplicationService>();
builder.Services.AddServiceWithInterceptor<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddServiceWithInterceptor<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<IAIChatService, AIChatService>();
builder.Services.AddScoped<ICandidateSearchService, CandidateSearchService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IPremiumPackageService, PremiumPackageService>();
builder.Services.AddHttpClient<IGeminiService, GeminiService>();
builder.Services.AddServiceWithInterceptor<IEmailService, EmailService>();

// Register Job Alert and Matching Services
builder.Services.AddScoped<IJobAlertService, JobAlertService>();
builder.Services.AddScoped<IJobMatchingService, JobMatchingService>();

// Register Background Email Services
builder.Services.AddScoped<IBackgroundEmailService, BackgroundEmailService>();
builder.Services.AddScoped<EmailJobProcessor>();
builder.Services.AddScoped<JobMatchProcessor>();

// Register Global Interceptors
builder.Services.AddGlobalInterceptors();
builder.Services.AddHangfireJobInterceptor();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only use HTTPS redirection in Production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");

// Use Hangfire Dashboard (before authentication for development)
var hangfireDashboardConfig = app.Configuration.GetSection("Hangfire");
if (hangfireDashboardConfig.GetValue<bool>("DashboardEnabled"))
{
    var dashboardPath = hangfireDashboardConfig.GetValue<string>("DashboardPath") ?? "/hangfire";
    app.UseHangfireDashboard(dashboardPath, new DashboardOptions
    {
        Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
    });
}

// Use I18N Middleware first (to detect language from headers)
app.UseI18nMiddleware();

// Use Global Audit Middleware (before authentication to capture all requests)
app.UseGlobalAuditMiddleware();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Log server information before running
Log.Information("═══════════════════════════════════════════════════════════");
Log.Information("🚀 CanPany API Server Starting");
Log.Information("═══════════════════════════════════════════════════════════");

// Get server URLs from configuration or use defaults
var configuredUrls = builder.Configuration["Urls"];
if (!string.IsNullOrEmpty(configuredUrls))
{
    var urls = configuredUrls.Split(';');
    foreach (var url in urls)
    {
        Log.Information("📍 Server Host: {ServerUrl}", url.Trim());
    }
}
else
{
    // Default URLs from launchSettings.json
    Log.Information("📍 Server Host: http://localhost:5047");
    Log.Information("📍 Server Host: https://localhost:7011");
}

// Log MongoDB connection info
if (mongoOptions != null && !string.IsNullOrEmpty(mongoOptions.ConnectionString))
{
    Log.Information("🗄️  MongoDB Host: {MongoHost}", mongoHost ?? "Unknown");
    Log.Information("📦 Database: {DatabaseName}", mongoOptions.DatabaseName);
}
else
{
    Log.Warning("⚠️  MongoDB configuration not found");
}

Log.Information("🌍 Environment: {Environment}", app.Environment.EnvironmentName);
Log.Information("═══════════════════════════════════════════════════════════");

app.Run();
