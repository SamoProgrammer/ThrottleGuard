![Project Image](https://github.com/SamoProgrammer/ThrottleGuard/blob/main/ProjectImage.webp)

# ThrottleGuard

**ThrottleGuard** is an ASP.NET Core middleware library that provides robust rate limiting and graceful degradation mechanisms to control API traffic and improve the user experience during high-traffic periods. With Redis support for distributed environments, it ensures scalable rate limiting across multiple instances.


[![NuGet](https://img.shields.io/nuget/v/ThrottleGuard)](https://www.nuget.org/packages/ThrottleGuard/)   [![NuGet](https://img.shields.io/nuget/dt/ThrottleGuard)](https://www.nuget.org/packages/ThrottleGuard/)


## Features

- **Rate Limiting**: Easily configure request limits per user/IP to prevent abuse.
- **Graceful Degradation**: Introduce response delays as the limit is approached to warn users and smooth the experience.
- **Highly Configurable**: Full configuration through `appsettings.json` for easy adjustments.
- **Redis Support**: Distributed caching with Redis to share state across multiple API servers.
- **Customizable Warnings**: Warn users via response headers when nearing the limit.

## Installation

### Prerequisites
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Redis](https://redis.io/) (optional for distributed rate limiting)

### Step-by-Step Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/your-username/ThrottleGuard.git
   cd ThrottleGuard
   ```

2. Add the necessary NuGet package for Redis caching (optional):

   ```bash
   dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
   ```

3. Configure Redis connection in `appsettings.json` (optional):

   ```json
   {
     "ConnectionStrings": {
       "Redis": "your_redis_connection_string"
     }
   }
   ```

4. Integrate **ThrottleGuard** into your ASP.NET Core application by adding the middleware in `Program.cs` or `Startup.cs`:

   ```csharp
   app.UseMiddleware<RateLimitingMiddleware>();
   ```

5. Customize the rate limiting behavior in `appsettings.json`:

   ```json
   {
     "RateLimiting": {
       "LimitPerMinute": 60,
       "CacheDurationSeconds": 30,
       "WarningThreshold": 50,
       "ResponseDelayMilliseconds": 1000
     }
   }
   ```

### Configuration Options

You can adjust the following options in the `appsettings.json` file:

- **LimitPerMinute**: Max requests allowed per user/IP per minute.
- **CacheDurationSeconds**: Duration for caching the request count.
- **WarningThreshold**: When to start issuing warnings to the user.
- **ResponseDelayMilliseconds**: Delay introduced when users reach the threshold.
- **Redis**: Connection string to the Redis server for distributed caching.

## Usage Example

1. After setting up the middleware, make multiple requests to your API endpoint.
2. Once the rate limit is reached, users will receive a `429 Too Many Requests` response.
3. Users approaching the limit will experience delays and see a response header:

   ```bash
   X-RateLimit-Warning: You are nearing the rate limit
   ```

4. If Redis is configured, rate limiting will be applied across all instances of your application.

## Example `appsettings.json`

```json
{
  "RateLimiting": {
    "LimitPerMinute": 100,
    "CacheDurationSeconds": 60,
    "WarningThreshold": 80,
    "ResponseDelayMilliseconds": 500
  },
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

## Graceful Degradation

Once the **WarningThreshold** is crossed, **ThrottleGuard** introduces a delay (`ResponseDelayMilliseconds`) before serving responses, giving users feedback without blocking them outright. This creates a smooth experience, especially for users who hit the rate limit unexpectedly.

## Contributing

Contributions are welcome! Feel free to submit a pull request or open an issue to suggest improvements or report bugs.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
