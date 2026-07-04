using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others.Google {
    [ExcludeFromCodeCoverage]
    public class GoogleAssessmentParams {
        [JsonPropertyName("event")]
        public required GoogleAssesmentEvent Event { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class  GoogleAssesmentEvent {
        [JsonPropertyName("siteKey")]
        public required string SiteKey { get; set; }

        [JsonPropertyName("expectedAction")]
        public required string ExpectedAction { get; set; }

        [JsonPropertyName("token")]
        public required string Token { get; set; }
    }
}
