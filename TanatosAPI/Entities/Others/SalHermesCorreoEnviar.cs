using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others {
	public class SalHermesCorreoEnviar {
		[JsonPropertyName("queueMessageId")]
		public required string QueueMessageId { get; set; }
	}
}
