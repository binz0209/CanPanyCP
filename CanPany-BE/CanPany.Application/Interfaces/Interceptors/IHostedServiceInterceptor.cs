namespace CanPany.Application.Interfaces.Interceptors;

/// <summary>
/// Hosted service interceptor interface for background jobs
/// </summary>
public interface IHostedServiceInterceptor
{
    /// <summary>
    /// Execute operation with interception (audit, performance, exception tracking)
    /// </summary>
    Task<T> ExecuteWithInterceptionAsync<T>(
        string serviceName,
        string methodName,
        Func<Task<T>> operation);

    /// <summary>
    /// Execute void operation with interception
    /// </summary>
    Task ExecuteWithInterceptionAsync(
        string serviceName,
        string methodName,
        Func<Task> operation);
}
