using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ThrottleGuard.Tests
{
    public class RateLimitingMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _next;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<IConfiguration> _configMock;

        public RateLimitingMiddlewareTests()
        {
            _next = new Mock<RequestDelegate>();
            _cacheMock = new Mock<IDistributedCache>();
            _configMock = new Mock<IConfiguration>();

            // Set up mock configuration values if needed
            _configMock.SetupGet(c => c["RateLimiting:LimitPerMinute"]).Returns("60");
            _configMock.SetupGet(c => c["RateLimiting:CacheDurationSeconds"]).Returns("30");
            _configMock.SetupGet(c => c["RateLimiting:WarningThreshold"]).Returns("50");
            _configMock.SetupGet(c => c["RateLimiting:ResponseDelayMilliseconds"]).Returns("1000");
        }

        [Fact]
        public async Task Should_Allow_Request_When_Under_Limit()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var middleware = new RateLimitingMiddleware(_next.Object, _cacheMock.Object, _configMock.Object);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            _next.Verify(next => next(httpContext), Times.Once);
        }

        [Fact]
        public async Task Should_Block_Request_When_Limit_Exceeded()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var middleware = new RateLimitingMiddleware(_next.Object, _cacheMock.Object, _configMock.Object);

            // Simulate exceeding the limit
            for (int i = 0; i < 100; i++)
            {
                await middleware.InvokeAsync(httpContext);
            }

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            Assert.Equal(StatusCodes.Status429TooManyRequests, httpContext.Response.StatusCode);
        }

    }
}
