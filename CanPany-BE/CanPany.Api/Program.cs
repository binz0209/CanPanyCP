using CanPany.Infrastructure.Data;
using CanPany.Infrastructure.Repositories;
using CanPany.Infrastructure.Security.Encryption;
using CanPany.Infrastructure.Security.Hashing;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Services;
using CanPany.Application.Validators;
using CanPany.Infrastructure.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger, dispose: true);

// Add services to the container
builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<LoginRequestValidator>());
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

// Register Global Interceptors
builder.Services.AddGlobalInterceptors();

// Register FluentValidation
// FluentValidation registration
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

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

app.Run();
