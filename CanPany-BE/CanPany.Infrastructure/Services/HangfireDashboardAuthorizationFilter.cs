using Hangfire.Dashboard;

namespace CanPany.Infrastructure.Services;

/// <summary>
/// Authorization filter for Hangfire Dashboard
/// In production, implement proper authentication
/// </summary>
public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // For development: allow all
        // TODO: In production, implement proper authentication
        // Example: Check if user is authenticated and has admin role
        // var httpContext = context.GetHttpContext();
        // return httpContext.User.Identity?.IsAuthenticated == true && 
        //        httpContext.User.IsInRole("Admin");
        
        return true;
    }
}
