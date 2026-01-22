using CanPany.Infrastructure.Data;
using CanPany.Infrastructure.Repositories;
using CanPany.Infrastructure.Security.Encryption;
using CanPany.Infrastructure.Security.Hashing;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Services;
using CanPany.Application.Validators;
using CanPany.Infrastructure.Extensions;
using CanPany.Application.Common.SemanticSearch;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;
using Serilog;
using MongoDB.Driver;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger, dispose: true);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

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

// Register Application Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<ICVService, CVService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<IAIChatService, AIChatService>();
builder.Services.AddScoped<ICandidateSearchService, CandidateSearchService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IPremiumPackageService, PremiumPackageService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IFilterPresetService, FilterPresetService>();

// Semantic search helpers
builder.Services.AddSingleton<ITextEmbeddingService>(sp => new HashingTextEmbeddingService(dims: 256));

// Register Global Interceptors
builder.Services.AddGlobalInterceptors();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

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
