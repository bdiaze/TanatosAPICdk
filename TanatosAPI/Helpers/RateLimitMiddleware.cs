using TanatosAPI.Entities.Others;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
    public class RateLimitMiddleware(RequestDelegate next, IRateLimiter rateLimiter) {
        public async Task InvokeAsync(HttpContext context) {
            string key = GetRateLimitKey(context);

            bool isPublic = context.Request.Path.StartsWithSegments("/public/");
            (int maxRequests, TimeSpan window) = isPublic
                ? (20, TimeSpan.FromMinutes(1))
                : (100, TimeSpan.FromMinutes(1));

            RateLimitResult result = await rateLimiter.CheckAsync(key, maxRequests, window);

            context.Response.Headers["X-RateLimit-Limit"] = maxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = result.Remaining.ToString();

            if (!result.Allowed) {
                int retryAfterSeconds = (int)Math.Ceiling(Math.Max(0, (result.RetryAfter - DateTimeOffset.UtcNow).TotalSeconds));
                context.Response.Headers.RetryAfter = retryAfterSeconds.ToString();
                context.Response.Headers["X-RateLimit-Reset"] = result.RetryAfter.ToUnixTimeSeconds().ToString();
                context.Response.StatusCode = 429;
                return;
            }

            await next(context);
        }

        private static string GetRateLimitKey(HttpContext context) {
            string? sub = context.User.FindFirst("sub")?.Value;
            if (sub is not null) {
                return $"USER:{sub}";
            }

            string ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim() ??
                context.Connection.RemoteIpAddress?.ToString() ??
                "UNKNOWN";
            return $"IP:{ip}";
        }
    }
}
