using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others {
	public class SalHermesWhatsappMedia {
		[JsonPropertyName("url")]
		public required string Url { get; set; }
	}
}
