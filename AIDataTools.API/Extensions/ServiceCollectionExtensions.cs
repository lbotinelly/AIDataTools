using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace AIDataTools.API.Extensions;

/// <summary>
/// Extension methods for IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds diagnostics services to the service collection
    /// </summary>
    public static IServiceCollection AddDiagnostics(this IServiceCollection services)
    {
        // Add health checks
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddCheck("memory", () => 
            {
                var memoryInfo = GC.GetGCMemoryInfo();
                var memoryUsed = memoryInfo.TotalCommittedBytes;
                var memoryThreshold = 1024L * 1024L * 1024L; // 1 GB
                
                return memoryUsed < memoryThreshold 
                    ? HealthCheckResult.Healthy($"Memory usage: {memoryUsed / (1024 * 1024)} MB")
                    : HealthCheckResult.Degraded($"Memory usage: {memoryUsed / (1024 * 1024)} MB");
            });
        
        // Add activity source for distributed tracing
        services.AddSingleton(new ActivitySource("DataForgeAI.Api"));
        
        // Add metrics
        services.AddMetrics();
        
        return services;
    }
    
    /// <summary>
    /// Adds metrics services to the service collection
    /// </summary>
    private static IServiceCollection AddMetrics(this IServiceCollection services)
    {
        // Add metrics services
        services.AddSingleton<MetricsService>();
        
        return services;
    }
}

/// <summary>
/// Service for tracking metrics
/// </summary>
public class MetricsService
{
    private readonly Dictionary<string, Stopwatch> _timers = new();
    private readonly Dictionary<string, long> _counters = new();
    
    /// <summary>
    /// Start timing an operation
    /// </summary>
    public void StartTimer(string name)
    {
        if (!_timers.ContainsKey(name))
        {
            _timers[name] = new Stopwatch();
        }
        
        _timers[name].Start();
    }
    
    /// <summary>
    /// Stop timing an operation and return the elapsed milliseconds
    /// </summary>
    public long StopTimer(string name)
    {
        if (!_timers.ContainsKey(name))
        {
            return 0;
        }
        
        _timers[name].Stop();
        return _timers[name].ElapsedMilliseconds;
    }
    
    /// <summary>
    /// Increment a counter
    /// </summary>
    public void IncrementCounter(string name, long amount = 1)
    {
        if (!_counters.ContainsKey(name))
        {
            _counters[name] = 0;
        }
        
        _counters[name] += amount;
    }
    
    /// <summary>
    /// Get the value of a counter
    /// </summary>
    public long GetCounter(string name)
    {
        return _counters.ContainsKey(name) ? _counters[name] : 0;
    }
    
    /// <summary>
    /// Get all counters
    /// </summary>
    public Dictionary<string, long> GetAllCounters()
    {
        return new Dictionary<string, long>(_counters);
    }
}
