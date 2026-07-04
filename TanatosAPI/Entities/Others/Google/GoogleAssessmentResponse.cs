using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others.Google {
    [ExcludeFromCodeCoverage]
    public class GoogleAssessmentResponse {
        [JsonPropertyName("tokenProperties")]
        public GoogleTokenProperties? TokenProperties { get; set; }

        [JsonPropertyName("riskAnalysis")]
        public GoogleRiskAnalysis? RiskAnalysis { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class GoogleTokenProperties {
        [JsonPropertyName("valid")]
        public required bool Valid { get; set; }

        [JsonPropertyName("invalidReason")]
        public string? InvalidReason { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class GoogleRiskAnalysis {
        [JsonPropertyName("score")]
        public required float Score { get; set; }
    }
}
