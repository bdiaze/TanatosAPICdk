using Amazon.Lambda.Core;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Entities.Others;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
    public class RateLimitMiddleware(RequestDelegate next, IRateLimiter rateLimiter, IVariableEntornoHelper variableEntorno) {
        private readonly HashSet<string> RATE_LIMITS_SUBS_TO_SKIP = [.. variableEntorno.Obtener("RATE_LIMITS_SUBS_TO_SKIP").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

        public async Task InvokeAsync(HttpContext context) {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string path = context.Request.Path.ToString();
            if (path.StartsWith("/public/Suscripcion/flow-webhook/", StringComparison.OrdinalIgnoreCase)) {
                LambdaLogger.Log(
                    $"[RateLimitMiddleware] - [{stopwatch.ElapsedMilliseconds} ms] - [Skipped] - " +
                    $"Se salta rate limit para webhook de Flow.");
                await next(context);
                return;
            }


            string? sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sub != null && RATE_LIMITS_SUBS_TO_SKIP.Contains(sub)) {
                LambdaLogger.Log(
                    $"[RateLimitMiddleware] - [{stopwatch.ElapsedMilliseconds} ms] - [Skipped] - " +
                    $"Se salta rate limit para sub: {sub}.");
                await next(context);
                return;
            }


            bool isAuthenticated = !string.IsNullOrWhiteSpace(sub);
            (RateLimitGroup group, int maxRequests, TimeSpan window) = path switch { 
                string p when p.StartsWith("/public/Auth/", StringComparison.OrdinalIgnoreCase) => (RateLimitGroup.Auth, 10, TimeSpan.FromMinutes(1)),
                string _ when isAuthenticated => (RateLimitGroup.Authenticated, 100, TimeSpan.FromMinutes(1)),
                _ => (RateLimitGroup.Public, 20, TimeSpan.FromMinutes(1))
            };

            string ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim() ?? "UNKNOWN";
            string key = isAuthenticated
                ? $"USER:{sub}#{group}"
                : $"IP:{ip}#{group}";

            RateLimitContext rateLimitContext = new() { 
                Path = path,
                Method = context.Request.Method,
                IP = ip,
                Sub = sub
            };
            RateLimitResult result = await rateLimiter.CheckAsync(key, maxRequests, window, rateLimitContext);

            context.Response.Headers["X-RateLimit-Limit"] = maxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = result.Remaining.ToString();

            if (!result.Allowed) {
                int retryAfterSeconds = (int)Math.Ceiling(Math.Max(0, (result.RetryAfter - DateTimeOffset.UtcNow).TotalSeconds));
                context.Response.Headers.RetryAfter = retryAfterSeconds.ToString();
                context.Response.Headers["X-RateLimit-Reset"] = result.RetryAfter.ToUnixTimeSeconds().ToString();
                context.Response.StatusCode = 429;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("\"Has realizado demasiadas peticiones. Inténtalo de nuevo más tarde.\"");

                LambdaLogger.Log(
                    $"[RateLimitMiddleware] - [{stopwatch.ElapsedMilliseconds} ms] - [Not Allowed] - " +
                    $"Key: {key} - Max Requests: {maxRequests} - Remaining: {result.Remaining} - Retry After: {result.RetryAfter:O}.");
                return;
            }

            LambdaLogger.Log(
                $"[RateLimitMiddleware] - [{stopwatch.ElapsedMilliseconds} ms] - [Allowed] - " +
                $"Key: {key} - Max Requests: {maxRequests} - Remaining: {result.Remaining}.");
            await next(context);
        }
    }
}
