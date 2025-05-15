using System.Collections.Concurrent;

namespace PollSpark.Services;

public interface IRateLimiter
{
    Task<bool> TryAcquireAsync(string? key = null, TimeSpan? window = null);
}

public class MemoryRateLimiter : IRateLimiter
{
    private readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimits = new();
    private readonly ILogger<MemoryRateLimiter> _logger;
    private readonly int _defaultLimit;
    private readonly TimeSpan _defaultWindow;

    public MemoryRateLimiter(
        ILogger<MemoryRateLimiter> logger,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _defaultLimit = configuration.GetValue<int>("RateLimiting:DefaultLimit", 100);
        _defaultWindow = TimeSpan.FromMinutes(
            configuration.GetValue<int>("RateLimiting:DefaultWindowMinutes", 1)
        );
    }

    public Task<bool> TryAcquireAsync(string? key = null, TimeSpan? window = null)
    {
        var rateKey = key ?? "global";
        var rateWindow = window ?? _defaultWindow;
        var now = DateTime.UtcNow;

        var rateLimit = _rateLimits.GetOrAdd(
            rateKey,
            _ => new RateLimitInfo(_defaultLimit, rateWindow)
        );

        // Clean up expired entries
        if (now - rateLimit.LastReset > rateWindow)
        {
            rateLimit.Reset(_defaultLimit, rateWindow);
        }

        // Check if we're within the rate limit
        if (rateLimit.Remaining > 0)
        {
            rateLimit.Decrement();
            return Task.FromResult(true);
        }

        _logger.LogWarning(
            "Rate limit exceeded for key {Key}. Limit: {Limit}, Window: {Window}",
            rateKey,
            _defaultLimit,
            rateWindow
        );
        return Task.FromResult(false);
    }

    private class RateLimitInfo
    {
        public int Remaining { get; private set; }
        public DateTime LastReset { get; private set; }
        private readonly int _limit;
        private readonly TimeSpan _window;

        public RateLimitInfo(int limit, TimeSpan window)
        {
            _limit = limit;
            _window = window;
            Reset(limit, window);
        }

        public void Reset(int limit, TimeSpan window)
        {
            Remaining = limit;
            LastReset = DateTime.UtcNow;
        }

        public void Decrement()
        {
            if (Remaining > 0)
            {
                Remaining--;
            }
        }
    }
}
