namespace TanatosAPI.Entities.Others {
    public class RateLimitResult {
        public required bool Allowed { get; set; }
        public required int Remaining { get; set; }
        public required DateTimeOffset RetryAfter { get; set; }
    }
}
