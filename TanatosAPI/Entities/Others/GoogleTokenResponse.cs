using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others {
    public class GoogleTokenResponse {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }
    }
}
