using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others.Google {
    [ExcludeFromCodeCoverage]
    public class GoogleTokenResponse {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }
    }
}
