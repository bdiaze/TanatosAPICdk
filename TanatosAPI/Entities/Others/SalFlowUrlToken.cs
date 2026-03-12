using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others {
	public class SalFlowUrlToken {
		[JsonPropertyName("token")]
		public required string Token { get; set; }

		[JsonPropertyName("url")]
		public required string Url { get; set; }
	}
}
