using System.Security.Cryptography.X509Certificates;
using TanatosAPI.Entities.Others;

namespace TanatosAPI.Interfaces {
    public interface IRateLimiter {
        public Task<RateLimitResult> CheckAsync(string key, int maxRequests, TimeSpan window, RateLimitContext rateLimitContext);
    }
}
