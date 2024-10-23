using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly int _limitPerMinute;
    private readonly int _cacheDurationSeconds;
    private readonly int _warningThreshold;
    private readonly int _responseDelayMilliseconds;

    public RateLimitingMiddleware(RequestDelegate next, IDistributedCache cache, IConfiguration configuration)
    {
        _next = next;
        _cache = cache;

        // Load values from configuration
        _limitPerMinute = configuration.GetValue<int>("RateLimiting:LimitPerMinute");
        _cacheDurationSeconds = configuration.GetValue<int>("RateLimiting:CacheDurationSeconds");
        _warningThreshold = configuration.GetValue<int>("RateLimiting:WarningThreshold");
        _responseDelayMilliseconds = configuration.GetValue<int>("RateLimiting:ResponseDelayMilliseconds");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var cacheKey = $"RateLimit_{ipAddress}";
        var requestCount = await _cache.GetStringAsync(cacheKey);

        int.TryParse(requestCount, out int count);

        if (count >= _limitPerMinute)
        {
            // User exceeded the limit, block the request
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Rate limit exceeded. Please wait before making more requests.");
            return;
        }

        if (count >= _warningThreshold && count < _limitPerMinute)
        {
            // Graceful degradation: add artificial delay and send a warning
            await Task.Delay(_responseDelayMilliseconds); // Delay based on configuration

            // Set a custom header to warn about nearing the limit
            context.Response.Headers.Append("X-RateLimit-Warning", "You are nearing the rate limit");
        }

        // Proceed with the normal request processing
        await _next(context);

        // Increment request count and update cache
        count++;
        await _cache.SetStringAsync(cacheKey, count.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        });
    }
}
