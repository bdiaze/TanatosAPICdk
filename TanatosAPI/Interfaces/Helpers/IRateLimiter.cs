using System.Security.Cryptography.X509Certificates;
using TanatosAPI.Entities.Others.RateLimit;

namespace TanatosAPI.Interfaces.Helpers {
    public interface IRateLimiter {
        public Task<RateLimitResult> CheckAsync(string key, int maxRequests, TimeSpan window, RateLimitContext rateLimitContext);
    }
}
