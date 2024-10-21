
using Microsoft.Extensions.Caching.Distributed;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly int _maxRequests;
    private readonly int _timeWindowMinutes;

    public RateLimitingMiddleware(RequestDelegate next, IDistributedCache cache, IConfiguration config)
    {
        _next = next;
        _cache = cache;
        _maxRequests = config.GetValue<int>("RateLimiting:MaxRequestsPerMinute");
        _timeWindowMinutes = config.GetValue<int>("RateLimiting:TimeWindowMinutes");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrEmpty(clientIp))
        {
            await _next(context);
            return;
        }

        var cacheKey = $"Request_Count_{clientIp}";
        var requestCountString = await _cache.GetStringAsync(cacheKey);
        var requestCount = string.IsNullOrEmpty(requestCountString) ? 0 : int.Parse(requestCountString);

        if (requestCount >= _maxRequests)
        {
            context.Response.StatusCode = 429;  // Too Many Requests
            await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
            return;
        }

        // Increment request count and update Redis cache
        requestCount++;
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_timeWindowMinutes)
        };
        await _cache.SetStringAsync(cacheKey, requestCount.ToString(), options);

        // Add response headers for rate limiting information
        context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = (_maxRequests - requestCount).ToString();
        context.Response.Headers["X-RateLimit-Reset"] = _timeWindowMinutes.ToString();

        await _next(context);
    }
}
