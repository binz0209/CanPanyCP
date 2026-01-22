using CanPany.Application.Interfaces.Interceptors;
using CanPany.Application.Interfaces.Services;
using CanPany.Infrastructure.Interceptors;
using CanPany.Infrastructure.Services;
using CanPany.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CanPany.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering global interceptors
/// </summary>
public static class InterceptorServiceExtensions
{
    /// <summary>
    /// Add global interceptors to DI container
    /// </summary>
    public static IServiceCollection AddGlobalInterceptors(this IServiceCollection services)
    {
        // Register I18N service first (required by interceptors)
        services.AddHttpContextAccessor();
        services.AddScoped<II18nService, I18nService>();
        
        // Register interceptor services
        services.AddSingleton<IDataMasker, DataMasker>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<IPerformanceMonitor, PerformanceMonitor>();
        services.AddScoped<ISecurityEventTracker, SecurityEventTracker>();
        services.AddScoped<IExceptionCapture, ExceptionCapture>();

        // Register background job interceptors
        // Note: JobExecutionFilter requires Hangfire - uncomment when Hangfire is installed
        // services.AddScoped<JobExecutionFilter>();
        services.AddScoped<HostedServiceInterceptor>();

        return services;
    }
    
    /// <summary>
    /// Use I18N middleware (should be called before UseGlobalAuditMiddleware)
    /// </summary>
    public static IApplicationBuilder UseI18nMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<I18nMiddleware>();
    }

    /// <summary>
    /// Use global audit middleware
    /// </summary>
    public static IApplicationBuilder UseGlobalAuditMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalAuditMiddleware>();
    }

    /// <summary>
    /// Register service interceptor for specific service
    /// </summary>
    public static IServiceCollection AddServiceWithInterceptor<TInterface, TImplementation>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        // Register implementation
        services.Add(new ServiceDescriptor(typeof(TImplementation), typeof(TImplementation), lifetime));

        // Register interceptor wrapper
        services.Add(new ServiceDescriptor(
            typeof(TInterface),
            sp =>
            {
                var implementation = sp.GetRequiredService<TImplementation>();
                return ServiceInterceptorFactory.CreateInterceptor<TInterface>(implementation, sp);
            },
            lifetime));

        return services;
    }

    /// <summary>
    /// Register Hangfire job filter
    /// Note: Requires Hangfire package. Uncomment JobExecutionFilter class when Hangfire is installed.
    /// </summary>
    public static IServiceCollection AddHangfireJobInterceptor(this IServiceCollection services)
    {
        services.AddScoped<JobExecutionFilter>();
        return services;
    }
}
