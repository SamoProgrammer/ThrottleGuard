![Project Image](https://github.com/SamoProgrammer/ThrottleGuard/blob/main/ProjectImage.webp)

# ThrottleGuard - Rate Limiting Middleware

**ThrottleGuard** is a customizable ASP.NET Core middleware for rate limiting API requests with added features like **graceful degradation**. It allows you to manage the rate of incoming requests, returning cached responses or adding artificial delays as users approach the limit, providing a smoother experience without abrupt rejections.

## Features
- **Rate Limiting**: Limit the number of requests per user/IP to prevent abuse.
- **Graceful Degradation**: Gradually slow down responses and provide warnings as users approach the rate limit.
- **Configurable**: Rate limit settings are fully configurable via `appsettings.json`.
- **Distributed Caching**: Supports Redis for distributed caching of request counts across instances.

## Getting Started

### Prerequisites

- [.NET 6](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- [Redis](https://redis.io/) for distributed caching

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/your-repo/ThrottleGuard.git
   cd ThrottleGuard
   ```

2. Install the required NuGet packages:

   ```bash
   dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
   ```

3. Update the **Redis** connection string in `appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "Redis": "your_redis_connection_string"
     }
   }
   ```

4. Configure your `Program.cs` or `Startup.cs` to use the rate limiting middleware:

   ```csharp
   app.UseMiddleware<RateLimitingMiddleware>();
   ```

### Configuration

You can configure rate limiting options in the `appsettings.json` file. Here are the default settings:

```json
{
  "RateLimiting": {
    "LimitPerMinute": 60,
    "CacheDurationSeconds": 30,
    "WarningThreshold": 50,
    "ResponseDelayMilliseconds": 1000
  },
  "ConnectionStrings": {
    "Redis": "your_redis_connection_string"
  }
}
```

- **LimitPerMinute**: Maximum number of requests allowed per minute per user/IP.
- **CacheDurationSeconds**: How long to keep request counts in cache.
- **WarningThreshold**: The number of requests after which warnings and delays are applied.
- **ResponseDelayMilliseconds**: The delay (in milliseconds) introduced for each request after hitting the warning threshold.

### How It Works

1. The middleware tracks the number of requests each user/IP makes per minute using Redis.
2. When a user exceeds the rate limit (`LimitPerMinute`), they receive a **429 - Too Many Requests** response.
3. When a user reaches the **WarningThreshold**, a delay is added to each request (`ResponseDelayMilliseconds`) and a warning header is sent (`X-RateLimit-Warning`).
4. If Redis is enabled, request counts are stored across instances for distributed systems.

### Usage

1. Run the application:

   ```bash
   dotnet run
   ```

2. Make requests to your API, and you will start seeing rate limiting in effect after the specified number of requests per minute.

### Example

To see rate limiting in action:

1. Configure the `LimitPerMinute` in `appsettings.json` to a low value, such as 5, for testing purposes.
2. Make multiple requests to the API (more than 5 per minute).
3. Observe the response with a `429` status code after the rate limit is reached.
4. If the request count exceeds the **WarningThreshold** but is below the rate limit, notice the artificial delay and the presence of the `X-RateLimit-Warning` header.

```bash
HTTP/1.1 200 OK
X-RateLimit-Warning: You are nearing the rate limit
```

### Distributed Caching with Redis

This middleware supports Redis as the distributed caching mechanism to store request counts. You can configure Redis in the `appsettings.json` file using the `ConnectionStrings` section:

```json
{
  "ConnectionStrings": {
    "Redis": "your_redis_connection_string"
  }
}
```

### Customizing the Middleware

If you'd like to extend the middleware with custom behaviors, such as different thresholds or more advanced degradation strategies, feel free to modify the `RateLimitingMiddleware` class.

### Contributing

Contributions to **ThrottleGuard** are welcome! If you have a bug report or feature request, feel free to open an issue or submit a pull request.

