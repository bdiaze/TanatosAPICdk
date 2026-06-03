using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others {
    public class GoogleAssessmentResponse {
        [JsonPropertyName("tokenProperties")]
        public GoogleTokenProperties? TokenProperties { get; set; }

        [JsonPropertyName("riskAnalysis")]
        public GoogleRiskAnalysis? RiskAnalysis { get; set; }
    }

    public class GoogleTokenProperties {
        [JsonPropertyName("valid")]
        public required bool Valid { get; set; }

        [JsonPropertyName("invalidReason")]
        public string? InvalidReason { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }
    }

    public class GoogleRiskAnalysis {
        [JsonPropertyName("score")]
        public required float Score { get; set; }
    }
}
