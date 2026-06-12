namespace TanatosAPI.Entities.Others {
    public class RateLimitContext {
        public string? Path { get; set; }
        public string? Method { get; set; }
        public string? IP { get; set; }
        public string? Sub { get; set; }
    }

    public enum RateLimitGroup {
        Auth,
        Public,
        Authenticated
    }
}
