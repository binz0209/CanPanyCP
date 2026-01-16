namespace CanPany.Application.Interfaces.Interceptors;

/// <summary>
/// Performance monitoring interface
/// </summary>
public interface IPerformanceMonitor
{
    /// <summary>
    /// Start timing an operation
    /// </summary>
    IDisposable StartTiming(string operationName, Dictionary<string, object?>? metadata = null);
    
    /// <summary>
    /// Record execution time
    /// </summary>
    void RecordExecutionTime(string operationName, long milliseconds, Dictionary<string, object?>? metadata = null);
}
