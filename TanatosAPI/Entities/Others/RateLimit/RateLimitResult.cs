using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.RateLimit {
	[ExcludeFromCodeCoverage]
	public class RateLimitResult {
        public required bool Allowed { get; set; }
        public required int Remaining { get; set; }
        public required DateTimeOffset RetryAfter { get; set; }
    }
}
